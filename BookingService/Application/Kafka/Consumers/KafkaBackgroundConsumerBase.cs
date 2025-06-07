using Confluent.Kafka;
using System.Text.Json;

namespace BookingService.Application.Kafka.Consumers
{
    public abstract class KafkaBackgroundConsumerBase<T> : BackgroundService
    {
        private readonly string _topic;
        private readonly string _bootstrapServers;
        private readonly ILogger _logger;
        private readonly IServiceProvider _serviceProvider;

        protected KafkaBackgroundConsumerBase(
            string topic,
            string bootstrapServers,
            ILogger logger,
            IServiceProvider serviceProvider)
        {
            _topic = topic;
            _bootstrapServers = bootstrapServers;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected abstract Task HandleMessageAsync(T? message, IServiceScope scope);

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = _bootstrapServers,
                GroupId = $"{_topic}-consumer-group",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            return Task.Run(async () =>
            {
                using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
                consumer.Subscribe(_topic);

                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var result = consumer.Consume(stoppingToken);
                        var message = JsonSerializer.Deserialize<T>(result.Message.Value);

                        // Create a new scope for each message
                        using var scope = _serviceProvider.CreateScope();
                        await HandleMessageAsync(message, scope);
                    }
                    catch (OperationCanceledException)
                    {
                        // Expected when cancellation is requested
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error consuming from {_topic}");

                        // Optional: Add delay to prevent tight error loops
                        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                    }
                }
            }, stoppingToken);
        }
    }
}