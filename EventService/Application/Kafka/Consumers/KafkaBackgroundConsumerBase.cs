using Confluent.Kafka;
using System.Text.Json;

namespace EventService.Application.Kafka.Consumers
{
    public abstract class KafkaBackgroundConsumerBase<T> : BackgroundService
    {
        private readonly string _topic;
        private readonly string _bootstrapServers;
        private readonly ILogger _logger;

        protected KafkaBackgroundConsumerBase(string topic, string bootstrapServers, ILogger logger)
        {
            _topic = topic;
            _bootstrapServers = bootstrapServers;
            _logger = logger;
        }

        protected abstract Task HandleMessageAsync(T? message);

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = _bootstrapServers,
                GroupId = $"{_topic}-consumer-group",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
            consumer.Subscribe(_topic);

            return Task.Run(() =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var result = consumer.Consume(stoppingToken);
                        var message = JsonSerializer.Deserialize<T>(result.Message.Value);
                        HandleMessageAsync(message);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error consuming from {_topic}");
                    }
                }

                consumer.Close();
            }, stoppingToken);
        }
    }
}
