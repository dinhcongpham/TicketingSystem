using SearchService.Models;

namespace SearchService.Application
{
    public interface IEventSearchService
    {
        Task<bool> IndexEventAsync(Event eventEntity);
        Task<bool> UpdateEventAsync(Event eventEntity);
        Task<bool> DeleteEventAsync(Guid eventId);
        Task<EventSearchResponse> SearchEventsAsync(EventSearchRequest request);
        Task<bool> BulkIndexEventsAsync(IEnumerable<Event> events);
        Task<bool> CreateIndexAsync();
    }
}
