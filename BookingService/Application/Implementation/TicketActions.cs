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
        private const string TICKET_RESERVE_PREFIX = "ticket-reserve:";
        public TicketActions(BookingDbContext context, ICacheService cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task BulkCreateTicketsAsync(Guid eventId, int numSeats)
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

        public async Task<bool> BulkUpdateTicketsAsync(IEnumerable<Guid> ticketsId, TicketStatus newStatus)
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


        public async Task<bool> DeleteTicketAsync(Guid id)
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

        public async Task<bool> DeleteTicketsByEventIdAsync(Guid eventId, int numSeat)
        {
            try
            {
                if (numSeat <= 0)
                {
                    return true;
                }

                var tickets = await _context.Tickets
                    .Where(t => t.EventId == eventId && t.Index >= 1 && t.Index <= numSeat)
                    .ToListAsync();

                if (tickets.Count == 0)
                {
                    return true;
                }

                _context.Tickets.RemoveRange(tickets);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<List<Ticket>> GetAllTicketsByEventIdAsync(Guid eventId, int numSeats)
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

        public async Task<Ticket> GetTicketByIdAsync(Guid id)
        {
            var ticket = await _context.Tickets
                .FirstOrDefaultAsync(t => t.Id == id);
            if (ticket == null)
            {
                throw new KeyNotFoundException($"Ticket with ID {id} not found.");
            }
            return ticket;
        }

        public async Task<bool> UpdateTicketAsync(Guid id, TicketDto ticketDto)
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
    }
}
