using Nest;
using SearchService.Models;

namespace SearchService.Application
{
    public class EventSearchService : IEventSearchService
    {
        private readonly IElasticClient _elasticClient;
        private readonly ILogger<EventSearchService> _logger;
        private const string IndexName = "events";

        public EventSearchService(IElasticClient elasticClient, ILogger<EventSearchService> logger)
        {
            _elasticClient = elasticClient;
            _logger = logger;
        }

        public async Task<bool> CreateIndexAsync()
        {
            var indexExists = await _elasticClient.Indices.ExistsAsync(IndexName);
            if (indexExists.Exists)
                return true;

            var createIndexResponse = await _elasticClient.Indices.CreateAsync(IndexName, c => c
                .Map<EventSearchDocument>(m => m
                    .Properties(p => p
                        .Keyword(k => k.Name(n => n.Id))
                        .Text(t => t
                            .Name(n => n.Name)
                            .Analyzer("standard")
                            .Fields(f => f
                                .Keyword(k => k.Name("keyword"))
                            )
                        )
                        .Text(t => t
                            .Name(n => n.Description)
                            .Analyzer("standard")
                        )
                        .Text(t => t
                            .Name(n => n.SearchText)
                            .Analyzer("standard")
                        )
                        .Keyword(k => k.Name(n => n.VenueId))
                        .Date(d => d.Name(n => n.StartTime))
                        .Date(d => d.Name(n => n.EndTime))
                    )
                )
            );

            if (!createIndexResponse.IsValid)
            {
                _logger.LogError("Failed to create index: {Error}", createIndexResponse.OriginalException?.Message);
                return false;
            }

            _logger.LogInformation("Index {IndexName} created successfully", IndexName);
            return true;
        }

        public async Task<bool> IndexEventAsync(Event eventEntity)
        {
            var document = MapToSearchDocument(eventEntity);

            var response = await _elasticClient.IndexAsync(document, idx => idx.Index(IndexName));

            if (!response.IsValid)
            {
                _logger.LogError("Failed to index event {EventId}: {Error}",
                    eventEntity.Id, response.OriginalException?.Message);
                return false;
            }

            return true;
        }

        public async Task<bool> UpdateEventAsync(Event eventEntity)
        {
            return await IndexEventAsync(eventEntity); // Elasticsearch will update if document exists
        }

        public async Task<bool> DeleteEventAsync(Guid eventId)
        {
            var response = await _elasticClient.DeleteAsync<EventSearchDocument>(eventId, d => d.Index(IndexName));

            if (!response.IsValid && response.Result != Result.NotFound)
            {
                _logger.LogError("Failed to delete event {EventId}: {Error}",
                    eventId, response.OriginalException?.Message);
                return false;
            }

            return true;
        }

        public async Task<bool> BulkIndexEventsAsync(IEnumerable<Event> events)
        {
            var documents = events.Select(MapToSearchDocument);

            var bulkResponse = await _elasticClient.BulkAsync(b => b
                .Index(IndexName)
                .IndexMany(documents)
            );

            if (!bulkResponse.IsValid)
            {
                _logger.LogError("Bulk indexing failed: {Error}", bulkResponse.OriginalException?.Message);
                return false;
            }

            return true;
        }

        public async Task<EventSearchResponse> SearchEventsAsync(EventSearchRequest request)
        {
            var searchRequest = BuildSearchRequest(request);

            var response = await _elasticClient.SearchAsync<EventSearchDocument>(searchRequest);

            if (!response.IsValid)
            {
                _logger.LogError("Search failed: {Error}", response.OriginalException?.Message);
                return new EventSearchResponse();
            }

            var totalCount = response.Total;
            var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

            return new EventSearchResponse
            {
                Events = response.Documents.ToList(),
                TotalCount = totalCount,    
                Page = request.Page,
                PageSize = request.PageSize,
                TotalPages = totalPages,
                HasNextPage = request.Page < totalPages,
                HasPreviousPage = request.Page > 1
            };
        }

        private SearchRequest<EventSearchDocument> BuildSearchRequest(EventSearchRequest request)
        {
            var searchRequest = new SearchRequest<EventSearchDocument>(IndexName)
            {
                From = (request.Page - 1) * request.PageSize,
                Size = request.PageSize
            };

            // Build query
            var queries = new List<QueryContainer>();

            // Text search on name, description, and combined search text
            if (!string.IsNullOrWhiteSpace(request.Query))
            {
                queries.Add(Query<EventSearchDocument>.MultiMatch(m => m
                    .Query(request.Query)
                    .Fields(f => f
                        .Field(p => p.Name, boost: 2.0) // Boost name matches
                        .Field(p => p.Description)
                        .Field(p => p.SearchText)
                    )
                    .Type(TextQueryType.BestFields)
                    .Fuzziness(Fuzziness.Auto)
                ));
            }

            // Date range filter
            if (request.StartDate.HasValue || request.EndDate.HasValue)
            {
                queries.Add(Query<EventSearchDocument>.DateRange(d => d
                    .Field(f => f.StartTime)
                    .GreaterThanOrEquals(request.StartDate)
                    .LessThanOrEquals(request.EndDate)
                ));
            }

            // Combine queries
            if (queries.Any())
            {
                searchRequest.Query = queries.Count == 1
                    ? queries.First()
                    : Query<EventSearchDocument>.Bool(b => b.Must(queries.ToArray()));
            }
            else
            {
                searchRequest.Query = Query<EventSearchDocument>.MatchAll();
            }

            // Add sorting
            searchRequest.Sort = GetSortDescriptors(request.SortBy, request.SortOrder);

            return searchRequest;
        }

        private IList<ISort> GetSortDescriptors(string? sortBy, string? sortOrder)
        {
            var sortDescriptors = new List<ISort>();
            var ascending = sortOrder?.ToLower() != "desc";

            switch (sortBy?.ToLower())
            {
                case "name":
                    sortDescriptors.Add(new FieldSort { Field = "name.keyword", Order = ascending ? SortOrder.Ascending : SortOrder.Descending });
                    break;
                case "date":
                    sortDescriptors.Add(new FieldSort { Field = "startTime", Order = ascending ? SortOrder.Ascending : SortOrder.Descending });
                    break;
                case "relevance":
                default:
                    sortDescriptors.Add(new FieldSort { Field = "_score", Order = SortOrder.Descending });
                    sortDescriptors.Add(new FieldSort { Field = "startTime", Order = SortOrder.Ascending });
                    break;
            }

            return sortDescriptors;
        }

        private EventSearchDocument MapToSearchDocument(Event eventEntity)
        {
            return new EventSearchDocument
            {
                Id = eventEntity.Id,
                Name = eventEntity.Name,
                Description = eventEntity.Description,
                VenueId = eventEntity.VenueId,
                StartTime = eventEntity.StartTime,
                EndTime = eventEntity.EndTime,
                SearchText = $"{eventEntity.Name} {eventEntity.Description}".ToLower()
            };
        }
    }
}
