using CampusEats.Core.Delivering.Application.Pipelines;
using CampusEats.Core.Delivering.Domain;
using CampusEats.Core.Delivering.Domain.Dto;
using CampusEats.Core.Delivering.Pipelines;
using CampusEats.Core.Ordering.Domain;
using CampusEats.Core.Ordering.Domain.Events;
using Status = CampusEats.Core.Delivering.Domain.Status;

namespace IntegrationTests;

public sealed class DeliveringTests : BaseIntegrationTest
{
    public DeliveringTests(CampusEatsFactory factory) : base(factory)
    {
    }

    private async Task<(Delivery, Order)> CreateValidDelivery()
    {
        var order = new Order(new Location("test", "test", "test"), new Customer("someId", "someEmail"), "someId");
        order.AddOrderLine(Guid.NewGuid(), 100, 3, "test name");
        order.AddOrderLine(Guid.NewGuid(), 200, 2, "test name");
        order.Place();
        await AddAsync(order);

        var delivery = await GetEntityAsync<Delivery>(d => d.OrderId == order.Id);
        return (delivery!, order);
    }


    [Fact]
    public async Task Delivery_ShouldBeCreated_WhenOrderPlaced()
    {
        // Arrange
        var user = await UseCustomer();
        var order = new Order(new Location("test", "test", "test"), new Customer(user.Id, user.Email), "someId");
        await AddAsync(order);

        // Act
        var orderPlaced = new OrderPlaced(order.Id, order.Location, order.DeliveryFee);
        await PublishEvent(orderPlaced);

        // Assert
        var delivery = await GetEntityAsync<Delivery>(d => d.OrderId == order.Id);
        delivery.Should().NotBeNull();
        delivery!.Courier.Should().BeNull();
        delivery.OrderId.Should().Be(order.Id);
        delivery.Status.Should().Be(Status.Unassigned);
    }
    [Fact]
    public async Task Order_ShouldBeAccepted_WhenDeliveryAssigned()
    {
        // Arrange
        var user = await UseCourier();
        var (delivery, order) = await CreateValidDelivery();

        // Act
        var res = await SendRequest(new AssignCourier.Request(delivery.OrderId));

        // Assert
        res.Should().NotBeNull();
        res.Success.Should().BeTrue();
        res.Errors.Should().BeEmpty();

        var updatedOrder = await GetEntityAsync<Order>(o => o.Id == delivery.OrderId);
        updatedOrder.Should().NotBeNull();
        updatedOrder!.Id.Should().Be(order.Id);
        updatedOrder.Status.Should().Be(CampusEats.Core.Ordering.Domain.Status.Accepted);

        var updatedDelivery = await GetEntityAsync<Delivery>(d => d.Id == delivery.Id, new[] { "Courier" });
        updatedDelivery.Should().NotBeNull();
        updatedDelivery!.OrderId.Should().Be(updatedOrder.Id);
        updatedDelivery.Status.Should().Be(Status.Assigned);
        updatedDelivery.Courier.Should().NotBeNull();
        updatedDelivery.Courier!.UserId.Should().Be(user.Id);
    }

    [Fact]
    public async Task DeliveryStatus_ShouldBeCanceled_WhenOrderCanceled()
    {
        // Arrange
        var (_, order) = await CreateValidDelivery();

        // Act
        var refunded = order.Cancel();
        await UpdateEntityAsync(order);
        await PublishEvent(new OrderCanceled(order.Id, refunded, order.Customer, order.Status));

        // Assert
        var delivery = await GetEntityAsync<Delivery>(d => d.OrderId == order.Id);
        delivery.Should().NotBeNull();
        delivery!.Courier.Should().BeNull();
        delivery.OrderId.Should().Be(order.Id);
        delivery.Status.Should().Be(Status.Canceled);
    }

    [Fact]
    public async Task Delivery_ShouldBeAssigned_WhenDeliveryUnassigned()
    {
        // Arrange
        var (_, order) = await CreateValidDelivery();
        var user = await UseCourier();

        // Act
        var response = await SendRequest(new AssignCourier.Request(order.Id));
        response.Should().NotBeNull();
        response.Success.Should().BeTrue();
        response.Errors.Should().BeEmpty();

        var updatedDelivery = await GetEntityAsync<Delivery>(d => d.OrderId == order.Id, new[] { "Courier" });
        updatedDelivery.Should().NotBeNull();
        updatedDelivery!.Status.Should().Be(Status.Assigned);
        updatedDelivery.Courier.Should().NotBeNull();
        updatedDelivery.Courier!.UserId.Should().Be(user.Id);
    }

