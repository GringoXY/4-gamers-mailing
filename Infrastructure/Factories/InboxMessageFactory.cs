using Contracts.Dtos;
using Contracts.Dtos.Shared.Enums.EventTypes;
using Contracts.Events;
using Contracts.Events.Order;
using System.Text.Json;

namespace Shared.Factories;

public static class InboxMessageFactory
{
    public static IEvent CreateEvent(this InboxMessageDto message)
        => message.EventType switch
        {
            OrderEventTypes.OrderCreated => JsonSerializer.Deserialize<OrderCreatedEvent>(message.Payload)
                ?? throw new InvalidOperationException($"Invalid payload for {nameof(OrderCreatedEvent)}"),
            OrderEventTypes.OrderStateUpdated => JsonSerializer.Deserialize<OrderStateUpdatedEvent>(message.Payload)
                ?? throw new InvalidOperationException($"Invalid payload for {nameof(OrderStateUpdatedEvent)}"),
            _ => throw new NotSupportedException()
        };
}
