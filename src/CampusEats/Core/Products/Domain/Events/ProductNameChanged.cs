using CampusEats.Core.Common;

namespace CampusEats.Core.Products.Events;

public sealed record ProductNameChanged : IDomainEvent
{
	public ProductNameChanged(Guid productId, string oldName, string newName)
	{
		ProductId = productId;
		OldName = oldName ?? throw new System.ArgumentNullException(nameof(oldName));
		NewName = newName ?? throw new System.ArgumentNullException(nameof(newName));
	}

	public Guid ProductId { get; }
	public string OldName { get; }
	public string NewName { get; }
}
