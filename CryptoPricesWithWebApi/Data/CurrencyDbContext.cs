using Microsoft.EntityFrameworkCore;

namespace CryptoPricesWithWebApi.Data;

public class CurrencyDbContext : DbContext
{
    public CurrencyDbContext()
    {
    }
    public CurrencyDbContext(DbContextOptions<CurrencyDbContext> options)
        : base(options)
    {
    }
    public DbSet<PairData> Pairs { get; set; }
    public DbSet<PairPrice> Prices { get; set; }

}