    [Fact]
    public async Task Delivery_ShouldNotBeAssigned_WhenDeliveryAlreadyAssigned()
    {
        // Arrange
        var (delivery, order) = await CreateValidDelivery();
        delivery.AssignCourier("userId", "user@email.com");
        await UpdateEntityAsync(delivery);

        // Act
        var user = await UseCourier();
        await SendRequest(new AssignCourier.Request(order.Id));

        // Assert
        var updatedDelivery = await GetEntityAsync<Delivery>(d => d.OrderId == order.Id, new[] { "Courier" });
        updatedDelivery.Should().NotBeNull();
        updatedDelivery!.Courier.Should().NotBeNull();
        updatedDelivery!.Courier!.UserId.Should().NotBe(user.Id);
    }

    [Fact]
    public async Task Delivery_ShouldNotBeAssigned_WhenDeliveryCanceled()
    {
        // Arrange
        var (delivery, _) = await CreateValidDelivery();
        delivery.Cancel();
        await UpdateEntityAsync(delivery);

        // Act
        var user = await UseCourier();
        await SendRequest(new AssignCourier.Request(delivery.OrderId));

        // Assert
        var updatedDelivery = await GetEntityAsync<Delivery>(d => d.OrderId == delivery.OrderId, new[] { "Courier" });
        updatedDelivery.Should().NotBeNull();
        updatedDelivery!.Id.Should().Be(delivery.Id);
        updatedDelivery.Status.Should().Be(Status.Canceled);
    }

    [Fact]
    public async Task MarkOrderInNextPhase_ShouldSucceed_WhenOwnerOfDelivery()
    {
        // Arrange
        var user = await UseCourier();
        var (delivery, _) = await CreateValidDelivery();
        delivery.AssignCourier(user.Id, user.Email);
        await UpdateEntityAsync(delivery);

        // Act
        var res = await SendRequest(new MarkOrderInNextPhase.Request(delivery.Id));

        // Assert
        res.Should().NotBeNull();
        res.Success.Should().BeTrue();
        res.Errors.Should().BeEmpty();

        var updatedOrder = await GetEntityAsync<Order>(o => o.Id == delivery.OrderId);
        updatedOrder.Should().NotBeNull();
        updatedOrder!.Id.Should().Be(delivery.OrderId);
        updatedOrder.Status.Should().Be(CampusEats.Core.Ordering.Domain.Status.Picked);

        var updatedDelivery = await GetEntityAsync<Delivery>(d => d.Id == delivery.Id, new[] { "Courier" });
        updatedDelivery.Should().NotBeNull();
        updatedDelivery!.Courier!.UserId.Should().Be(user.Id);
    }

    [Fact]
    public async Task MarkOrderInNextPhase_ShouldNotSucceed_WhenNotOwnerOfDelivery()
    {
        // Arrange
        var (delivery, _) = await CreateValidDelivery();
        delivery.AssignCourier("otherId", "otherEmail");
        await UpdateEntityAsync(delivery);

        // Act
        var user = await UseCourier();
        await SendRequest(new MarkOrderInNextPhase.Request(delivery.Id));

        // Assert
        var updatedOrder = await GetEntityAsync<Order>(o => o.Id == delivery.OrderId);
        updatedOrder.Should().NotBeNull();
        updatedOrder!.Id.Should().Be(delivery.OrderId);
        updatedOrder.Status.Should().Be(CampusEats.Core.Ordering.Domain.Status.Accepted);

        var updatedDelivery = await GetEntityAsync<Delivery>(d => d.Id == delivery.Id, new[] { "Courier" });
        updatedDelivery.Should().NotBeNull();
        updatedDelivery!.Courier!.UserId.Should().NotBe(user.Id);
    }

    [Fact]
    public async Task OrderDeliveredEvent_ShouldMarkDeliveryAsDelivered()
    {
        var user = await UseCourier();
        var (delivery, order) = await CreateValidDelivery();
        delivery.AssignCourier(user.Id, user.Email);
        await UpdateEntityAsync(delivery);

        // Act
        for(int i = 0; i < 2; i++)
        {
            var res = await SendRequest(new MarkOrderInNextPhase.Request(delivery.Id));
            res.Should().NotBeNull();
            res.Success.Should().BeTrue();
            res.Errors.Should().BeEmpty();
        }

        // Assert
        var updatedDelivery = await GetEntityAsync<Delivery>(d => d.Id == delivery.Id, new[] { "Courier" });
        updatedDelivery.Should().NotBeNull();
        updatedDelivery!.OrderId.Should().Be(order.Id);
        updatedDelivery.Courier.Should().NotBeNull();
        updatedDelivery.Courier!.UserId.Should().Be(user.Id);
        updatedDelivery.Status.Should().Be(Status.Delivered);

        var updatedOrder = await GetEntityAsync<Order>(o => o.Id == delivery.OrderId);
        updatedOrder.Should().NotBeNull();
        updatedOrder!.Status.Should().Be(CampusEats.Core.Ordering.Domain.Status.Delivered);
    }

