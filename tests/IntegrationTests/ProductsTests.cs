using CampusEats.Core.Common;
using CampusEats.Core.Products.Application;
using CampusEats.Core.Products.Domain;
using CampusEats.Core.Products.Domain.Dto;

namespace IntegrationTests;

public sealed class ProductsTest : BaseIntegrationTest
{
    public ProductsTest(CampusEatsFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetProducts_ShouldNeverBeNull()
    {
        var response = await SendRequest(new GetProducts.Request());
        response.Should().NotBeNull();
        response.Should().BeOfType<GenericResponse<List<ProductDto>>>();
    }

    [Theory]
    [InlineData("Name", "Description", 1.0)]
    [InlineData("Name Is valid", "Description asjdasnd saolaND", 14550.0)]
    public async Task CreateProduct_ShouldSucceed_WhenInputIsValid(string name, string description, decimal price)
    {
        var productDto = new ProductDto
        {
            Name = name,
            Description = description,
            Price = price
        };
        
        var response = await SendRequest(new CreateProduct.Request(productDto));
        response.Should().NotBeNull();
        response.Success.Should().BeTrue();
        response.Data.Should().NotBeEmpty();
        response.Errors.Should().BeEmpty();
    }
    [Theory]
    [InlineData("", "Description", 1.0)]
    [InlineData("Name", "", 1.0)]
    [InlineData("Name", "Description", 0.0)]
    public async Task CreateProduct_ShouldReturnError_WhenInputIsInvalid(string name, string description, decimal price)
    {
        var productDto = new ProductDto
        {
            Name = name,
            Description = description,
            Price = price
        };
        
        var response = await SendRequest(new CreateProduct.Request(productDto));
        response.Should().NotBeNull();
        response.Success.Should().BeFalse();
        response.Data.Should().BeEmpty();
        response.Errors.Should().NotBeEmpty();
        response.Errors.Length.Should().Be(1);
    }

    [Theory]
    [InlineData("", "", 1.0)]
    [InlineData("", "Description", 0.0)]
    [InlineData("Name", "", 0.0)]
    public async Task CreateProduct_ShouldReturnError_When2InputIsInvalid(string name, string description, decimal price)
    {
        var productDto = new ProductDto
        {
            Name = name,
            Description = description,
            Price = price
        };
        
        var response = await SendRequest(new CreateProduct.Request(productDto));
        response.Should().NotBeNull();
        response.Success.Should().BeFalse();
        response.Data.Should().BeEmpty();
        response.Errors.Should().NotBeEmpty();
        response.Errors.Length.Should().Be(2);
    }
    [Fact]
    public async Task CreateProduct_ShouldReturnError_WhenAllInputIsInvalid()
    {
        var productDto = new ProductDto
        {
            Name = "",
            Description = "",
            Price = 0.0m
        };
        
        var response = await SendRequest(new CreateProduct.Request(productDto));
        response.Should().NotBeNull();
        response.Success.Should().BeFalse();
        response.Data.Should().BeEmpty();
        response.Errors.Should().NotBeEmpty();
        response.Errors.Length.Should().Be(3);
    }



    //////7

    [Theory]
    [InlineData("Name", "Description", 1.0)]
    [InlineData("Name Is valid", "Description asjdasnd saolaND", 14550.0)]
    public async Task EditProduct_ShouldSucceed_WhenInputIsValid(string name, string description, decimal price)
    {
        var existingProduct = await MakeExistingEditProduct();
        var productDto = new ProductDto
        {
            Id = existingProduct,
            Name = name,
            Description = description,
            Price = price
        };
        
        var response = await SendRequest(new EditProduct.Request(productDto));
        response.Should().NotBeNull();
        response.Success.Should().BeTrue();
        response.Errors.Should().BeEmpty();
    }
    [Theory]
    [InlineData("", "Description", 1.0)]
    [InlineData("Name", "", 1.0)]
    [InlineData("Name", "Description", 0.0)]
    public async Task EditProduct_ShouldReturnError_WhenInputIsInvalid(string name, string description, decimal price)
    {
        var existingProduct = await MakeExistingEditProduct();
        var productDto = new ProductDto
        {
            Id = existingProduct,
            Name = name,
            Description = description,
            Price = price
        };
        
        var response = await SendRequest(new EditProduct.Request(productDto));
        response.Should().NotBeNull();
        response.Success.Should().BeFalse();
        response.Errors.Should().NotBeEmpty();
        response.Errors.Length.Should().Be(1);
    }

    [Theory]
    [InlineData("", "", 1.0)]
    [InlineData("", "Description", 0.0)]
    [InlineData("Name", "", 0.0)]
    public async Task EditProduct_ShouldReturnError_When2InputIsInvalid(string name, string description, decimal price)
    {
        var existingProduct = await MakeExistingEditProduct();
        var productDto = new ProductDto
        {
            Id = existingProduct,
            Name = name,
            Description = description,
            Price = price
        };
        
        var response = await SendRequest(new EditProduct.Request(productDto));
        response.Should().NotBeNull();
        response.Success.Should().BeFalse();
        response.Errors.Should().NotBeEmpty();
        response.Errors.Length.Should().Be(2);
    }
    [Fact]
    public async Task EditProduct_ShouldReturnError_WhenAllInputIsInvalid()
    {
        var existingProduct = await MakeExistingEditProduct();
        var productDto = new ProductDto
        {
            Id = existingProduct,
            Name = "",
            Description = "",
            Price = 0.0m
        };
        
        var response = await SendRequest(new EditProduct.Request(productDto));
        response.Should().NotBeNull();
        response.Success.Should().BeFalse();
        response.Errors.Should().NotBeEmpty();
        response.Errors.Length.Should().Be(3);
    }
    public async Task<Guid> MakeExistingEditProduct()
    {
        var product = new Product("existing product", 1.0m, "existing desc");
        await AddAsync(product);
        return product.Id;
    }

}
