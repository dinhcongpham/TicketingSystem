namespace Front_end.Models.DTOs
{
    public record VenueDto
    {
        public Guid Id { get; set; }
        public Guid EventId { get; set; }
        public string Name { get; set; } = default!;
        public string Location { get; set; } = default!;
        public int NumSeat { get; set; }
    }
}
