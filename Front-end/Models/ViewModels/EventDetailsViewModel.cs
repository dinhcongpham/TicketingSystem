using Front_end.Models.DTOs;

namespace Front_end.Models.ViewModels
{
    public class EventDetailsViewModel
    {
        public EventDto Event { get; set; } = default!;
        public List<TicketDto> Tickets { get; set; } = new();
        public VenueDto Venue { get; set; } = default!;
        public List<Guid> SelectedTicketIds { get; set; } = new();
    }
}
