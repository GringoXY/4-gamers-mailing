using Contracts.Dtos.Shared.Enums;
using System.Text.Json;

namespace Shared.Entities;

public class InboxMessage : BaseEntity
{
    /// <summary>
    /// Unfortunately EF Core requires ANY constructor occurrence.
    /// If we do not provide it, it will throw exception
    /// </summary>
    private InboxMessage() { }

    public static InboxMessage Create(
        Guid entityId,
        EntityType entityType,
        string eventType,
        object payload)
        => new()
        {
            EntityId = entityId,
            EntityType = entityType,
            EventType = eventType,
            Payload = JsonSerializer.Serialize(payload)
        };

    public static InboxMessage Create(
        Guid entityId,
        EntityType entityType,
        string eventType,
        string payload)
        => new()
        {
            EntityId = entityId,
            EntityType = entityType,
            EventType = eventType,
            Payload = payload
        };

    public void MarkAsProcessed()
    {
        if (IsProcessed)
        {
            throw new InvalidOperationException($"Inbox message with ID: {Id} is already processed");
        }

        ProcessedAt = DateTime.UtcNow;
    }

    public Guid EntityId { get; private set; }
    public EntityType EntityType { get; private set; }
    public string EventType { get; private set; }

    /// <summary>
    /// Data serialized to JSON format
    /// </summary>
    public string Payload { get; private set; }
    public DateTime? ProcessedAt { get; private set; }

    /// <summary>
    /// DO NOT USE DIRECTLY IN LINQ EF CORE EXPRESSION!
    /// When used in EF Core an exception is thrown:
    /// ```
    /// .Where(o => o.IsProcessed == False)' could not be translated.
    /// Additional information: Translation of member 'IsProcessed' on entity type 'OutboxMessage' failed.
    /// This commonly occurs when the specified member is unmapped.
    /// Either rewrite the query in a form that can be translated, or switch to client evaluation explicitly by inserting a call to 'AsEnumerable', 'AsAsyncEnumerable', 'ToList', or 'ToListAsync'
    /// ```
    /// </summary>
    public bool IsProcessed => ProcessedAt is not null;
}
