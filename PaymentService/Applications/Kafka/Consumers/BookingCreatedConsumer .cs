using Microsoft.Extensions.Options;
using PaymentService.Entities.Records;
using PaymentService.Application.Kafka.Consumers;
using Microsoft.Extensions.DependencyInjection;
using PaymentService.Applications.Kafka;
using PaymentService.Applications.Interfaces;

namespace PaymentService.Application.Kafka.Consumers
{
    public class BookingCreatedConsumer : KafkaBackgroundConsumerBase<BookingCreatedMessage>
    {
        private readonly ILogger<BookingCreatedConsumer> _logger;

        public BookingCreatedConsumer(
            IOptions<KafkaSettings> options,
            ILogger<BookingCreatedConsumer> logger,
            IServiceProvider serviceProvider
        )
            : base(options.Value.Topics[KafkaTopic.BookingCreated.ToString()], options.Value.BootstrapServers, logger, serviceProvider)
        {
            _logger = logger;
        }

        protected override Task HandleMessageAsync(BookingCreatedMessage? message, IServiceScope scope)
        {
            _logger.LogInformation("Received BookingCreatedMessage: {@message}", message);
            if (message == null)
                return Task.CompletedTask;

            var paymentActions = scope.ServiceProvider.GetRequiredService<IPaymentActions>();

            return paymentActions.HandleBookingCreatedAsync(message);
        }
    }
}
