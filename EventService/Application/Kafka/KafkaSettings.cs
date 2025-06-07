namespace EventService.Application.Kafka
{
    public class KafkaSettings
    {
        public string BootstrapServers { get; set; } = default!;
        public Dictionary<string, string> Topics { get; set; } = new();
    }
}
