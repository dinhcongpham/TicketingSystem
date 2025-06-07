using BookingService.Application.Kafka.Consumers;
using BookingService.Entities;
using BookingService.Entities.Records;

namespace BookingService.Application.Interfaces
{
    public interface IBookingActions
    {
        Task<List<Booking>?> GetAllBookingEntry(); 
        Task<ReserveTicketsResponse> ReserveTicketsAsync(ReserveTicketsRequest request);
        Task<Booking> CreateBookingEntry(ReserveTicketsRequest request);
        Task<Booking?> GetBookingById(Guid bookingId);
        Task HandlePaymentSuccessAsync(PaymentSuccessMessage message);
        Task HandleExpiredReservationsAsync();
        Task HandleExpiredBooking(Guid bookingId);
        Task CleanupReservations(List<Guid> ticketIds);
    }
}
