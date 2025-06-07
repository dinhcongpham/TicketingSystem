namespace BookingService.Entities.Records
{
    public record ReserveTicketsResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = default!;
        public Guid? BookingId { get; set; }
        public List<string> UnavailableTickets { get; set; } = new();
    }
}
