using BookingService.Application.Interfaces;
using BookingService.Entities.Records;
using Microsoft.AspNetCore.Mvc;

namespace BookingService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingController : ControllerBase
    {
        private readonly IBookingActions _bookingActions;
        private readonly ILogger<BookingController> _logger;
        public BookingController(IBookingActions bookingActions, ILogger<BookingController> logger)
        {
            _bookingActions = bookingActions;
            _logger = logger;
        }

        [HttpPost("reserve")]
        public async Task<IActionResult> ReserveTickets([FromBody] ReserveTicketsRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors)
                              .Select(e => e.ErrorMessage)
                              .ToList();
                    _logger.LogWarning("Model binding failed: {Errors}", string.Join("; ", errors));
                    return BadRequest(ModelState);
                }

                var result = await _bookingActions.ReserveTicketsAsync(request);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ReserveTickets endpoint");
                return StatusCode(500, new ReserveTicketsResponse
                {
                    Success = false,
                    Message = "An error occurred while reserving tickets"
                });
            }
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetBookingById(Guid id)
        {
            try
            {
                var booking = await _bookingActions.GetBookingById(id);
                if (booking == null)
                {
                    return NotFound();
                }
                return Ok(booking);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetBookingById endpoint");
                return StatusCode(500, "An error occurred while retrieving the booking");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllBookingEntry()
        {
            try
            {
                var bookings = await _bookingActions.GetAllBookingEntry();
                if (bookings == null || !bookings.Any())
                {
                    return NotFound("No bookings found");
                }
                return Ok(bookings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAllBookingEntry endpoint");
                return StatusCode(500, "An error occurred while retrieving bookings");
            }
        }
    }
}
