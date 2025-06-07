using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaymentService.Application.Kafka.Consumers;
using PaymentService.Applications.Interfaces;
using PaymentService.Entities.Records;

namespace PaymentService.Applications.Kafka.Consumers
{
    public class BookingExpiredConsumer : KafkaBackgroundConsumerBase<BookingExpiredMessage>
    {
        private readonly ILogger<BookingExpiredConsumer> _logger;

        public BookingExpiredConsumer(
            IOptions<KafkaSettings> options,
            ILogger<BookingExpiredConsumer> logger,
            IServiceProvider serviceProvider
        )
            : base(options.Value.Topics[KafkaTopic.BookingCreated.ToString()], options.Value.BootstrapServers, logger, serviceProvider)
        {
            _logger = logger;
        }

        protected override Task HandleMessageAsync(BookingExpiredMessage? message, IServiceScope scope)
        {
            _logger.LogInformation("Received BookingExpriedMessage: {@message}", message);
            if (message == null)
                return Task.CompletedTask;

            var paymentActions = scope.ServiceProvider.GetRequiredService<IPaymentActions>();

            return paymentActions.HandleBookingExpriedAsync(message);
        }
    }
}
