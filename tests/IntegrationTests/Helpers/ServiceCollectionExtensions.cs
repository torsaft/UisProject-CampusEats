using CampusEats.Core.Identity.Application.Services;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using System.Data.Common;

namespace IntegrationTests.Helpers;

public static class ServiceCollectionExtensions
{
    public static void UseTestDb<TContext>(this IServiceCollection services, DbConnection connection) where TContext : DbContext
    {
        services.RemoveAll(typeof(DbContextOptions<TContext>));
        services.AddDbContext<TContext>(options =>
        {
            options.UseNpgsql(connection);
        });
    }

    public static void UseCurrentUserMock(this IServiceCollection services, Func<string> GetUserId, Func<string> GetUserEmail)
    {
        services.RemoveAll(typeof(ICurrentUser));
        services.AddTransient(provider => Mock.Of<ICurrentUser>(s =>
            s.UserId == GetUserId()
            && s.IsAuthenticated == !string.IsNullOrWhiteSpace(GetUserId())
            && s.Email == GetUserEmail()
        ));
    }

    public static void MockEmailSender(this IServiceCollection services)
    {
        services.RemoveAll(typeof(IEmailSender));
        services.AddTransient(s => Mock.Of<IEmailSender>());
    }
}
