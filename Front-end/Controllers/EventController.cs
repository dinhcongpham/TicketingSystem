using Front_end.Models.DTOs;
using Front_end.Models.ViewModels;
using Front_end.Services;
using Microsoft.AspNetCore.Mvc;

namespace Front_end.Controllers
{
    public class EventController : Controller
    {
        private readonly IApiService _apiService;
        private readonly ILogger<EventController> _logger;

        public EventController(IApiService apiService, ILogger<EventController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        [Route("event/{id}")]
        public async Task<IActionResult> Details(Guid id)
        {
            var eventDto = await _apiService.GetEventByIdAsync(id);
            if (eventDto == null)
            {
                return NotFound();
            }

            var venue = await _apiService.GetVenueByEventIdAsync(id);
            var tickets = await _apiService.GetTicketsByEventIdAsync(id, venue.NumSeat);

            var viewModel = new EventDetailsViewModel
            {
                Event = eventDto,
                Tickets = tickets,
                Venue = venue
            };

            return View(viewModel);
        }

        [HttpPost]
        [Route("event/{id}/booking/reserve")]
        public async Task<IActionResult> Book(Guid id, string email, int numSeat, List<Guid> selectedTicketIds)
        {
            if (!selectedTicketIds.Any())
            {
                TempData["Error"] = "Please select at least one ticket.";
                return RedirectToAction("Details", new { id });
            }

            if (string.IsNullOrEmpty(email))
            {
                TempData["Error"] = "Please provide your email address.";
                return RedirectToAction("Details", new { id });
            }

            try
            {
                var bookingResponse = await _apiService.CreateBookingAsync(id, selectedTicketIds, email, numSeat);

                if (!bookingResponse.Success)
                {
                    TempData["Error"] = bookingResponse.Message;
                    if (bookingResponse.UnavailableTickets.Any())
                    {
                        TempData["UnavailableTickets"] = string.Join(", ", bookingResponse.UnavailableTickets);
                    }
                    return RedirectToAction("Details", new { id });
                }

                // Store booking information in TempData for the payment page
                TempData["BookingId"] = bookingResponse.BookingId;
                TempData["Email"] = email;
                TempData["EventId"] = id;

                return RedirectToAction("Payment", new { id = bookingResponse.BookingId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to create booking. Please try again.";
                _logger.LogError(ex, "Error creating booking for event {EventId}", id);
                return RedirectToAction("Details", new { id });
            }
        }

        [Route("payment/{id}")]
        public async Task<IActionResult> Payment(Guid id)
        {
            try
            {
                // Get booking details from API
                _logger.LogInformation("Loading payment information for booking ID: {BookingId}", id);
                var booking = await _apiService.GetBookingByIdAsync(id);
                if (booking == null)
                {
                    TempData["Error"] = "Booking not found.";
                    return RedirectToAction("Index", "Home");
                }
                if (booking.CreatedAt < DateTime.UtcNow.AddMinutes(-6))
                {
                    TempData["Error"] = "Payment session expired";
                    return RedirectToAction("Payment", new { id });
                }

                // Get event details
                var ticket = await _apiService.GetTicketByIdAsync(booking.TicketIds[0]);
                var eventDto = await _apiService.GetEventByIdAsync(ticket.EventId);
                if (eventDto == null)
                {
                    TempData["Error"] = "Event not found.";
                    return RedirectToAction("Index", "Home");
                }

                // Get ticket details for the booking
                var venue = await _apiService.GetVenueByEventIdAsync(eventDto.Id);
                var allTickets = await _apiService.GetTicketsByEventIdAsync(eventDto.Id, venue.NumSeat);
                var bookedTickets = allTickets.Where(t => booking.TicketIds.Contains(t.Id)).ToList();

                // Calculate total amount
                var totalAmount = bookedTickets.Sum(t => t.Price);

                var viewModel = new PaymentViewModel
                {
                    BookingId = id,
                    Email = booking.Email,
                    Event = eventDto,
                    BookedTickets = bookedTickets,
                    TotalAmount = totalAmount,
                    NumSeats = venue.NumSeat,
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to load payment information. Please try again.";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        [Route("payment/process/{id}")]
        public async Task<IActionResult> ProcessPayment(Guid id, string email, int numSeats)
        {
            try
            {
                _logger.LogInformation("Processing payment for booking ID: {BookingId}", id);
                var paymentResponse = await _apiService.ProcessPaymentAsync(id, email, numSeats);

                if (paymentResponse.Success)
                {
                    TempData["Success"] = paymentResponse.Message;
                    TempData["PaymentId"] = paymentResponse.PaymentId;
                    return RedirectToAction("PaymentSuccess", new { id = paymentResponse.PaymentId });
                }
                else
                {
                    TempData["Error"] = paymentResponse.Message;
                    return RedirectToAction("Payment", new { id });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment for booking ID: {BookingId}", id);
                TempData["Error"] = "Payment processing failed. Please try again.";
                return RedirectToAction("Payment", new { id });
            }
        }


        [Route("payment/success/{id}")]
        public async Task<IActionResult> PaymentSuccess(Guid? id)
        {
            var viewModel = new PaymentSuccessViewModel
            {
                PaymentId = id,
                Message = TempData["Success"]?.ToString() ?? "Payment completed successfully!"
            };

            return View(viewModel);
        }
    }
}
