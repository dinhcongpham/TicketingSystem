using EventService.Entities;
using EventService.Entities.Records;

namespace EventService.Application.Interfaces
{
    public interface IEventActions
    {
        Task<List<Event>> GetEventsAsync();
        Task<Event?> GetEventByIdAsync(Guid id);
        Task<Venue?> GetVenueByEventIdAsync(Guid id);
        Task<List<TicketDto>> GetAllTicketsByEventIDAsync(Guid id);
        Task<Event> CreateEventAsync(EventDto eventDto);
        Task<Event?> UpdateEventAsync(Guid id, EventDto eventDto);
        Task<bool> DeleteEventAsync(Guid id);
    }
}
