namespace SearchService.Models
{
    public class EventSearchRequest
    {
        public string? Query { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; } = "relevance"; // relevance, date, name
        public string? SortOrder { get; set; } = "asc"; // asc, desc
    }
}
