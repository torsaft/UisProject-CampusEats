using CampusEats.Core.Admin.Application.Pipelines;
using CampusEats.Core.Identity.Domain;
using CampusEats.Core.Identity.Domain.Dto;
using CampusEats.Core.Ordering.Domain.Dto;
using Microsoft.AspNetCore.Identity;

namespace IntegrationTests;

public sealed class AdminTests : BaseIntegrationTest
{
    public AdminTests(CampusEatsFactory factory) : base(factory)
    {
    }

    // [Fact]
    // public async Task ApproveCourier_ShouldConfirm_WhenApproved()
    // {
    //     _ = await UseAdmin();
    //     var user = await UseCustomer();

    //     var response = await SendRequest(new ApproveCourier.Request(user.Id, true));
    //     response.Should().NotBeNull();
    //     response.UserCourierStatus.Should().Be(RequestStatus.Confirmed);
    // }

    [Fact]
    public async Task ApproveCourier_ShouldDecline_WhenNotApproved()
    {
        var user = await UseCustomer();
        _ = await UseAdmin();

        var response = await SendRequest(new ApproveCourier.Request(user.Id, false));
        response.Should().NotBeNull();
        response.Success.Should().BeTrue();
        response.Errors.Should().BeEmpty();
        response.Data.Should().BeOfType<CourierStatusDto>();
        response.Data.UserCourierStatus.Should().Be(RequestStatus.Declined);
    }

    [Fact]
    public async Task EditUser_ShouldSucceed()
    {
        var user = await UseCustomer();

        var response = await SendRequest(new EditUser.Request(user.Id, Roles.Customer.ToString(), "New Name", "New Phone"));
        response.Should().NotBeNull();
        response.Success.Should().BeTrue();
        response.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task GetCourierApprovals_ShouldSucceed()
    {
        _ = await UseAdmin();

        var response = await SendRequest(new GetCourierApprovals.Request());
        response.Should().NotBeNull();
        response.Success.Should().BeTrue();
        response.Errors.Should().BeEmpty();
        response.Data.Should().NotBeNull();
        response.Data.Should().BeOfType(typeof(UserDto[]));
    }

    [Fact]
    public async Task GetPaidDeliveries_ShouldSucceed_WhenAdmin()
    {
        // Arrange
        _ = await UseAdmin();

        // Act
        var response = await SendRequest(new GetPaidDeliveries.Request());

        // Assert
        response.Should().NotBeNull();
        response.Success.Should().BeTrue();
        response.Errors.Should().BeEmpty();
        response.Data.Should().NotBeNull();
        response.Data.Should().BeOfType(typeof(OrderExpenseDto[]));
    }

    [Fact]
    public async Task GetRoles_ShouldSucceed_WhenAdmin()
    {
        _ = await UseAdmin();

        var response = await SendRequest(new GetRoles.Request());
        response.Should().NotBeNull();
        response.Should().BeOfType(typeof(IdentityRole[]));
    }

    [Fact]
    public async Task GetUsers_ShouldSucceed()
    {
        _ = await UseAdmin();

        var response = await SendRequest(new GetUsers.Request());
        response.Should().NotBeNull();
        response.Success.Should().BeTrue();
        response.Errors.Should().BeEmpty();
        response.Data.Should().NotBeNull();
        response.Data.Should().BeOfType(typeof(UserDto[]));
    }

    [Fact]
    public async Task GetUserDetails_ShouldSucceed_WhenAdmin()
    {
        // Arrange
        var user = await UseCustomer();
        _ = await UseAdmin();

        // Act
        var response = await SendRequest(new GetUserDetails.Request(user.Id));

        // Assert
        response.Should().NotBeNull();
        response.Success.Should().BeTrue();
        response.Errors.Should().BeEmpty();
        response.Data.Should().NotBeNull();
        response.Data.Should().BeOfType(typeof(UserDto));
        response.Data.Id.Should().Be(user.Id);
    }

    [Fact]
    public async Task SetDeliveryFee_ShouldSucceed()
    {
        _ = await UseAdmin();

        var response = await SendRequest(new SetDeliveryFee.Request(200));
        response.Should().NotBeNull();
        response.Should().BeOfType(MediatR.Unit.Value.GetType());
    }
}