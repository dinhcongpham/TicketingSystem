namespace PaymentService.Entities.Records
{
    public record PaymentSuccessMessage
    {
        public Guid BookingId { get; set; }
        public Guid PaymentId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaidAt { get; set; }
        public int NumSeats { get; set; } = 0;
    }
}
