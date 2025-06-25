using Front_end.Models.DTOs;

namespace Front_end.Services
{
    public interface IApiService
    {
        Task<List<EventDto>> GetEventsAsync();
        Task<EventSearchResponse> SearchEventsAsync(string? name, DateTime? startDate, DateTime? endDate, int page, int pageSize);
        Task<EventDto?> GetEventByIdAsync(Guid eventId);
        Task<VenueDto> GetVenueByEventIdAsync(Guid venueId);
        Task<List<TicketDto>> GetTicketsByEventIdAsync(Guid eventId, int numSeat);
        Task<TicketDto?> GetTicketByIdAsync(Guid ticketId);
        Task<ReserveTicketsResponse> CreateBookingAsync(Guid eventId, List<Guid> ticketIds, string email, int numSeat);
        Task<ProcessPaymentResponse> ProcessPaymentAsync(Guid paymentId, string email, int numSeats);
        Task<BookingDto?> GetBookingByIdAsync(Guid bookingId);
    }
}
