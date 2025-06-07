using Confluent.Kafka;
using EventService.Application.Kafka.Interfaces;
using Microsoft.Extensions.Options;
using System.Text.Json;
using static Confluent.Kafka.ConfigPropertyNames;

namespace BookingService.Application.Kafka.Producers
{
    public class KafkaProducer : IKafkaProducer
    {
        private readonly KafkaSettings _settings;
        private readonly ILogger<KafkaProducer> _logger;

        public KafkaProducer(IOptions<KafkaSettings> options, ILogger<KafkaProducer> logger)
        {
            _settings = options.Value;
            _logger = logger;
        }

        public async Task PublishAsync<T>(string topicName, T message)
        {
            try
            {
                var config = new ProducerConfig { BootstrapServers = _settings.BootstrapServers };

                using var producer = new ProducerBuilder<Null, string>(config).Build();
                var json = JsonSerializer.Serialize(message);

                var result = await producer.ProduceAsync(topicName, new Message<Null, string> { Value = json });

                _logger.LogInformation($"Produced to {topicName}: {result.Value}");
            }
            catch
            {
                _logger.LogError($"Failed to produce message to {topicName}.");
            }
        }
    }
}
