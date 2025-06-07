namespace Front_end.Models.DTOs
{
    public class BookingDto
    {
        public Guid Id { get; set; }
        public List<Guid> TicketIds { get; set; } = new();
        public BookingStatus Status { get; set; } // (pending, confirmed, cancelled, expired)
        public DateTime CreatedAt { get; set; }
        public DateTime? ConfirmedAt { get; set; }
        public Guid? PaymentId { get; set; }
        public string Email { get; set; } = default!;
    }
}
