using CryptoPricesWithWebApi.ApiProxy;
using CryptoPricesWithWebApi.ApiProxy.Models;
using CryptoPricesWithWebApi.Data;
using Microsoft.EntityFrameworkCore;

namespace CryptoPricesWithWebApi.BackgroundServices
{
    public class CurrencyPriceService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<CurrencyPriceService> _logger;
        public CurrencyPriceService(IServiceScopeFactory scopeFactory, ILogger<CurrencyPriceService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using IServiceScope scope = _scopeFactory.CreateScope();
                    CurrencyDbContext dbContext = scope.ServiceProvider.GetRequiredService<CurrencyDbContext>();

                    TickerResponse tickerResponse = await TickerApiClient.GetTicker();//
                    List<PairData> pairDataList = await dbContext.Pairs.ToListAsync(cancellationToken: stoppingToken);

                    if (tickerResponse == null || pairDataList == null)
                    {
                        _logger.LogError("toUseTicker or pairDataList is null");
                    }
                    else
                    {
                        List<string> pairs = pairDataList.Select(p => p.Name).ToList();

                        IEnumerable<PairTickerDetail> matchingPairs = tickerResponse.Data.Where(p => pairs.Contains(p.Pair));

                        foreach (PairTickerDetail matchingPair in matchingPairs)
                        {
                            string pairSymbol = matchingPair.Pair;
                            PairPrice pricesToInsert = new()
                            {
                                Pairsymbol = matchingPair.Pair,
                                Price = matchingPair.Last,
                                Time = DateTime.Now,

                            };

                            dbContext.Prices.Add(pricesToInsert);
                            await dbContext.SaveChangesAsync(stoppingToken);

                            await RemoveOldPrices(pairSymbol, stoppingToken);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception");
                }

                DateTime currentTime = DateTime.Now;

                DateTime nextHour = currentTime.AddHours(1);
                nextHour = new DateTime(nextHour.Year, nextHour.Month, nextHour.Day, nextHour.Hour, 0, 0);

                TimeSpan delay = nextHour - currentTime;

                await Task.Delay(delay, stoppingToken);

                // await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }

        }
        private async Task RemoveOldPrices(string pairSymbol, CancellationToken stoppingToken)
        {
            using IServiceScope scope = _scopeFactory.CreateScope();
            CurrencyDbContext dbContext = scope.ServiceProvider.GetRequiredService<CurrencyDbContext>();
            List<PairPrice> priceList = await dbContext.Prices.ToListAsync(cancellationToken: stoppingToken);
            int count = priceList.Where(p => p.Pairsymbol == pairSymbol).Count();

            if (count <= 100)
            {
                return;
            }
            int recordsToRemove = count - 100;
            List<PairPrice> oldestPrices = priceList.OrderBy(t => t.Time).Take(recordsToRemove).ToList();

            if (oldestPrices == null)
            {
                return;
            }
            foreach (PairPrice oldestPrice in oldestPrices)
            {
                dbContext.Prices.Remove(oldestPrice);
            }

            await dbContext.SaveChangesAsync(stoppingToken);
        }
    }
}






