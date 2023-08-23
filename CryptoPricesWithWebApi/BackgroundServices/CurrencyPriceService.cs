using CryptoPricesWithWebApi.ApiProxy;
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
                try {
                    using IServiceScope scope = _scopeFactory.CreateScope();
                    CurrencyDbContext dbContext = scope.ServiceProvider.GetService<CurrencyDbContext>();

                    TickerResponse toUseTicker = await TickerApiClient.GetTicker();
                    List<PairData> pairDataList = await dbContext.Pairs.ToListAsync(cancellationToken: stoppingToken);

                    if (toUseTicker != null && pairDataList != null)
                    {
                        List<string> pairs = pairDataList.Select(p => p.Name).ToList();

                        IEnumerable<PairTickerDetail> matchingPairs = toUseTicker.Data.Where(p => pairs.Contains(p.Pair));

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

                            List<PairPrice> priceList = await dbContext.Prices.ToListAsync(cancellationToken: stoppingToken);
                            int count = priceList.Where(p => p.Pairsymbol == pairSymbol).Count();

                            if (count > 100)
                            {
                                PairPrice oldestPrice = priceList.OrderBy(t => t.Time).FirstOrDefault();

                                if (oldestPrice != null)
                                {
                                    dbContext.Prices.Remove(oldestPrice);  

                                    await dbContext.SaveChangesAsync(stoppingToken);
                                    
                                }                             
                            }                                                                         
                        }
                    }
                    else
                    {
                        _logger.LogError("toUseTicker or pairDataList is null");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception");
                }

                   /* DateTime currentTime = DateTime.Now;

                    DateTime nextHour = currentTime.AddHours(1);
                    nextHour = new DateTime(nextHour.Year, nextHour.Month, nextHour.Day, nextHour.Hour, 0, 0);
        
                    TimeSpan delay = nextHour - currentTime;

                    await Task.Delay(delay, stoppingToken);*/

                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
            
        }
    }
}






