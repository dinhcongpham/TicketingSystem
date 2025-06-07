namespace EventService.Application.Kafka.Interfaces
{
    public interface IKafkaProducer
    {
        Task PublishAsync<T>(string topicName, T message);
    }
}
