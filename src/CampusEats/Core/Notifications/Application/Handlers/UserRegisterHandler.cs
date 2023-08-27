using CampusEats.Core.Identity.Domain.Events;
using MediatR;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace CampusEats.Core.Notifications.Application.Handlers;

// When a user register, we want to send a email with a confirmation 
public class UserRegisterHandler : INotificationHandler<UserRegister>
{
    private readonly IEmailSender _emailSender;

    public UserRegisterHandler(IEmailSender emailSender)
    {
        _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
    }

    public async Task Handle(UserRegister notification, CancellationToken cancellationToken)
    {
        await _emailSender.SendEmailAsync(notification.Email, "Register Confirmation",
            $"Thank you for registering on CampuseEats :).");
    }
}
