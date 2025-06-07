using BookingService.Entities.Records;
using BookingService.Entities;

namespace BookingService.Application.Interfaces
{
    public interface ITicketActions
    {
        Task BulkCreateTicketsAsync(Guid eventId, int numSeats);
        Task<bool> BulkUpdateTicketsAsync(IEnumerable<Guid> ticketsId, TicketStatus status);
        Task<Ticket> GetTicketByIdAsync(Guid id);
        Task<bool> DeleteTicketAsync(Guid id);
        Task<bool> UpdateTicketAsync(Guid id, TicketDto ticketDto);
        Task<bool> DeleteTicketsByEventIdAsync(Guid eventId, int numSeat);
        Task<List<Ticket>> GetAllTicketsByEventIdAsync(Guid id, int numSeat);
    }
}
