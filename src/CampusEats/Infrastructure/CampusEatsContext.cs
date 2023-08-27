using CampusEats.Core.Cart.Domain;
using CampusEats.Core.Common;
using CampusEats.Core.Delivering.Domain;
using CampusEats.Core.Ordering.Domain;
using CampusEats.Core.Products.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Infrastructure;
public sealed class CampusEatsContext : DbContext
{
    private readonly IMediator _mediator;
    public CampusEatsContext(DbContextOptions<CampusEatsContext> options, IMediator mediator) : base(options)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<ShoppingCart> ShoppingCarts { get; set; } = null!;
    public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<Delivery> Deliveries { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>().OwnsOne(o => o.Location);
        modelBuilder.Entity<Delivery>().OwnsOne(d => d.Address);
        base.OnModelCreating(modelBuilder);
    }

    public override int SaveChanges()
    {
        EmitEventsAsync().GetAwaiter().GetResult();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var res = await base.SaveChangesAsync(cancellationToken);
        await EmitEventsAsync();
        return res;
    }

    public async Task EmitEventsAsync()
    {
        var entities = ChangeTracker.Entries<BaseEntity>()
            .Select(p => p.Entity)
            .Where(e => e.DomainEvents.Count > 0)
            .ToArray();

        var domainEvents = entities.SelectMany(e => e.DomainEvents).ToArray();
        foreach(var entity in entities)
        {
            entity.ClearEvents();
        }

        foreach(var @event in domainEvents)
        {
            await _mediator.Publish(@event);
        }
    }
}