using CampusEats.Core.Identity.Domain.Events;
using MediatR;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace CampusEats.Core.Notifications.Application.Handlers;

public class CourierRequestHandler : INotificationHandler<CourierRequest>
{
    private readonly IEmailSender _emailSender;

    public CourierRequestHandler(IEmailSender emailSender)
    {
        _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
    }

    public async Task Handle(CourierRequest notification, CancellationToken cancellationToken)
    {
        var userEmail = notification.UserEmail;
        var status = notification.UserCourierStatus;

        await _emailSender.SendEmailAsync(userEmail, "Courier Request",
            $"Your request for courier has been {status}");
    }
}
