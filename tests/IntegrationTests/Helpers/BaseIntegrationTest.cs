using CampusEats.Core.Common;
using CampusEats.Core.Identity.Domain;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Linq.Expressions;

namespace IntegrationTests.Helpers;


[CollectionDefinition("Shared test collection")]
public class SharedTestCollection : ICollectionFixture<CampusEatsFactory> { }

[Collection("Shared test collection")]
public abstract class BaseIntegrationTest : IAsyncLifetime
{
    private readonly IServiceProvider _serviceProvider;
    private readonly CampusEatsFactory _factory;
    public BaseIntegrationTest(CampusEatsFactory factory)
    {
        _serviceProvider = factory.Services;
        _factory = factory;
    }

    public async Task UseAnonymous() => await _factory.UseAnonymous();
    public async Task<AppUser> UseCustomer() => await _factory.UseCustomer();
    public async Task<AppUser> UseAdmin() => await _factory.UseAdmin();
    public async Task<AppUser> UseCourier() => await _factory.UseCourier();

    public async Task<TEntity> AddAsync<TEntity>(TEntity entity) where TEntity : BaseEntity
    {
        return await _factory.AddAsync(entity);
    }

    public async Task AddRangeAsync<TEntity>(IEnumerable<TEntity> entities) where TEntity : BaseEntity
    {
        await _factory.AddRangeAsync(entities);
    }

    public async Task<TEntity[]> GetEntitiesAsync<TEntity>(Expression<Func<TEntity, bool>>? expression = null, string[]? includes = null) where TEntity : BaseEntity
    {
        return await _factory.GetEntitiesAsync(expression, includes);
    }

    public async Task<TEntity?> GetEntityAsync<TEntity>(Expression<Func<TEntity, bool>> expression, string[]? includes = null) where TEntity : BaseEntity
    {
        return await _factory.GetEntityAsync(expression, includes);
    }

    public async Task DeleteEntityAsync<TEntity>(TEntity entity) where TEntity : BaseEntity
    {
        await _factory.DeleteEntityAsync(entity);
    }

    public async Task DeleteEntitiesAsync<TEntity>(IEnumerable<TEntity> entities) where TEntity : BaseEntity
    {
        await _factory.DeleteEntitiesAsync(entities);
    }

    public async Task UpdateEntityAsync<TEntity>(TEntity entity) where TEntity : BaseEntity
    {
        await _factory.UpdateEntityAsync(entity);
    }

    public async Task<TResponse> SendRequest<TResponse>(IRequest<TResponse> request)
    {
        using var scope = _serviceProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        return await mediator.Send(request);
    }

    public async Task PublishEvent(IDomainEvent @event)
    {
        using var scope = _serviceProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        await mediator.Publish(@event);
    }

    public async Task DisposeAsync() => await _factory.ResetData();
    public Task InitializeAsync() => Task.CompletedTask;
}
