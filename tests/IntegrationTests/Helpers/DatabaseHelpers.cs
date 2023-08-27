using CampusEats.Core.Common;
using CampusEats.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Linq.Expressions;

namespace IntegrationTests.Helpers;

public static class DatabaseHelpers
{
    public static async Task<TEntity> AddAsync<TEntity>(this CampusEatsFactory factory, TEntity entity) where TEntity : BaseEntity
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CampusEatsContext>();

        var res = db.Add(entity);
        await db.SaveChangesAsync();
        return res.Entity;
    }

    public static async Task AddRangeAsync<TEntity>(this CampusEatsFactory factory, IEnumerable<TEntity> entities) where TEntity : BaseEntity
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CampusEatsContext>();

        db.AddRange(entities);
        await db.SaveChangesAsync();
    }

    public static async Task<TEntity[]> GetEntitiesAsync<TEntity>(this CampusEatsFactory factory, Expression<Func<TEntity, bool>>? expression = null, string[]? includes = null) where TEntity : BaseEntity
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CampusEatsContext>();

        IQueryable<TEntity> query = db.Set<TEntity>();
        if(includes != null)
        {
            query = includes.Aggregate(query, (current, include) => current.Include(include));
        }

        if(expression != null)
        {
            query = query.Where(expression);
        }

        return await query.AsNoTracking().ToArrayAsync();
    }

    public static async Task<TEntity?> GetEntityAsync<TEntity>(this CampusEatsFactory factory,
        Expression<Func<TEntity, bool>> expression, string[]? includes = null) where TEntity : BaseEntity
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CampusEatsContext>();

        IQueryable<TEntity> query = db.Set<TEntity>();
        if(includes != null)
        {
            query = includes.Aggregate(query, (current, include) => current.Include(include));
        }

        return await query.AsNoTracking().FirstOrDefaultAsync(expression);
    }

    public static async Task DeleteEntityAsync<TEntity>(this CampusEatsFactory factory, TEntity entity) where TEntity : BaseEntity
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CampusEatsContext>();

        db.Remove(entity);
        await db.SaveChangesAsync();
    }

    public static async Task DeleteEntitiesAsync<TEntity>(this CampusEatsFactory factory, IEnumerable<TEntity> entities) where TEntity : BaseEntity
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CampusEatsContext>();

        db.RemoveRange(entities);
        await db.SaveChangesAsync();
    }

    public static async Task UpdateEntityAsync<TEntity>(this CampusEatsFactory factory, TEntity entity) where TEntity : BaseEntity
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CampusEatsContext>();

        db.Update(entity);
        await db.SaveChangesAsync();
    }
}
