using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingService.Entities
{
    public class Booking
    {
        public Guid Id { get; set; }
        public List<Guid> TicketIds { get; set; } = new();
        public BookingStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ConfirmedAt { get; set; }
        public Guid? PaymentId { get; set; }
        public string Email { get; set; } = default!;
    }
}
