using CampusEats.Core.Common;
using Stripe.Checkout;

namespace CampusEats.Core.Ordering.Domain.Events;

public class TipSucceeded : IDomainEvent
{
    public TipSucceeded(Session session)
    {
        Session = session;
    }

    public Session Session { get; init; }
}
