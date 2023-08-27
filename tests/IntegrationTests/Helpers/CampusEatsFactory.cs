using CampusEats.Core.Identity.Domain;
using CampusEats.Infrastructure;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Respawn;
using System.Data.Common;

namespace IntegrationTests.Helpers;

public sealed class CampusEatsFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private static string _currentUserId = "";
    private static string _currentUserEmail = "";

    private readonly PostgreSqlTestcontainer _dbContainer = new TestcontainersBuilder<PostgreSqlTestcontainer>()
        .WithDatabase(new PostgreSqlTestcontainerConfiguration
        {
            Database = "Test",
            Username = "Test",
            Password = "Test"
        })
        .Build();

    private DbConnection _dbConnection = default!;
    private Respawner _respawner = default!;
    public HttpClient HttpClient { get; private set; } = default!;
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.UseTestDb<CampusEatsContext>(_dbConnection);
            services.UseTestDb<IdentityContext>(_dbConnection);
            services.UseCurrentUserMock(GetUserId, GetUserEmail);
            services.MockEmailSender();
        });
        base.ConfigureWebHost(builder);
    }

    private static void ResetCurrentUser()
    {
        _currentUserEmail = "";
        _currentUserId = "";
    }
    private static string GetUserId() => _currentUserId;
    private static string GetUserEmail() => _currentUserEmail;

    public Task UseAnonymous()
    {
        ResetCurrentUser();
        return Task.CompletedTask;
    }

    public async Task<AppUser> UseCustomer() => await UseUser(Roles.Customer.ToString());
    public async Task<AppUser> UseCourier() => await UseUser(Roles.Courier.ToString());
    public async Task<AppUser> UseAdmin() => await UseUser(Roles.Admin.ToString());
    private async Task<AppUser> UseUser(string roleName)
    {
        using var scope = Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        if(!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }

        var user = (await userManager.GetUsersInRoleAsync(roleName)).FirstOrDefault();
        if(user == null)
        {
            user = new AppUser
            {
                Email = $"{roleName}@test.com",
                FullName = roleName,
                PhoneNumber = "1234",
                UserName = $"{roleName}@test.com"
            };
            await userManager.CreateAsync(user, "Test1234!");
            await userManager.AddToRoleAsync(user, roleName);
        }

        _currentUserId = user.Id;
        _currentUserEmail = user.Email;
        return user;
    }

    public async Task ResetData()
    {
        await _respawner.ResetAsync(_dbConnection);
        ResetCurrentUser();
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        _dbConnection = new NpgsqlConnection(_dbContainer.ConnectionString);
        HttpClient = CreateClient();

        await _dbConnection.OpenAsync();
        _respawner = await Respawner.CreateAsync(_dbConnection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = new[] { "public" }
        });
    }

    async Task IAsyncLifetime.DisposeAsync() => await _dbContainer.StopAsync();
}
