namespace PaymentService.Entities.Records
{
    public record ProcessPaymentResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = default!;
        public Guid? PaymentId { get; set; }
    }
}
