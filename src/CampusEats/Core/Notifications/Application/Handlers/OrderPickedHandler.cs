using CampusEats.Core.Ordering.Domain.Events;
using CampusEats.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace CampusEats.Core.Notifications.Application.Handlers;

public class OrderPickedHandler : INotificationHandler<OrderPicked>
{
    private readonly IEmailSender _emailSender;

    public OrderPickedHandler(IEmailSender emailSender, CampusEatsContext db)
    {
        _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
    }

    public async Task Handle(OrderPicked notification, CancellationToken cancellationToken)
    {
        await _emailSender.SendEmailAsync(notification.CustomerEmail, "Order Picked", "Your order has been picked up!");
    }
}
