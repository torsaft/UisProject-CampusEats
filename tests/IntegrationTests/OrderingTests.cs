using CampusEats.Core.Cart.Domain;
using CampusEats.Core.Cart.Domain.Events;
using CampusEats.Core.Cart.Pipelines;
using CampusEats.Core.Ordering.Application.Pipelines;
using CampusEats.Core.Ordering.Domain;
using CampusEats.Core.Ordering.Domain.Dto;
using CampusEats.Core.Products.Domain;
using Stripe.Checkout;

namespace IntegrationTests;

public sealed class OrderingTests : BaseIntegrationTest
{
	public OrderingTests(CampusEatsFactory factory) : base(factory)
	{
	}

	[Fact]
	public async Task GetAllOrders_ShouldNotBeNull_WhenAdmin()
	{
		_ = await UseAdmin();

		var response = await SendRequest(new GetAllOrders.Request());
		response.Should().NotBeNull();
		response.Should().BeOfType<List<Order>>();
	}

	[Fact]
	public async Task GetAllOrders_ShouldReturnEmptyList_WhenNotAuthenticated()
	{
		var response = await SendRequest(new GetAllOrders.Request());
		response.Should().BeEmpty();
		response.Should().BeOfType<List<Order>>();
	}

	[Fact]
	public async Task GetAllOrders_ShouldNotBeEmpty_WhenCurrentUserHaveOrders()
	{
		var user = await UseCustomer();

		var order = new Order(
			new Location("Test", "test", "test"),
			new Customer(user.Id, user.Email),
			"test"
		);
		await AddAsync(order);

		var response = await SendRequest(new GetAllOrders.Request());
		response.Should().NotBeEmpty();
		response.Should().BeOfType<List<Order>>();
	}

	[Fact]
	public async Task GetAllOrders_ShouldOnlyContainUsersOrders_WhenCurrentUserHaveOrders()
	{
		// Arrange
		var user = await UseCustomer();

		var currentCustomer = new Customer(user.Id, user.Email);
		var someOtherCustomer = new Customer(Guid.NewGuid().ToString(), "test@test.com");
		var orders = new Order[]
		{
			new Order(new Location("Test", "test", "test"), currentCustomer, "test"),
			new Order(new Location("Test", "test", "test"), currentCustomer, "test"),
			new Order(new Location("Test", "test", "test"), currentCustomer, "test"),
			new Order(new Location("Test", "test", "test"), someOtherCustomer, "test"),
			new Order(new Location("Test", "test", "test"), someOtherCustomer, "test"),
			new Order(new Location("Test", "test", "test"), someOtherCustomer, "test"),
			new Order(new Location("Test", "test", "test"), someOtherCustomer, "test"),
		};

		await AddRangeAsync(orders);

		// Act
		var response = await SendRequest(new GetAllOrders.Request());

		// Assert
		var usersOrders = orders.Where(o => o.Customer.UserId == user.Id).ToArray();
		response.Should().NotBeEmpty();
		response.Should().BeOfType<List<Order>>();
		response.Should().HaveCount(usersOrders.Length);
		response.Should().OnlyContain(o => o.Customer.UserId == user.Id);
	}

	[Fact]
	public async Task GetSingleOrder_ShouldAlwaysSucceed_WhenAdmin()
	{
		// Arrange
		await UseAdmin();

		var order = new Order(
			new Location("Test", "test", "test"),
			new Customer("test", "test"),
			"test"
		);
		await AddAsync(order);

		// Act
		var response = await SendRequest(new GetSingleOrder.Request(order.Id));

		// Assert
		response.Should().NotBeNull();
		response.Success.Should().BeTrue();
		response.Data.Should().NotBeNull();
		response.Data.Should().BeOfType<OrderDto>();
		response.Data.OrderId.Should().Be(order.Id);
		response.Errors.Should().BeEmpty();
	}

	[Fact]
	public async Task GetSingleOrder_ShouldNotSucceed_WhenNotAuthenticated()
	{
		// Arrange
		var order = new Order(
			new Location("Test", "test", "test"),
			new Customer("id", "email"),
			"test"
		);
		await AddAsync(order);

		// Act
		var response = await SendRequest(new GetSingleOrder.Request(order.Id));

		// Assert
		response.Should().NotBeNull();
		response.Data.Should().BeNull();
		response.Success.Should().BeFalse();
		response.Errors.Should().HaveCount(1);
		response.Errors.First().Should().Be("User is not authenticated");
	}

