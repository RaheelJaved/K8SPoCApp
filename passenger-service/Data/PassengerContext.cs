using Microsoft.EntityFrameworkCore;
using PassengerService.Models;

namespace PassengerService.Data
{
    public class PassengerContext : DbContext
    {
        public PassengerContext(DbContextOptions<PassengerContext> options) : base(options) { }

        public DbSet<Passenger> Passengers { get; set; }
    }
}