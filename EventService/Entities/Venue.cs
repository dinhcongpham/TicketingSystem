using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventService.Entities
{
    public class Venue
    {
        public Guid Id { get; set; }
        public Guid EventId { get; set; }
        public string Name { get; set; } = default!;
        public string Location { get; set; } = default!;
        public int NumSeat { get; set; }
    }
}
