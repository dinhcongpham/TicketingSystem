using BookingService.Application.Interfaces;

namespace BookingService.Application.Implementation
{
    // Background Service for Handling Expired Reservations
    public class ExpiredReservationCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ExpiredReservationCleanupService> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromMinutes(1); // Check every minute

        public ExpiredReservationCleanupService(
            IServiceProvider serviceProvider,
            ILogger<ExpiredReservationCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var bookingActions = scope.ServiceProvider.GetRequiredService<IBookingActions>();

                    await bookingActions.HandleExpiredReservationsAsync();

                    await Task.Delay(_interval, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in expired reservation cleanup service");
                    await Task.Delay(_interval, stoppingToken);
                }
            }
        }
    }
}
