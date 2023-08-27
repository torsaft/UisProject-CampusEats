﻿namespace CampusEats.Core.Products.Domain.Dto;

public sealed record CreateProductDto
{
    public string Description { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}