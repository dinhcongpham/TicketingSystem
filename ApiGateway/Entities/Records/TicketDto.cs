namespace ApiGateway.Entities.Records
{
    public enum TicketStatus
    {
        Available,
        Reserved,
        Booked,
        Cancelled
    }
    public record TicketDto
    {
        public Guid Id { get; set; }
        public Guid EventId { get; set; }
        public int Index { get; set; }
        public decimal Price { get; set; }
        public TicketStatus Status { get; set; }
        public Guid? BookingId { get; set; }
    }
}
