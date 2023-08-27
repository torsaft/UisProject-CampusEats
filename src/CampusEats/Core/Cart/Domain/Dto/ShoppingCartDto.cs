namespace CampusEats.Core.Cart.Domain.Dto;

public sealed record ShoppingCartDto(IEnumerable<CartItemDto> CartItems);
