using Contracts.Events;
using Contracts.Events.Order;
using Shared.Factories;

namespace Infrastructure.Factories;

public record EmailTemplate(string To, string Subject, string Body);

public static class EmailTemplateFactory
{
    public static async Task<EmailTemplate> GetTemplateAsync(this IEvent @event)
        => @event switch
            {
                OrderCreatedEvent createdEvent => new EmailTemplate(createdEvent.ShipToEmail, $"New order received: {createdEvent.Id}", await @event.RenderTemplateAsync()),
                OrderStateUpdatedEvent stateUpdatedEvent => new EmailTemplate(stateUpdatedEvent.ShipToEmail, $"Order state updated: {stateUpdatedEvent.Id}", await @event.RenderTemplateAsync()),
                _ => throw new NotSupportedException($"No email template has been defined for event type: {@event.GetType().Name}")
            };
}
