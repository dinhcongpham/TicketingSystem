using Front_end.Models.DTOs;
using System.Text;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net;

namespace Front_end.Services
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ApiService> _logger;
        private readonly ApiUrlBuilder _apiUrlBuilder;

        public ApiService(
            HttpClient httpClient, 
            ILogger<ApiService> logger,
            ApiUrlBuilder apiUrlBuilder)
        {
            _httpClient = httpClient;
            _logger = logger;
            _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            _apiUrlBuilder = apiUrlBuilder;
        }

        public async Task<List<EventDto>> GetEventsAsync()
        {
            var url = _apiUrlBuilder.Build(ApiEndpoints.Event.GetAll);
            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<EventDto>>(json) ?? new List<EventDto>();
            }
            return new List<EventDto>();
        }

        public async Task<EventSearchResponse> SearchEventsAsync(
            string? name,
            DateTime? startDate,
            DateTime? endDate,
            int page = 1,
            int pageSize = 9)
        {
            var queryParams = new List<string>();
            if (!string.IsNullOrEmpty(name))
                queryParams.Add($"query={Uri.EscapeDataString(name)}");
            if (startDate.HasValue)
                queryParams.Add($"startDate={startDate.Value:yyyy-MM-dd}");
            if (endDate.HasValue)
                queryParams.Add($"endDate={endDate.Value:yyyy-MM-dd}");

            // Pagination parameters
            queryParams.Add($"page={page}");
            queryParams.Add($"pageSize={pageSize}");

            var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            var url = _apiUrlBuilder.Build(ApiEndpoints.Event.Search);
            var response = await _httpClient.GetAsync($"{url}{queryString}");

            if (!response.IsSuccessStatusCode)
            {
                return new EventSearchResponse(); // Return empty result
            }

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<EventSearchResponse>(json) ?? new EventSearchResponse();
        }


        public async Task<EventDto?> GetEventByIdAsync(Guid eventId)
        {
            var url = _apiUrlBuilder.Build(ApiEndpoints.Event.GetById(eventId));
            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<EventDto>(json);
            }
            return null;
        }

        public async Task<VenueDto> GetVenueByEventIdAsync(Guid venueId)
        {
            var url = _apiUrlBuilder.Build(ApiEndpoints.Venue.GetById(venueId));
            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<VenueDto>(json) ?? throw new InvalidOperationException("Failed to retrieve venue");
            }
            throw new HttpRequestException($"Failed to retrieve venue with ID {venueId}");
        }

        public async Task<List<TicketDto>> GetTicketsByEventIdAsync(Guid eventId)
        {
            var url = _apiUrlBuilder.Build(ApiEndpoints.Ticket.GetByEventId(eventId));
            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<TicketDto>>(json) ?? new List<TicketDto>();
            }
            return new List<TicketDto>();
        }

        public async Task<TicketDto?> GetTicketByIdAsync(Guid id)
        {
            var url = _apiUrlBuilder.Build(ApiEndpoints.Ticket.GetById(id));
            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<TicketDto>(json) ?? throw new InvalidOperationException("Failed to retrieve ticket");
            }
            throw new HttpRequestException($"Failed to retrieve ticket with ID {id}");
        }

        public async Task<ReserveTicketsResponse> CreateBookingAsync(Guid eventId, List<Guid> ticketIds, string email, int numSeat)
        {
            var bookingRequest = new 
            {
                EventId = eventId,
                ListTicketsId = ticketIds,
                Email = email,
                NumSeats = numSeat
            };

            // Serialize the anonymous object to JSON and wrap it in StringContent
            var content = new StringContent(JsonConvert.SerializeObject(bookingRequest), Encoding.UTF8, "application/json");

            var url = _apiUrlBuilder.Build(ApiEndpoints.Booking.Reserve);
            var response = await _httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Booking failed with status code {response.StatusCode}");
                _logger.LogError($"Response content: {await response.Content.ReadAsStringAsync()}");
                var error = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Booking failed: {response.StatusCode} - {error}");
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ReserveTicketsResponse>(responseJson)
                   ?? throw new InvalidOperationException("Failed to parse booking response");
        }



        public async Task<ProcessPaymentResponse> ProcessPaymentAsync(Guid id, string email, int numSeats)
        {
            var processRequest = new ProcessPaymentRequest
            {
                BookingId = id,
                Email = email,
                NumSeats = numSeats
            };

            var content = new StringContent(JsonConvert.SerializeObject(processRequest), Encoding.UTF8, "application/json");

            var url = _apiUrlBuilder.Build(ApiEndpoints.Payment.Process(id));
            var response = await _httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Payment failed: {response.StatusCode} - {error}");
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ProcessPaymentResponse>(responseJson)
                   ?? throw new InvalidOperationException("Failed to parse payment response");
        }

        public async Task<BookingDto?> GetBookingByIdAsync(Guid id)
        {
            try
            {
                var url = _apiUrlBuilder.Build(ApiEndpoints.Booking.GetById(id));
                var response = await _httpClient.GetAsync(url);

                if (response.StatusCode == HttpStatusCode.NotFound)
                    return null;

                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<BookingDto>(json);
            }
            catch (Exception ex)
            {
                // Optional: add logging here
                throw new InvalidOperationException($"Failed to get booking with ID: {id}", ex);
            }
        }
    }
}
