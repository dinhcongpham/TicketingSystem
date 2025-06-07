using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingService.Entities
{
    public class Ticket
    {
        public Guid Id { get; set; }
        public Guid EventId { get; set; }
        public int Index { get; set; }
        public decimal Price { get; set; }
        public TicketStatus Status { get; set; }
        public Guid? BookingId { get; set; }
    }
}
