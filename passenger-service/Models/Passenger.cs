using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PassengerService.Models
{
    public class Passenger
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string PNR { get; set; }
        public required string FlightNumber { get; set; }
        public required string Status { get; set; }
    }

}