namespace Front_end.Services
{
    public static class ApiEndpoints
    {
        public static class Event
        {
            public const string GetAll = "/event";
            public const string Search = "/event/search";
            public static string GetById(Guid id) => $"/event/{id}";
        }

        public static class Venue
        {
            public const string GetAll = "/venue";
            public static string GetById(Guid id) => $"/venue/{id}";
        }
        public static class Ticket
        {
            public static string GetByEventId(Guid eventId) => $"/tickets/event/{eventId}";
            public static string GetById(Guid id) => $"/ticket/{id}";
        }

        public static class Booking
        {
            public const string Reserve = "/booking/reserve";
            public static string GetById(Guid id) => $"/booking/{id}";
        }

        public static class Payment
        {
            public static string Process(Guid id) => $"/payment/process/{id}";
        }
    }

}
