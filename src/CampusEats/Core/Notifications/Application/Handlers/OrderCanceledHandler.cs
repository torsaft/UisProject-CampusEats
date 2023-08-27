using CampusEats.Core.Ordering.Domain.Events;
using MediatR;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace CampusEats.Core.Notifications.Application.Handlers;

public class OrderCanceledHandler : INotificationHandler<OrderCanceled>
{
    private readonly IEmailSender _emailSender;

    public OrderCanceledHandler(IEmailSender emailSender)
    {
        _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
    }

    public async Task Handle(OrderCanceled notification, CancellationToken cancellationToken)
    {
        var userEmail = notification.CustomerEmail;
        var amount = notification.Sum;

        await _emailSender.SendEmailAsync(userEmail, "Order Canceled",
            $"Your order has been canceled. You will be refunded {amount} ");
    }
}
