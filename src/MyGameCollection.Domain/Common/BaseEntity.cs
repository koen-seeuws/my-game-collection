namespace MyGameCollection.Domain.Common;

public abstract class BaseEntity
{
    protected BaseEntity() : this(Guid.CreateVersion7()) { }

    protected BaseEntity(Guid id) => Id = id;

    public Guid Id { get; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    /// <summary>Restores audit timestamps when the entity is rebuilt from storage.</summary>
    public void RestoreAudit(DateTimeOffset createdAt, DateTimeOffset updatedAt)
    {
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }
}
