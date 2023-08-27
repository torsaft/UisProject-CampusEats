using CampusEats.Core.Common;

namespace CampusEats.Core.Products.Events;

public sealed record ProductPriceChanged : IDomainEvent
{
    public ProductPriceChanged(Guid productId, decimal oldPrice, decimal newPrice)
    {
        ProductId = productId;
        OldPrice = oldPrice;
        NewPrice = newPrice;
    }

    public Guid ProductId { get; }
    public decimal NewPrice { get; }
    public decimal OldPrice { get; }
}
