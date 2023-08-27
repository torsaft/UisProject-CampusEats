namespace CampusEats.Core.Common;
public abstract class BaseEntity
{
    public Guid Id { get; protected set; }
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    public void AddEvent(IDomainEvent @event) => _domainEvents.Add(@event);
    public void ClearEvents() => _domainEvents.Clear();
}
