using CampusEats.Core.Common;

namespace CampusEats.Core.Cart.Domain;

public sealed class ShoppingCart : BaseEntity
{
    public ShoppingCart(Guid id)
    {
        Id = id;
    }
    private readonly List<CartItem> _cartItems = new();
    public IReadOnlyCollection<CartItem> CartItems => _cartItems.AsReadOnly();

    public void AddItem(Guid productId, string name, decimal price)
    {
        var cartItem = _cartItems.FirstOrDefault(i => i.ProductId == productId);
        if (cartItem == null)
        {
            cartItem = new CartItem(productId, name, price);
            _cartItems.Add(cartItem);
            return;
        }
        cartItem.AddOne();
    }
}
