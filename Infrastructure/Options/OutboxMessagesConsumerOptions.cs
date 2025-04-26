using Shared.Settings;

namespace Infrastructure.Options;

public class OutboxMessagesConsumerOptions
{
    public static readonly string SectionName = "BackgroundServices:OutboxMessagesConsumer";

    public RabbitMQOptions RabbitMQ { get; set; }
}
