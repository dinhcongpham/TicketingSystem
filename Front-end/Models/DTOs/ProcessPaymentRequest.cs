namespace Front_end.Models.DTOs
{
    public record ProcessPaymentRequest
    {
        public Guid BookingId { get; set; }
        public string Email { get; set; } = default!;
        public int NumSeats { get; set; }
    }
}
