namespace EventService.Entities.Records
{
    public record EventDto
    {
        public string NameEvent { get; init; } = default!;
        public string Description { get; init; } = default!;
        public DateTime StartTime { get; init; }
        public DateTime EndTime { get; init; }
        public string NameVenue { get; init; } = default!;
        public string Location { get; init; } = default!;
        public int NumSeat { get; init; }
    }
}
