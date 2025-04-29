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
    private IConnection _outboxMessageConsumerConnection;
    private IChannel _outboxMessageConsumerChannel;
    private readonly OutboxMessagesConsumerOptions _outboxMessageConsumerOptions;
    private readonly IInboxMessageRepository _inboxMessageRepository;

    public OutboxMessagesConsumerBackgroundService(
        ILogger<OutboxMessagesConsumerBackgroundService> logger,
        IOptions<OutboxMessagesConsumerOptions> outboxMessageConsumerOptions,
        IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _outboxMessageConsumerOptions = outboxMessageConsumerOptions.Value;
        _inboxMessageRepository = serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<IInboxMessageRepository>();
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _outboxMessageConsumerConnection = await _outboxMessageConsumerOptions.RabbitMQ.Factory.CreateConnectionAsync(cancellationToken);
        _outboxMessageConsumerChannel = await _outboxMessageConsumerConnection.CreateChannelAsync(cancellationToken: cancellationToken);
        await _outboxMessageConsumerChannel.QueueDeclareAsync(
            queue: _outboxMessageConsumerOptions.RabbitMQ.QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);
        _logger.LogInformation("Outbox messages consumer broker (RabbitMQ) connection established and queue declared.");

        _logger.LogInformation($"{nameof(OutboxMessagesConsumerBackgroundService)} started at: {DateTime.Now}");

        var consumer = new AsyncEventingBasicConsumer(_outboxMessageConsumerChannel);

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
                await _outboxMessageConsumerChannel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving the message: {message}", message);
            }
        };

        await _outboxMessageConsumerChannel.BasicConsumeAsync(
            queue: _outboxMessageConsumerOptions.RabbitMQ.QueueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: cancellationToken);
    }

    public override void Dispose()
    {
        _outboxMessageConsumerConnection?.Dispose();
        _outboxMessageConsumerChannel?.Dispose();

        base.Dispose();
    }
}
