using GrpcContracts;
using EventService.Entities.Records;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace BookingService.Application.Grpc.Clients
{
    public class BookingGrpcClient
    {
        private readonly TicketService.TicketServiceClient _client;

        public BookingGrpcClient(TicketService.TicketServiceClient client)
        {
            _client = client;
        }

        public async Task<List<TicketDto>> GetTicketsByEventAsync(Guid eventId, int numSeat)
        {
            try
            {
                var response = await _client.GetTicketsByEventAsync(new TicketRequest
                {
                    EventId = eventId.ToString(),
                    NumSeat = numSeat
                });

                var tickets = response.Tickets.Select(t => new TicketDto
                {
                    Id = Guid.Parse(t.Id),
                    EventId = Guid.Parse(t.EventId),
                    Index = t.Index,
                    Price = (decimal)t.Price,
                    Status = (TicketStatus)t.Status,
                    BookingId = string.IsNullOrEmpty(t.BookingId) ? null : Guid.Parse(t.BookingId)
                }).ToList();

                return tickets;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get tickets for event {eventId}: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteTicketsByEventAsync(Guid eventId, int numSeat)
        {
            try
            {
                var response = await _client.DeleteTicketsByEventAsync(new TicketRequest
                {
                    EventId = eventId.ToString(),
                    NumSeat = numSeat
                });
                return response.Status;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
