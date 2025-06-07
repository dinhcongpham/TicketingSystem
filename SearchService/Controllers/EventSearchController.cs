using Microsoft.AspNetCore.Mvc;
using SearchService.Application;
using SearchService.Models;

namespace SearchService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventSearchController : ControllerBase
    {
        private readonly IEventSearchService _searchService;
        private readonly ILogger<EventSearchController> _logger;

        public EventSearchController(IEventSearchService searchService, ILogger<EventSearchController> logger)
        {
            _searchService = searchService;
            _logger = logger;
        }

        [HttpPost("search")]
        public async Task<ActionResult<EventSearchResponse>> Search([FromBody] EventSearchRequest request)
        {
            try
            {
                var response = await _searchService.SearchEventsAsync(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while searching events");
                return StatusCode(500, "An error occurred while searching events");
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<EventSearchResponse>> SearchGet(
            [FromQuery] string? query = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? sortBy = "relevance",
            [FromQuery] string? sortOrder = "asc")
        {
            var request = new EventSearchRequest
            {
                Query = query,
                StartDate = startDate,
                EndDate = endDate,
                Page = page,
                PageSize = pageSize,
                SortBy = sortBy,
                SortOrder = sortOrder
            };

            return await Search(request);
        }

        [HttpPost("index")]
        public async Task<ActionResult> IndexEvent([FromBody] Event eventEntity)
        {
            try
            {
                var success = await _searchService.IndexEventAsync(eventEntity);
                return success ? Ok() : StatusCode(500, "Failed to index event");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while indexing event {EventId}", eventEntity.Id);
                return StatusCode(500, "An error occurred while indexing event");
            }
        }

        [HttpPut("index/{id}")]
        public async Task<ActionResult> UpdateEvent(Guid id, [FromBody] Event eventEntity)
        {
            try
            {
                eventEntity.Id = id;
                var success = await _searchService.UpdateEventAsync(eventEntity);
                return success ? Ok() : StatusCode(500, "Failed to update event");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating event {EventId}", id);
                return StatusCode(500, "An error occurred while updating event");
            }
        }

        [HttpDelete("index/{id}")]
        public async Task<ActionResult> DeleteEvent(Guid id)
        {
            try
            {
                var success = await _searchService.DeleteEventAsync(id);
                return success ? Ok() : StatusCode(500, "Failed to delete event");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting event {EventId}", id);
                return StatusCode(500, "An error occurred while deleting event");
            }
        }

        [HttpPost("bulk-index")]
        public async Task<ActionResult> BulkIndexEvents([FromBody] List<Event> events)
        {
            try
            {
                var success = await _searchService.BulkIndexEventsAsync(events);
                return success ? Ok() : StatusCode(500, "Failed to bulk index events");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while bulk indexing events");
                return StatusCode(500, "An error occurred while bulk indexing events");
            }
        }

        [HttpPost("create-index")]
        public async Task<ActionResult> CreateIndex()
        {
            try
            {
                var success = await _searchService.CreateIndexAsync();
                return success ? Ok() : StatusCode(500, "Failed to create index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating index");
                return StatusCode(500, "An error occurred while creating index");
            }
        }
    }
}