	[Fact]
	public async Task GetSingleOrder_ShouldNotSucceed_WhenNotYourOrder()
	{
		// Arrange
		await UseCustomer();
		var order = new Order(
			new Location("Test", "test", "test"),
			new Customer("notCurrentUserId", "notCurrentUserEmail"),
			"test"
		);
		await AddAsync(order);

		// Act
		var response = await SendRequest(new GetSingleOrder.Request(order.Id));

		// Assert
		response.Should().NotBeNull();
		response.Success.Should().BeFalse();
		response.Data.Should().BeNull();
		response.Errors.Should().HaveCount(1);
		response.Errors.First().Should().Be("You are not authorized to view this order");
	}

	[Fact]
	public async Task GetSingleOrder_ShouldBeSuccess_WhenYourOrder()
	{
		// Arrange
		var user = await UseCustomer();
		var order = new Order(
			new Location("Test", "test", "test"),
			new Customer(user.Id, user.Email),
			"test"
		);
		await AddAsync(order);

		// Act
		var response = await SendRequest(new GetSingleOrder.Request(order.Id));

		// Assert
		response.Should().NotBeNull();
		response.Success.Should().BeTrue();
		response.Errors.Should().BeEmpty();
		response.Data.Should().NotBeNull();
		response.Data.Should().BeOfType<OrderDto>();
		response.Data.OrderId.Should().Be(order.Id);
	}

	[Fact]
	public async Task CancelOrder_ShouldChangeStatus()
	{
		var user = await UseCustomer();
		var order = new Order(
			new Location("Test", "test", "test"),
			new Customer("Test", "Test"),
			"test"
		);
		await AddAsync(order);
		order.Cancel();

		order.Status.Should().Be(Status.Canceled);
	}

	[Fact]
	public async Task TipCourier_ShouldReturnStripeSession()
	{
		var user = await UseCustomer();
		var order = new Order(
			new Location("Test", "test", "test"),
			new Customer("Test", "myEmail@test.com"),
			"test"
		);
		await AddAsync(order);

		var response = await SendRequest(new TipCourier.Request(order.Id, 100));
		response.Should().NotBeNull();
		response.Should().BeOfType<TipCourier.Response>();
		response.Session.Should().NotBeNull();
		response.Session.Should().BeOfType<Session>();
	}

	[Fact]
	public async Task CheckoutSucceededHandler_ShouldPlaceOrder()
	{
		var user = await UseCustomer();

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

		var session = await CreateFakeStripeSession(cart.Id);

		var orderDto = new CreateOrderDto(session);

		await PublishEvent(new CheckoutSucceeded(orderDto));

		var savedOrder = await GetEntityAsync<Order>(o => o.StripePaymentId == "testForCheckOutSucceededHandler");
		savedOrder.Should().NotBeNull();
		savedOrder.Should().BeOfType<Order>();

	}

	public async Task<Session> CreateFakeStripeSession(Guid cartId)
	{
		var options = new SessionCreateOptions
		{
			PaymentMethodTypes = new List<string>
			{
				"card",
			},
			Mode = "payment",
			LineItems = new List<SessionLineItemOptions>
			{
				new SessionLineItemOptions
				{
					PriceData = new SessionLineItemPriceDataOptions
					{
						UnitAmount = 1000,
						Currency = "usd",
						ProductData = new SessionLineItemPriceDataProductDataOptions
						{
							Name = "Test Product",
						},
					},
					Quantity = 1,
				},

			},
			SuccessUrl = "https://example.com/success",
			CancelUrl = "https://example.com/cancel",
			Metadata = new Dictionary<string, string>()
				{
					{"cartId", $"{cartId}"},
					{"locationBuilding", "Test"},
					{"locationRoomNumber", $"Test"},
					{"locationNotes", $"Test"},
					{"deliveryFee", $"Test"},
				},

		};
		var service = new SessionService();
		var session = await service.CreateAsync(options);
		session.PaymentIntentId = "testForCheckOutSucceededHandler";
		return session;
	}
}
