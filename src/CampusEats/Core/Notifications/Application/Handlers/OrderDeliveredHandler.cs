using CampusEats.Core.Ordering.Domain.Events;
using MediatR;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace CampusEats.Core.Notifications.Application.Handlers;

public sealed class OrderDeliveredHandler : INotificationHandler<OrderDelivered>
{
    private readonly IEmailSender _emailSender;

    public OrderDeliveredHandler(IEmailSender emailSender)
    {
        _emailSender = emailSender;
    }
    public async Task Handle(OrderDelivered notification, CancellationToken cancellationToken)
    {
        await _emailSender.SendEmailAsync(notification.CustomerEmail, "Order Delivered", "Your order has been delivered!");
    }
}
