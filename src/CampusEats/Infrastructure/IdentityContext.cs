using CampusEats.Core.Common;
using CampusEats.Core.Identity.Domain;
using MediatR;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Infrastructure;

public sealed partial class IdentityContext : IdentityDbContext<AppUser>
{
    private readonly IMediator _mediator;

    public IdentityContext(DbContextOptions<IdentityContext> options, IMediator mediator) : base(options)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<AppUser>().Property(n => n.FullName).HasMaxLength(255);
        builder.Entity<AppUser>().Ignore(c => c.PhoneNumberConfirmed)
                                    .Ignore(c => c.LockoutEnd)
                                    .Ignore(c => c.LockoutEnabled)
                                    .Ignore(c => c.TwoFactorEnabled);

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