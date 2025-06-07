using EventService.Entities;

namespace EventService.Application.SearchService
{
    public class SearchServiceClient : ISearchServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<SearchServiceClient> _logger;

        public SearchServiceClient(HttpClient httpClient, ILogger<SearchServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<bool> CreateEventAsync(Event eventDto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/eventsearch/index", eventDto);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create event in search service");
                return false; // Don't fail the main operation
            }
        }

        public async Task<bool> UpdateEventAsync(Event eventDto)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/eventsearch/index/{eventDto.Id}", eventDto);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update event in search service");
                return false;
            }
        }

        public async Task<bool> DeleteEventAsync(Guid eventId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/eventsearch/index/{eventId}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete event in search service");
                return false;
            }
        }
    }
}
