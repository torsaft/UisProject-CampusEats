using CampusEats.Core.Identity.Domain;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using System.Text.Encodings.Web;


namespace CampusEats.Core.Notifications.Application.Pipeline;

public class SendEmailNewPassword
{
    public sealed record Request(string Email) : IRequest<Response>;

    public sealed record Response(bool Success, string[] Errors);

    public sealed class Handler : IRequestHandler<Request, Response>
    {
        private readonly IEmailSender _emailSender;
        private readonly UserManager<AppUser> _userManager;

        public Handler(IEmailSender emailSender, UserManager<AppUser> userManager)
        {
            _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);

            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            string callbackUrl = "https://localhost:5001/Identity/ResetPassword?code=" + code;

            await _emailSender.SendEmailAsync(
                request.Email,
                "Reset Password",
                $"Please reset your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

            return new Response(true, Array.Empty<string>());
        }
    }
}
