using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PassengerService.Data;
using PassengerService.Models;

namespace PassengerService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PassengerController : ControllerBase
    {
        private readonly PassengerContext _context;

        public PassengerController(PassengerContext context)
        {
            _context = context;
        }

        [HttpGet("{flightId}")]
        public async Task<ActionResult<IEnumerable<Passenger>>> GetPassengersByFlight(string flightId)
        {
            return await _context.Passengers.Where(p => p.FlightId == flightId).ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult> AddPassenger([FromBody] Passenger passenger)
        {
            _context.Passengers.Add(passenger);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetPassengersByFlight), new { flightId = passenger.FlightId }, passenger);
        }
    }
}