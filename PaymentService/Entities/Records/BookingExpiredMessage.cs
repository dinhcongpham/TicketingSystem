namespace PaymentService.Entities.Records
{
    public record BookingExpiredMessage
    {
        public Guid BookingId { get; set; }
    }
}
