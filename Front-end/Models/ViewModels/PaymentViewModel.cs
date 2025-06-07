using Front_end.Models.DTOs;

namespace Front_end.Models.ViewModels
{
    public class PaymentViewModel
    {
        public Guid BookingId { get; set; }
        public string Email { get; set; } = default!;
        public EventDto Event { get; set; } = default!;
        public List<TicketDto> BookedTickets { get; set; } = new();
        public decimal TotalAmount { get; set; }
        public int NumSeats { get; set; }
    }
}
