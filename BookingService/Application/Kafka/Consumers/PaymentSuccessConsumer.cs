using BookingService.Application.Interfaces;
using BookingService.Entities.Records;
using BookingService.Application.Kafka.Consumers;
using Microsoft.Extensions.Options; 

namespace BookingService.Application.Kafka.Consumers
{
    public class PaymentSuccessConsumer : KafkaBackgroundConsumerBase<PaymentSuccessMessage>
    {
        private readonly ILogger<EventCreatedConsumer> _logger;

        public PaymentSuccessConsumer(
            IOptions<KafkaSettings> options,
            ILogger<EventCreatedConsumer> logger,
            IServiceProvider serviceProvider
        )
            : base(options.Value.Topics[KafkaTopic.PaymentSuccess.ToString()], options.Value.BootstrapServers, logger, serviceProvider)
        {
            _logger = logger;
        }

        protected override Task HandleMessageAsync(PaymentSuccessMessage? message, IServiceScope scope)
        {
            if (message == null)
                return Task.CompletedTask;

            var bookingActions = scope.ServiceProvider.GetRequiredService<IBookingActions>();

            return bookingActions.HandlePaymentSuccessAsync(message);
        }
    }
}
