using BookingService.Application.Interfaces;
using BookingService.Application.Kafka;
using BookingService.Data;
using BookingService.Entities;
using BookingService.Entities.Records;
using Confluent.Kafka;
using EventService.Application.Kafka.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BookingService.Application.Implementation
{
    public class BookingActions : IBookingActions
    {
        private readonly ICacheService _redisService;
        private readonly IKafkaProducer _kafkaProducer;
        private readonly ITicketActions _ticketActions;
        private readonly BookingDbContext _db;
        private readonly ILogger<BookingActions> _logger;
        private const string TICKET_RESERVE_PREFIX = "ticket-reserve:";
        private const int RESERVATION_TIMEOUT_MINUTES = 5;

        public BookingActions(
            ICacheService redisService,
            IKafkaProducer kafkaProducer,
            ITicketActions ticketActions,
            BookingDbContext db,
            ILogger<BookingActions> logger)
        {
            _redisService = redisService;
            _kafkaProducer = kafkaProducer;
            _ticketActions = ticketActions;
            _db = db;
            _logger = logger;
        }

        public async Task<List<Booking>?> GetAllBookingEntry()
        {
            try
            {
                return await _db.Bookings.ToListAsync();
            }
            catch (Exception ex)
            {
                // Log exception
                return null;
            }
        }   

        public async Task<ReserveTicketsResponse> ReserveTicketsAsync(ReserveTicketsRequest request)
        {
            try
            {
                var unavailableTickets = new List<string>();
                foreach (var id in request.ListTicketsId)
                {
                    var reserveKey = $"{TICKET_RESERVE_PREFIX}{id}";
                    if (await _redisService.ExistsAsync(reserveKey))
                    {
                        unavailableTickets.Add(id.ToString());
                    }
                }

                if (unavailableTickets.Any())
                {
                    return new ReserveTicketsResponse
                    {
                        Success = false,
                        Message = "Some tickets are already reserved by other users",
                        UnavailableTickets = unavailableTickets
                    };
                }

                // Reserve tickets in Redis with TTL
                var reservationExpiry = TimeSpan.FromMinutes(RESERVATION_TIMEOUT_MINUTES);
                var reservationTasks = request.ListTicketsId.Select(async id =>
                {
                    var reserveKey = $"{TICKET_RESERVE_PREFIX}{id}";
                    return await _redisService.SetAsync(reserveKey, "reserved", reservationExpiry);
                });

                var reservationResults = await Task.WhenAll(reservationTasks);
                if (!reservationResults.All(r => r))
                {
                    // Cleanup any successful reservations
                    await CleanupReservations(request.ListTicketsId);
                    return new ReserveTicketsResponse
                    {
                        Success = false,
                        Message = "Failed to reserve tickets. Please try again."
                    };
                }

                // Create booking entry with pending status
                var bookingEntry = await CreateBookingEntry(request);
                if (bookingEntry == null)
                {
                    await CleanupReservations(request.ListTicketsId);
                    return new ReserveTicketsResponse
                    {
                        Success = false,
                        Message = "Failed to create booking entry. Please try again."
                    };
                }

                // Phase 3: Send booking created message to payment service via Kafka
                var totalAmount = request.ListTicketsId.Count * 10000;
                var bookingCreatedMessage = new BookingCreatedMessage
                {
                    BookingId = bookingEntry.Id,
                    TotalAmount = totalAmount,
                    Email = request.Email
                };

                await _kafkaProducer.PublishAsync(KafkaTopic.BookingCreated.ToString(), bookingCreatedMessage);

                return new ReserveTicketsResponse
                {
                    Success = true,
                    Message = "Tickets reserved successfully",
                    BookingId = bookingEntry.Id 
                };
            }
            catch (Exception ex)
            {
                await CleanupReservations(request.ListTicketsId);
                throw;
            }
        }

        public async Task<Booking> CreateBookingEntry(ReserveTicketsRequest request)
        {
            try
            {
                // Create booking entry with pending status
                var booking = new Booking
                {
                    Id = Guid.NewGuid(),
                    TicketIds = request.ListTicketsId,
                    Status = BookingStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                    Email = request.Email
                };
                await _db.AddAsync(booking);
                await _db.SaveChangesAsync();
                return booking;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<Booking?> GetBookingById(Guid bookingId)
        {
            try
            {
                return await _db.Bookings.FindAsync(bookingId);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task HandlePaymentSuccessAsync(PaymentSuccessMessage message)
        {
            try
            {
                _logger.LogInformation("Handling payment success for booking ID: {BookingId}", message.BookingId);
                if (message == null || message.BookingId == Guid.Empty || string.IsNullOrEmpty(message.BookingId.ToString()))
                {
                    return;
                }
                // Get booking
                var booking = await GetBookingById(message.BookingId);
                if (booking == null)
                {
                    return;
                }

                var tickets = await _ticketActions.GetTicketByIdAsync(booking.TicketIds[0]);

                // Update booking status
                booking.Status = BookingStatus.Confirmed;
                booking.ConfirmedAt = DateTime.UtcNow;
                booking.PaymentId = message.PaymentId;
                _db.Bookings.Update(booking);
                await _db.SaveChangesAsync();

                // Update tickets status to booked
                await _ticketActions.BulkUpdateTicketsAsync(booking.TicketIds, TicketStatus.Booked);

                // Clear Redis reservations
                var reserveKeys = booking.TicketIds.Select(id => $"{TICKET_RESERVE_PREFIX}{id}");
                await _redisService.DeleteMultipleAsync(reserveKeys);

                var cacheKey = $"tickets:{tickets.EventId}:1-{message.NumSeats}";
                _logger.LogInformation("Removing cache for key: {CacheKey}", cacheKey);
                await _redisService.RemoveAsync(cacheKey);

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task HandleExpiredReservationsAsync()
        {
            try
            {
                var expirationThreshold = DateTime.UtcNow.AddMinutes(RESERVATION_TIMEOUT_MINUTES);
                var expiredBookings = await _db.Bookings
                            .Where(b => b.Status == BookingStatus.Pending && b.CreatedAt <= expirationThreshold)
                            .ToListAsync();

                foreach (var booking in expiredBookings)
                {
                    await HandleExpiredBooking(booking.Id);
                }
            }
            catch (Exception ex)
            {
            }
        }

        public async Task HandleExpiredBooking(Guid bookingId)
        {
            try
            {
                var booking = await GetBookingById(bookingId);
                if (booking == null || booking.Status != BookingStatus.Pending)
                    return;

                // Update booking status to expired
                booking.Status = BookingStatus.Expired;
                _db.Bookings.Update(booking);
                await _db.SaveChangesAsync();

                // Clear Redis reservations
                await CleanupReservations(booking.TicketIds);

            }
            catch (Exception ex)
            {
            }
        }

        public async Task CleanupReservations(List<Guid> ticketIds)
        {
            try
            {
                _logger.LogInformation("Cleaning up reservations for tickets: {TicketIds}", string.Join(", ", ticketIds));
                var reserveKeys = ticketIds.Select(id => $"{TICKET_RESERVE_PREFIX}{id}").ToList();
                await _redisService.DeleteMultipleAsync(reserveKeys);
            }
            catch (Exception ex)
            {

            }
        }
    }
}
