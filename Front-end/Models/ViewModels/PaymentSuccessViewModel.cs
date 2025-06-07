namespace Front_end.Models.ViewModels
{
    public record PaymentSuccessViewModel
    {
        public Guid? PaymentId { get; set; }
        public string Message { get; set; } = default!;
    }
}
