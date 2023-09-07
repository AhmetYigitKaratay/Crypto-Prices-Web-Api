using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CryptoPricesWithWebApi.Data;
using CryptoPricesWithWebApi.Models.Requests;

namespace CryptoPricesWithWebApi.Controllers
{
    [Route("api/pairs")]
    [ApiController]
    public class PairController : ControllerBase
    {
        private readonly CurrencyDbContext _context;

        public PairController(CurrencyDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok(await _context.Pairs.ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePairRequest createPairRequest)
        {
            if (_context.Pairs.Count() > 10)
            {
                string message = "It's not allowed to enter more than 10 data.";
                return StatusCode(406, message);
            }

            if(await _context.Pairs.AnyAsync(c => c.Name == createPairRequest.Name))
            {
                string message = "It's not allowed to enter the same data";
                return BadRequest(message);
            }

            PairData pairsToInsert = new() { Name = createPairRequest.Name };

            _context.Pairs.Add(pairsToInsert);

            await _context.SaveChangesAsync();

            return StatusCode(201);

        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromRoute] int id, UpdatePairRequest updatePairRequest)
        {  
            PairData existingPair = await _context.Pairs.FirstOrDefaultAsync(u => u.Id == id);

            if (await _context.Pairs.AnyAsync(u => u.Name == updatePairRequest.Name))
            {
                string message = "It's not allowed to enter the same data";
                return BadRequest(message);
            }

            if (existingPair != null)
            {
                existingPair.Name = updatePairRequest.Name;

                await _context.SaveChangesAsync();
            }
            return Ok();
        }         

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            PairData pairToDelete = await _context.Pairs.FirstOrDefaultAsync(pair => pair.Id == id);

            if (pairToDelete == null)
            {
                return NotFound();
            }

            _context.Pairs.Remove(pairToDelete);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
       
}


    
