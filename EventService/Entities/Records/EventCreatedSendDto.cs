namespace EventService.Entities.Records
{
    public record EventCreatedSendDto
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public int NumSeat { get; init; }
    }
}