    [Fact]
    public async Task GetMyAssignedDeliveries_ShouldReturnOnlyMyDeliveries()
    {
        // Arrange
        var user = await UseCourier();
        var deliveries = new[]
        {
            new Delivery(Guid.NewGuid(), 60, new Address("test", "test", "test")),
            new Delivery(Guid.NewGuid(), 60, new Address("test", "test", "test")),
            new Delivery(Guid.NewGuid(), 60, new Address("test", "test", "test")),
            new Delivery(Guid.NewGuid(), 60, new Address("test", "test", "test")),
            new Delivery(Guid.NewGuid(), 60, new Address("test", "test", "test")),
            new Delivery(Guid.NewGuid(), 60, new Address("test", "test", "test")),
            new Delivery(Guid.NewGuid(), 60, new Address("test", "test", "test")),
        };

        for(int i = 0; i < 3; i++)
        {
            deliveries[i].AssignCourier(user.Id, user.Email);
            deliveries[i].ClearEvents();
        }
        await AddRangeAsync(deliveries);

        // Act
        var res = await SendRequest(new GetMyAssignedDeliveries.Request());

        // Assert
        var expectedIds = deliveries.Where(d => d.Courier?.UserId == user.Id).Select(d => d.Id).ToArray();

        res.Should().NotBeNull();
        res.Should().BeOfType(typeof(DeliveryDto[]));
        res.Should().HaveCount(3);
        res.Select(x => x.Id).Should().BeEquivalentTo(expectedIds);
    }

    [Fact]
    public async Task GetMyAssignedDeliveries_ShouldReturnEmptyList_WhenNotCourier()
    {
        // Arrange
        var deliveries = new[]
        {
            new Delivery(Guid.NewGuid(), 60, new Address("test", "test", "test")),
            new Delivery(Guid.NewGuid(), 60, new Address("test", "test", "test")),
            new Delivery(Guid.NewGuid(), 60, new Address("test", "test", "test")),
            new Delivery(Guid.NewGuid(), 60, new Address("test", "test", "test")),
            new Delivery(Guid.NewGuid(), 60, new Address("test", "test", "test")),
            new Delivery(Guid.NewGuid(), 60, new Address("test", "test", "test")),
            new Delivery(Guid.NewGuid(), 60, new Address("test", "test", "test")),
        };
        await AddRangeAsync(deliveries);

        _ = await UseCustomer();
        // Act
        var res = await SendRequest(new GetMyAssignedDeliveries.Request());

        // Assert
        res.Should().NotBeNull();
        res.Should().BeOfType(typeof(DeliveryDto[]));
        res.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUnassignedDeliveries_ShouldOnlyReturnUnassignedDeliveries()
    {
        // Arrange
        var deliveries = new[]
        {
            new Delivery(Guid.NewGuid(), 60, new Address("test", "test", "test")),
            new Delivery(Guid.NewGuid(), 60, new Address("test", "test", "test")),
            new Delivery(Guid.NewGuid(), 60, new Address("test", "test", "test")),
            new Delivery(Guid.NewGuid(), 60, new Address("test", "test", "test")),
            new Delivery(Guid.NewGuid(), 60, new Address("test", "test", "test")),
            new Delivery(Guid.NewGuid(), 60, new Address("test", "test", "test")),
        };

        for(int i = 0; i < 4; i++)
        {
            deliveries[i].AssignCourier("otherId", "otherEmail");
            deliveries[i].ClearEvents();
        }

        await AddRangeAsync(deliveries);
        _ = await UseCourier();

        // Act
        var res = await SendRequest(new GetUnassignedDeliveries.Request());

        // Assert
        var expectedLength = await GetEntitiesAsync<Delivery>(d => d.Status == Status.Unassigned, new[] { "Courier" })
            .ContinueWith(t => t.Result.Length);

        res.Success.Should().BeTrue();
        res.Errors.Should().BeEmpty();
        res.Data.Should().NotBeNull();
        res.Data.Should().BeOfType(typeof(DeliveryDto[]));
        res.Data.Should().HaveCount(expectedLength);
    }

    [Fact]
    public async Task Deliveries_ShouldBeUnassigned_WhenNoCourier()
    {
        // Arrange
        var deliveries = new[]
        {
            new Delivery(Guid.NewGuid(), 60, new Address("test", "test", "test")),
            new Delivery(Guid.NewGuid(), 60, new Address("test", "test", "test")),
            new Delivery(Guid.NewGuid(), 60, new Address("test", "test", "test")),
            new Delivery(Guid.NewGuid(), 60, new Address("test", "test", "test")),
            new Delivery(Guid.NewGuid(), 60, new Address("test", "test", "test")),
            new Delivery(Guid.NewGuid(), 60, new Address("test", "test", "test")),
        };

        for(int i = 0; i < 4; i++)
        {
            deliveries[i].AssignCourier("otherId", "otherEmail");
            deliveries[i].ClearEvents();
        }
        await AddRangeAsync(deliveries);

        var allDeliveries = await GetEntitiesAsync<Delivery>(null, new[] { "Courier" });
        allDeliveries.Should().NotBeNull();
        allDeliveries.Where(d => d.Status == Status.Unassigned).Should().OnlyContain(d => d.Courier == null);
    }
}
