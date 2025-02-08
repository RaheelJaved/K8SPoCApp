using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PassengerService.Data;
using PassengerService.Messaging;
using PassengerService.Models;

namespace PassengerService.Controllers
{
    [ApiController]
    [Route("api/passengers")]
    public class PassengerController : ControllerBase
    {
        private readonly PassengerContext _context;
        private readonly ILogger<PassengerController> _logger;

        private readonly IRabbitMQService _rabbitMQService;

        public PassengerController(PassengerContext context, ILogger<PassengerController> logger, IRabbitMQService rabbitMQService)
        {
            _context = context;
            _logger = logger;
            _rabbitMQService = rabbitMQService;

        }

        //GET /api/passengers/flight/{flightNumber}
        [HttpGet("flight/{flightNumber}")]
        public async Task<ActionResult<IEnumerable<Passenger>>> GetPassengersByFlight(string flightNumber)
        {
            var passengers = await _context.Passengers
                .Where(p => p.FlightNumber == flightNumber)
                .ToListAsync();

            return Ok(passengers);
        }

        //GET /api/passengers/pnr/{pnr}
        // Now returns all passengers that share the given PNR.
        [HttpGet("pnr/{pnr}")]
        public async Task<ActionResult<IEnumerable<Passenger>>> GetPassengersByPNR(string pnr)
        {
            var passengers = await _context.Passengers
                .Where(p => p.PNR == pnr)
                .ToListAsync();

            if (passengers == null || passengers.Count == 0)
            {
                return NotFound($"No passengers found with PNR {pnr}.");
            }

            return Ok(passengers);
        }


        //POST /api/passengers/checkin
        [HttpPost("checkin")]
        public async Task<IActionResult> CheckInPassenger([FromBody] PassengerActionRequest request)
        {
            if (request.PassengerId <= 0)
            {
                return BadRequest("PassengerId is required and must be greater than 0.");
            }

            var passenger = await _context.Passengers.FindAsync(request.PassengerId);
            if (passenger == null)
            {
                return NotFound($"Passenger with ID {request.PassengerId} not found.");
            }

            passenger.Status = "CheckedIn";
            _context.Passengers.Update(passenger);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Passenger with ID {PassengerId} checked in.", request.PassengerId);
            await _rabbitMQService.SendMessageAsync("PassengerCheckedIn", passenger);
            return Ok(passenger);
        }

        //POST /api/passengers/offload
        [HttpPost("offload")]
        public async Task<IActionResult> OffloadPassenger([FromBody] PassengerActionRequest request)
        {
            if (request.PassengerId <= 0)
            {
                return BadRequest("PassengerId is required and must be greater than 0.");
            }

            var passenger = await _context.Passengers.FindAsync(request.PassengerId);
            if (passenger == null)
            {
                return NotFound($"Passenger with ID {request.PassengerId} not found.");
            }

            passenger.Status = "Offloaded";
            _context.Passengers.Update(passenger);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Passenger with ID {PassengerId} offloaded.", request.PassengerId);

            await _rabbitMQService.SendMessageAsync("PassengerOffloaded", passenger);
            return Ok(passenger);
        }

        //POST /api/passengers/board
        [HttpPost("board")]
        public async Task<IActionResult> BoardPassenger([FromBody] PassengerActionRequest request)
        {
            if (request.PassengerId <= 0)
            {
                return BadRequest("PassengerId is required and must be greater than 0.");
            }

            var passenger = await _context.Passengers.FindAsync(request.PassengerId);
            if (passenger == null)
            {
                return NotFound($"Passenger with ID {request.PassengerId} not found.");
            }

            passenger.Status = "Boarded";
            _context.Passengers.Update(passenger);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Passenger with ID {PassengerId} boarded.", request.PassengerId);
            await _rabbitMQService.SendMessageAsync("PassengerBoarded", passenger);
            return Ok(passenger);
        }
    }
}
