namespace ApiGateway.Entities.Records
{
    public record GetTicketsByEventIdRequest
    {
        public Guid EventId { get; set; }
        public int NumSeat { get; set; }
    }
}
