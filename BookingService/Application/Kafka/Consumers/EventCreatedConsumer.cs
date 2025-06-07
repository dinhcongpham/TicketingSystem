using BookingService.Application.Interfaces;
using BookingService.Application.Kafka;
using BookingService.Entities;
using Microsoft.Extensions.Options;
using BookingService.Entities.Records;
using BookingService.Application.Kafka.Consumers;
using Microsoft.Extensions.DependencyInjection;

namespace BookingService.Application.Kafka.Consumers
{
    public class EventCreatedConsumer : KafkaBackgroundConsumerBase<EventCreatedSendDto>
    {
        private readonly ILogger<EventCreatedConsumer> _logger;

        public EventCreatedConsumer(
            IOptions<KafkaSettings> options,
            ILogger<EventCreatedConsumer> logger,
            IServiceProvider serviceProvider
        )
            : base(options.Value.Topics[KafkaTopic.EventCreated.ToString()], options.Value.BootstrapServers, logger, serviceProvider)
        {
            _logger = logger;
        }

        protected override Task HandleMessageAsync(EventCreatedSendDto? message, IServiceScope scope)
        {
            if (message == null)
                return Task.CompletedTask;

            var ticketActions = scope.ServiceProvider.GetRequiredService<ITicketActions>();

            return ticketActions.BulkCreateTicketsAsync(message.Id, message.NumSeat);
        }
    }
}
