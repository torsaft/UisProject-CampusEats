using Microsoft.AspNetCore.Identity.UI.Services;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace CampusEats.Core.Notifications.Application.Services;

public class EmailSender : IEmailSender
{
    private readonly ILogger _logger;
    private readonly string _sendGridKey;
    public EmailSender(ILogger<EmailSender> logger, IConfiguration config)
    {
        _sendGridKey = config.GetValue<string>("SendGridKey");
        _logger = logger;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string message)
    {
        if(string.IsNullOrEmpty(_sendGridKey))
        {
            throw new ArgumentNullException(nameof(_sendGridKey));
        }
        await Execute(_sendGridKey, subject, message, toEmail);
    }

    private async Task Execute(string apiKey, string subject, string message, string toEmail)
    {
        var client = new SendGridClient(apiKey);
        var msg = new SendGridMessage()
        {
            From = new EmailAddress("knudsen3030@hotmail.no", "CampuseEats Email Service"),
            Subject = subject,
            PlainTextContent = message,
            HtmlContent = message
        };
        msg.AddTo(new EmailAddress(toEmail));
        msg.SetClickTracking(false, false);
        var response = await client.SendEmailAsync(msg);
        _logger.LogInformation(response.IsSuccessStatusCode
                              ? $"Email to {toEmail} queued successfully!"
                              : $"Failure Email to {toEmail}");
    }
}