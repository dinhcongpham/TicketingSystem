namespace BookingService.Entities.Records
{
    public record ReserveTicketsRequest
    {
        public Guid EventId { get; set; }
        public List<Guid> ListTicketsId { get; set; } = new List<Guid>();
        public int NumSeats { get; set; }
        public string Email { get; set; } = default!;
    }
}
