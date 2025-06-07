namespace Front_end.Models.DTOs
{
    public record TicketDto
    {
        public Guid Id { get; set; }
        public Guid EventId { get; set; }
        public int Index { get; set; }
        public decimal Price { get; set; }
        public TicketStatus Status { get; set; } // (Available, Reserved, Booked, Cancelled)
        public Guid? BookingId { get; set; }
    }
}
