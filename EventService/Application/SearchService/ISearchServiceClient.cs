using EventService.Entities;

namespace EventService.Application.SearchService
{
    public interface ISearchServiceClient
    {
        Task<bool> CreateEventAsync(Event eventDto);
        Task<bool> UpdateEventAsync(Event eventDto);
        Task<bool> DeleteEventAsync(Guid eventId);
    }
}
