using BookingService.Application.Interfaces;
using BookingService.Data;
using BookingService.Entities;
using GrpcContracts;
using Grpc.Core;
using Ticket = GrpcContracts.Ticket;

namespace BookingService.Application.Grpc.Servers
{
    public class TicketServiceImpl : TicketService.TicketServiceBase
    {
        private readonly ITicketActions _ticketActions;

        public TicketServiceImpl(ITicketActions ticketActions)
        {
            _ticketActions = ticketActions;
        }

        public override async Task<TicketResponse> GetTicketsByEvent(TicketRequest request, ServerCallContext context)
        {
            var tickets = await _ticketActions.GetAllTicketsByEventIdAsync(Guid.Parse(request.EventId), request.NumSeat);
            var response = new TicketResponse();

            response.Tickets.AddRange(tickets.Select(t => new Ticket
            {
                Id = t.Id.ToString(),
                EventId = t.EventId.ToString(),
                Index = t.Index,
                Price = (int)t.Price,
                Status = (int)t.Status,
                BookingId = t.BookingId?.ToString() ?? ""
            }));

            return response;
        }

        public override async Task<DeleteTicketResponseStatus> DeleteTicketsByEvent(TicketRequest request, ServerCallContext context)
        {
            var eventId = Guid.Parse(request.EventId);
            var result = await _ticketActions.DeleteTicketsByEventIdAsync(eventId, request.NumSeat);
            if (!result)
            {
                return new DeleteTicketResponseStatus { Status = false };
            }

            return new DeleteTicketResponseStatus { Status = true };
        }
    }
}
