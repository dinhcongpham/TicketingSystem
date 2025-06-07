using BookingService.Application.Interfaces;
using BookingService.Entities;
using BookingService.Entities.Records;
using BookingService.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BookingService.Application.Implementation
{
    public class TicketActions : ITicketActions
    {
        private readonly BookingDbContext _context;
        private readonly ICacheService _cache;
        private readonly ILogger<TicketActions> _logger;
        private const string TICKET_RESERVE_PREFIX = "ticket-reserve:";

        public TicketActions(BookingDbContext context, ICacheService cache, ILogger<TicketActions> logger)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
        }

        public async Task BulkCreateTicketsAsync(Guid eventId, int numSeats)
        {
            try
            {
                var tickets = new List<Ticket>(numSeats);

                for (int i = 1; i <= numSeats; i++)
                {
                    tickets.Add(new Ticket
                    {
                        Id = Guid.NewGuid(),
                        EventId = eventId,
                        Index = i,
                        Price = 100000,
                        Status = TicketStatus.Available,
                        BookingId = null
                    });
                }

                await _context.Tickets.AddRangeAsync(tickets);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to bulk create tickets for event {EventId}", eventId);
                throw;
            }
        }

        public async Task<bool> BulkUpdateTicketsAsync(IEnumerable<Guid> ticketsId, TicketStatus newStatus)
        {
            try
            {
                var tickets = await _context.Tickets
                    .Where(t => ticketsId.Contains(t.Id))
                    .ToListAsync();

                if (tickets.Count == 0)
                    return false;

                tickets.ForEach(t => t.Status = newStatus);

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to bulk update tickets");
                return false;
            }
        }

        public async Task<bool> DeleteTicketAsync(Guid id)
        {
            try
            {
                var ticket = await _context.Tickets.FindAsync(id);
                if (ticket == null)
                {
                    return false;
                }

                _context.Tickets.Remove(ticket);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete ticket with ID {TicketId}", id);
                return false;
            }
        }

        public async Task<bool> DeleteTicketsByEventIdAsync(Guid eventId, int numSeat)
        {
            try
            {
                if (numSeat <= 0)
                    return true;

                var tickets = await _context.Tickets
                    .Where(t => t.EventId == eventId && t.Index >= 1 && t.Index <= numSeat)
                    .ToListAsync();

                if (tickets.Count == 0)
                    return true;

                _context.Tickets.RemoveRange(tickets);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete tickets for event {EventId}", eventId);
                return false;
            }
        }

        public async Task<List<Ticket>> GetAllTicketsByEventIdAsync(Guid eventId, int numSeats)
        {
            try
            {
                var cacheKey = $"tickets:{eventId}:1-{numSeats}";

                var tickets = await _context.Tickets
                    .Where(t => t.EventId == eventId && t.Index >= 1 && t.Index <= numSeats)
                    .OrderBy(t => t.Index)
                    .ToListAsync();

                await _cache.SetAsync(cacheKey, tickets, TimeSpan.FromMinutes(2));

                foreach (var ticket in tickets)
                {
                    var reservedKey = $"{TICKET_RESERVE_PREFIX}{ticket.Id}";
                    if (await _cache.ExistsAsync(reservedKey))
                    {
                        ticket.Status = TicketStatus.Reserved;
                    }
                }

                return tickets;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get all tickets for event {EventId}", eventId);
                return new List<Ticket>();
            }
        }

        public async Task<Ticket> GetTicketByIdAsync(Guid id)
        {
            try
            {
                var ticket = await _context.Tickets
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (ticket == null)
                {
                    throw new KeyNotFoundException($"Ticket with ID {id} not found.");
                }

                return ticket;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get ticket with ID {TicketId}", id);
                return new Ticket();
            }
        }

        public async Task<bool> UpdateTicketAsync(Guid id, TicketDto ticketDto)
        {
            try
            {
                var ticket = await _context.Tickets.FindAsync(id);
                if (ticket == null)
                {
                    return false;
                }

                ticket.Price = ticketDto.Price;
                ticket.Status = ticketDto.Status;

                _context.Tickets.Update(ticket);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update ticket with ID {TicketId}", id);
                return false;
            }
        }
    }
}
