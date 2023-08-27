namespace CampusEats.Core.Ordering.Domain.Dto;

public sealed record OrderLineDto
(
    Guid ProductId,
    string ProductName,
    decimal Price,
    int Amount
);