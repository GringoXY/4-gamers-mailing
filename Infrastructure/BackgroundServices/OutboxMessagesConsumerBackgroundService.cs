using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Contracts.Dtos.OutboxMessage;
using Infrastructure.Options;
using Shared.Repositories;
using Shared.Entities;

namespace Infrastructure.BackgroundServices;

public class OutboxMessagesConsumerBackgroundService : BackgroundService
{
    private readonly ILogger<OutboxMessagesConsumerBackgroundService> _logger;
    private IConnection _connection;
    private IChannel _channel;
    private readonly OutboxMessagesConsumerOptions _outboxMessageConsumer;
    private readonly IInboxMessageRepository _inboxMessageRepository;

    public OutboxMessagesConsumerBackgroundService(
        ILogger<OutboxMessagesConsumerBackgroundService> logger,
        IOptions<OutboxMessagesConsumerOptions> outboxMessageConsumer,
        IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _outboxMessageConsumer = outboxMessageConsumer.Value;
        _inboxMessageRepository = serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<IInboxMessageRepository>();
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _connection = await _outboxMessageConsumer.RabbitMQ.Factory.CreateConnectionAsync(cancellationToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);
        await _channel.QueueDeclareAsync(
            queue: _outboxMessageConsumer.RabbitMQ.QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        _logger.LogInformation("RabbitMQ connection established and queue declared.");

        _logger.LogInformation($"{nameof(OutboxMessagesConsumerBackgroundService)} started at: {DateTime.Now}");

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            _logger.LogInformation("Received message: {message}", message);
            var outboxMessageDto = JsonSerializer.Deserialize<OutboxMessageDto>(message)!;
            var inboxMessage = InboxMessage.Create(
                entityId: outboxMessageDto.EntityId,
                entityType: outboxMessageDto.EntityType,
                eventType: outboxMessageDto.EventType,
                payload: outboxMessageDto.Payload);

            try
            {
                await _inboxMessageRepository.AddAsync(inboxMessage);
                await _channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving the message: {message}", message);
            }
        };

        await _channel.BasicConsumeAsync(
            queue: _outboxMessageConsumer.RabbitMQ.QueueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: cancellationToken);
    }

    public override void Dispose()
    {
        _connection?.Dispose();
        _channel?.Dispose();

        base.Dispose();
    }
}
