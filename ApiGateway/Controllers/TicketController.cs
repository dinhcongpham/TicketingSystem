using ApiGateway.Application.Grpc.Clients;
using ApiGateway.Entities.Records;
using Microsoft.AspNetCore.Mvc;

namespace ApiGateway.Controllers
{
    public class TicketController : Controller
    {
        private readonly TicketGrpcClient _ticketGrpcClient;

        public TicketController(TicketGrpcClient ticketGrpcClient)
        {
            _ticketGrpcClient = ticketGrpcClient;
        }

        [HttpPost("/tickets/event/{id}")]
        public async Task<ActionResult<TicketDto>> GetAllTicketsByEventId(GetTicketsByEventIdRequest request)
        {
            var tickets = await _ticketGrpcClient.GetTicketsByEventAsync(request.EventId, request.NumSeat);
            return Ok(tickets);
        }
    }
}
