namespace FluxoCaixa.Infrastructure.Kafka;

public interface IKafkaConsumer
{
    Task StartConsumingAsync(CancellationToken cancellationToken);
}
