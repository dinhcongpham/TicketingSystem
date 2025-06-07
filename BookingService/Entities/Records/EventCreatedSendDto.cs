namespace BookingService.Entities.Records
{
    public record EventCreatedSendDto
    {
        public Guid Id { get; set; }
        public int NumSeat { get; set; }
    }
}
