using CampusEats.Core.Cart.Domain;
using CampusEats.Core.Cart.Domain.Dto;
using CampusEats.Core.Cart.Pipelines;
using CampusEats.Core.Ordering.Domain.Dto;
using CampusEats.Core.Products.Domain;

namespace IntegrationTests;

public sealed class CartTests : BaseIntegrationTest
{
	public CartTests(CampusEatsFactory factory) : base(factory)
	{
	}

	[Fact]
	public async Task GetCart_ShouldAlways_ReturnShoppingCartDto_WhenCartExists()
	{
		// Arrange
		var cart = new ShoppingCart(Guid.NewGuid());
		await AddAsync(cart);

		// Act
		var res = await SendRequest(new GetCart.Request(cart.Id));

		// Assert
		res.Should().NotBeNull();
		res.Success.Should().BeTrue();
		res.Data.Should().NotBeNull();
		res.Data.Should().BeOfType<ShoppingCartDto>();
		res.Data.CartItems.Should().BeEmpty();
	}

	[Fact]
	public async Task AddItem_Should_AddItem_WhenInputIsValid()
	{
		// Arrange
		var cart = await AddAsync(new ShoppingCart(Guid.NewGuid()));

		var products = new List<Product>
		{
			new Product("Test Product 1", 10, "Test Description 1"),
			new Product("Test Product 2", 10, "Test Description 2"),
			new Product("Test Product 3", 10, "Test Description 3"),
		};
		await AddRangeAsync(products);

		// Act
		foreach(var product in products)
		{
			await SendRequest(new AddItem.Request(cart.Id, product.Id));
		}

		// Assert
		var savedCart = await GetEntityAsync<ShoppingCart>(c => c.Id == cart.Id, new[] { "CartItems" });

		savedCart.Should().NotBeNull();
		savedCart!.Id.Should().Be(cart.Id);
		savedCart.CartItems.Should().NotBeEmpty();
		savedCart.CartItems.Should().HaveCount(3);
		savedCart.CartItems.ElementAt(0).ProductId.Should().Be(products[0].Id);
		savedCart.CartItems.ElementAt(1).ProductId.Should().Be(products[1].Id);
		savedCart.CartItems.ElementAt(2).ProductId.Should().Be(products[2].Id);
	}

	[Fact]
	public async Task CheckOut_ShouldReturnStripeSession_WhenInputIsValid()
	{
		var cart = await MakeDummyCart();

		var locationDto = new LocationDto();
		locationDto.Building = "Test Building";
		locationDto.RoomNumber = "Test Room";
		locationDto.Notes = "Test Notes";

		var response = await SendRequest(new CartCheckoutStripe.Request(cart, locationDto));

		response.Should().NotBeNull();
		response.Success.Should().BeTrue();
		response.Data.Should().NotBeNull();
		response.Data.Should().BeOfType<string>();
		response.Errors.Should().BeEmpty();
	}

	// [Fact]
	// public async Task CheckOut_ShouldReturnError_WhenNotAuthenticated()
	// {
	//     var cart = await MakeDummyCartWithOutUser();

	//     var locationDto = new LocationDto();
	//     locationDto.Building = "Test Building";
	//     locationDto.RoomNumber = "Test Room";
	//     locationDto.Notes = "Test Notes";

	//     var response = await SendRequest(new CartCheckoutStripe.Request(cart, locationDto));

	//     response.Should().NotBeNull();
	//     response.Success.Should().BeFalse();
	//     response.Session.Should().BeNull();
	//     response.Errors.Length.Should().Be(1);
	//     response.Errors[0].Should().Be("User is not authenticated");
	// }

	[Theory]
	[InlineData("", "Test Room", "Test Notes")]
	[InlineData("Test Building", "", "Test Notes")]
	[InlineData("Test Building", "Test Room", "")]
	public async Task CheckOut_ShouldReturnError_WhenOneLocationIsInvalid(string building, string roomNumber, string notes)
	{
		var cart = await MakeDummyCart();

		var locationDto = new LocationDto();
		locationDto.Building = building;
		locationDto.RoomNumber = roomNumber;
		locationDto.Notes = notes;

		var response = await SendRequest(new CartCheckoutStripe.Request(cart, locationDto));

		response.Should().NotBeNull();
		response.Success.Should().BeFalse();
		response.Data.Should().BeNull();
		response.Errors.Length.Should().Be(1);
	}

	[Theory]
	[InlineData("", "", "Test Notes")]
	[InlineData("", "Test Room", "")]
	[InlineData("Test Building", "", "")]
	public async Task CheckOut_ShouldReturnError_WhenTwoLocationIsInvalid(string building, string roomNumber, string notes)
	{
		var cart = await MakeDummyCart();

		var locationDto = new LocationDto();
		locationDto.Building = building;
		locationDto.RoomNumber = roomNumber;
		locationDto.Notes = notes;

		var response = await SendRequest(new CartCheckoutStripe.Request(cart, locationDto));

		response.Should().NotBeNull();
		response.Success.Should().BeFalse();
		response.Data.Should().BeNull();
		response.Errors.Length.Should().Be(2);
	}

	[Fact]
	public async Task CheckOut_ShouldReturnError_WhenAllLocationIsInvalid()
	{
		var cart = await MakeDummyCart();

		var locationDto = new LocationDto
		{
			Building = "",
			RoomNumber = "",
			Notes = ""
		};

		var response = await SendRequest(new CartCheckoutStripe.Request(cart, locationDto));

		response.Should().NotBeNull();
		response.Success.Should().BeFalse();
		response.Data.Should().BeNull();
		response.Errors.Length.Should().Be(3);
	}

	private async Task<Guid> MakeDummyCart()
	{
		_ = await UseCustomer();
		var cart = await AddAsync(new ShoppingCart(Guid.NewGuid()));

		var products = new List<Product>
		{
			new Product("Test Product 1", 10, "Test Description 1"),
			new Product("Test Product 2", 10, "Test Description 2"),
			new Product("Test Product 3", 10, "Test Description 3"),
		};
		await AddRangeAsync(products);

		foreach(var product in products)
		{
			await SendRequest(new AddItem.Request(cart.Id, product.Id));
		}
		return cart.Id;
	}
	private async Task<Guid> MakeDummyCartWithOutUser()
	{
		var cart = await AddAsync(new ShoppingCart(Guid.NewGuid()));

		var products = new List<Product>
		{
			new Product("Test Product 1", 10, "Test Description 1"),
			new Product("Test Product 2", 10, "Test Description 2"),
			new Product("Test Product 3", 10, "Test Description 3"),
		};
		await AddRangeAsync(products);

		foreach(var product in products)
		{
			await SendRequest(new AddItem.Request(cart.Id, product.Id));
		}
		return cart.Id;
	}
}
