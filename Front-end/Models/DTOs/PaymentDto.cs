namespace Front_end.Models.DTOs
{
    public class PaymentDto
    {
        public Guid Id { get; set; }
        public Guid BookingId { get; set; }
        public decimal Amount { get; set; }
        public string Email { get; set; } = default!;
        public PaymentStatus Status { get; set; } // (Processing, Paid, Failed, Refunded)
    }
}
