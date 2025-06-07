using Front_end.Models.DTOs;

namespace Front_end.Models.ViewModels
{
    public class EventListViewModel
    {
        public List<EventDto> Events { get; set; } = new();
        public string? SearchName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        // Pagination
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
    }
}
