using CampusEats.Core.Common;
using CampusEats.Core.Products.Domain.Dto;
namespace CampusEats.Core.Products.Domain.Validators;

public class FoodItemNameValidator : IValidator<ProductDto>
{
    public (bool, string) IsValid(ProductDto product)
    {
        _ = product ?? throw new ArgumentNullException(nameof(product), "Cannot validate a null object");
        if(string.IsNullOrWhiteSpace(product.Name)) return (false, $"{nameof(product.Name)} cannot be empty");
        return (true, "");
    }
}
public class FoodItemDescriptionValidator : IValidator<ProductDto>
{
    public (bool, string) IsValid(ProductDto product)
    {
        _ = product ?? throw new ArgumentNullException(nameof(product), "Cannot validate a null object");
        if(string.IsNullOrWhiteSpace(product.Description)) return (false, $"{nameof(product.Description)} cannot be empty");
        return (true, "");
    }
}
public class FoodItemPriceValidator : IValidator<ProductDto>
{
    public (bool, string) IsValid(ProductDto product)
    {
        _ = product ?? throw new ArgumentNullException(nameof(product), "Cannot validate a null object");
        if(product.Price <= 0) return (false, $"{nameof(product.Price)} must be greater than 0");
        return (true, "");
    }
}

