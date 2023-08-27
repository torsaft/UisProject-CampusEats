using CampusEats.Core.Identity.Application.Pipeline;
using CampusEats.Core.Identity.Domain;

namespace IntegrationTests;

public sealed class IdentityTests : BaseIntegrationTest
{
    public IdentityTests(CampusEatsFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task EditUserInfo_ShouldSucceed()
    {
        var user = await UseCustomer();

        var response = await SendRequest(new EditUserInfo.Request(user.Id, "newFullName", "newPhoneNumber"));
        response.Should().NotBeNull();
        response.Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task RequestForCourier_ShouldSucceed_WhenCustomer()
    {
        var user = await UseCustomer();

        var response = await SendRequest(new RequestForCourier.Request(user.Id));
        response.Should().NotBeNull();
        response.Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task RequestForCourier_ShouldFail_WhenAlreadyCourier()
    {
        var user = await UseCourier();

        var response = await SendRequest(new RequestForCourier.Request(user.Id));
        response.Succeeded.Should().BeFalse();
    }
}