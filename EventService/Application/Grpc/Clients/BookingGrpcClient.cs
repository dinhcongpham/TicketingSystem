using GrpcContracts;
using EventService.Entities.Records;

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

        public async Task<bool> DeleteTicketsByEventAsync(Guid eventId, int numSeat)
        {
            var response = await _client.DeleteTicketsByEventAsync(new TicketRequest
            {
                EventId = eventId.ToString(),
                NumSeat = numSeat
            });
            return response.Status;
        }
    }
}
