namespace Front_end.Models.DTOs
{
    public record ReserveTicketsRequest
    {
        public Guid EventId { get; set; }
        public List<Guid> ListTicketsId { get; set; } = new();
        public int NumSeats { get; set; }
        public string Email { get; set; } = default!;
    }
}
