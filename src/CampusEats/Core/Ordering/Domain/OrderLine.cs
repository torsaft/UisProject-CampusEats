using CampusEats.Core.Common;

namespace CampusEats.Core.Ordering.Domain;

public class OrderLine : BaseEntity
{
    public OrderLine(Guid productId, decimal price, int amount, string productName)
    {
        Id = Guid.NewGuid();
        ProductId = productId;
        Price = price;
        Amount = amount;
        ProductName = productName;
    }
    public Guid ProductId { get; private set; }
    public int Amount { get; private set; }
    public string ProductName { get; private set; }
    public decimal Price { get; private set; }
    public decimal Sum => Price * Amount;
}