using ApiGateway.Entities.Records;
using GrpcContracts;

namespace ApiGateway.Application.Grpc.Clients
{
    public class TicketGrpcClient
    {
        private readonly TicketService.TicketServiceClient _client;

        public TicketGrpcClient(TicketService.TicketServiceClient client)
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
    }
}
