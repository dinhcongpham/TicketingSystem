using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventService.Data;
using EventService.Application.Interfaces;
using EventService.Entities.Records;

namespace EventService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventController : Controller
    {
        private readonly EventDbContext _context;
        private readonly IEventActions _eventAction;
        private readonly ILogger<EventController> _logger;

        public EventController(
            EventDbContext context, 
            IEventActions eventAction,
            ILogger<EventController> logger
        )
        {
            _context = context;
            _eventAction = eventAction;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetEvents()
        {
            var events = await _context.Events.ToListAsync();
            return Ok(events);  
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEventsByID(Guid id)
        {
            var eventItem = await _eventAction.GetEventByIdAsync(id);
            if (eventItem == null)
            {
                _logger.LogError($"[ERROR] Event with ID {id} not found.");
                return NotFound();
            }
            return Ok(eventItem);
        }

        [HttpGet("venue/{id}")]
        public async Task<IActionResult> GetVenueByEventID(Guid id)
        {
            var venue = await _eventAction.GetVenueByEventIdAsync(id);
            if (venue == null)
            {
                _logger.LogError($"[ERROR] Venue for event with ID {id} not found.");
                return NotFound();
            }
            return Ok(venue);
        }

        [HttpGet("tickets/{id}")]
        public async Task<IActionResult> GetAllTicketsByEventID(Guid id)
        {
            var tickets = await _eventAction.GetAllTicketsByEventIDAsync(id);
            return Ok(tickets);
        }

        [HttpPost]
        public async Task<IActionResult> Create(EventDto eventDto)
        {
            var newEvent = await _eventAction.CreateEventAsync(eventDto);

            return Ok(newEvent);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, EventDto eventDto)
        {
            var eventItem = await _eventAction.UpdateEventAsync(id, eventDto);
            return Ok(eventItem);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _eventAction.DeleteEventAsync(id);
            if (!result)
            {
                return NotFound();
            }
            return Ok();
        }
    }
}
