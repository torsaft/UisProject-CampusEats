using CampusEats.Core.Common;

namespace CampusEats.Core.Cart.Domain;

public sealed class CartItem : BaseEntity
{
    public CartItem(Guid productId, string productName, decimal price)
    {
        Price = price;
        ProductId = productId;
        ProductName = productName;
        Count = 1;
    }
    public Guid ProductId { get; private set; }
    public int Count { get; private set; }
    public string ProductName { get; set; }
    public decimal Price { get; set; }
    public decimal Sum => Price * Count;
    public void AddOne() => Count++;
}
