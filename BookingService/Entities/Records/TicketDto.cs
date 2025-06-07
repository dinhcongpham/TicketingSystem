namespace BookingService.Entities.Records
{
    public record TicketDto
    {
        public Guid EventId { get; set; }
        public int Index { get; set; }
        public decimal Price { get; set; }
        public TicketStatus Status { get; set; }
        public Guid? BookingId { get; set; }
    }
}
