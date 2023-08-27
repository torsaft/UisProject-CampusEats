namespace CampusEats.Core.Cart.Domain.Dto;

public sealed record CartItemDto
(
    Guid ProductId,
    string ProductName,
    decimal Price,
    int Count = 1
);
