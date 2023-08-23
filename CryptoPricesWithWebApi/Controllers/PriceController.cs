using CryptoPricesWithWebApi.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CryptoPricesWithWebApi.Controllers
{
    [Route("api/prices")]
    [ApiController]
    public class PriceController : ControllerBase
    {
        private readonly CurrencyDbContext _context;

        public PriceController(CurrencyDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetPrices()
        {
            return Ok(await _context.Prices.OrderBy(t => t.Time).ToListAsync());
        }
    }
}
