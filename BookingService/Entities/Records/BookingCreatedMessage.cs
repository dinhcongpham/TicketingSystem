namespace BookingService.Entities.Records
{
    public record BookingCreatedMessage
    {
        public Guid BookingId { get; set; }
        public decimal TotalAmount { get; set; }
        public string Email { get; set; } = default!;
    }
}
