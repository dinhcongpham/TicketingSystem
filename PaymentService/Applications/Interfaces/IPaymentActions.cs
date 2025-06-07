using PaymentService.Entities;
using PaymentService.Entities.Records;

namespace PaymentService.Applications.Interfaces
{
    public interface IPaymentActions
    {
        Task<List<Payment>> GetAllPaymentsAsync();
        Task HandleBookingCreatedAsync(BookingCreatedMessage message);
        Task<ProcessPaymentResponse> ProcessPaymentAsync(ProcessPaymentRequest request);
        Task<Payment> GetPaymentByBookingIdAsync(Guid bookingId);
        Task<Payment> CreatePaymentAsync(BookingCreatedMessage message);
        Task<Payment?> GetPaymentByIdAsync(Guid paymentId);
        Task<Payment?> UpdatePaymentAsync(Payment payment);
    }
}
