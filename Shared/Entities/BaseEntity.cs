namespace Shared.Entities;

public class BaseEntity
{
    /// <summary>
    /// The entity id
    /// </summary>
    public Guid Id { get; protected set; } = Guid.NewGuid();

    /// <summary>
    /// Description 
    /// </summary>
    public string Description { get; protected set; } = string.Empty;

    /// <summary>
    /// Remarks
    /// </summary>
    public string Remarks { get; protected set; } = string.Empty;

    /// <summary>
    /// When an entity was created
    /// </summary>
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;

    /// <summary>
    /// When an entity was updated
    /// </summary>
    public DateTime? UpdatedAt { get; protected set; } = null;

    /// <summary>
    /// When an entity was deleted
    /// </summary>
    public DateTime? DeletedAt { get; protected set; } = null;

    /// <summary>
    /// Helper property which determines
    /// whether entity was deleted
    /// </summary>
    public bool IsDeleted => DeletedAt.HasValue;
}
