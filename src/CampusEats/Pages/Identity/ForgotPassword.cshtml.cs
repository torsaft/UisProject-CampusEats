
#nullable disable

using CampusEats.Core.Identity.Domain;
using CampusEats.Core.Notifications.Application.Pipeline;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;


namespace CampusEats.Pages.Identity;

public class ForgotPasswordModel : PageModel
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<ForgotPasswordModel> _logger;
    private readonly IMediator _mediator;

    public ForgotPasswordModel(
        UserManager<AppUser> userManager,
        IEmailSender emailSender,
        ILogger<ForgotPasswordModel> logger,
        IMediator mediator)
    {
        _userManager = userManager;
        _emailSender = emailSender;
        _logger = logger;
        _mediator = mediator;
    }

    [BindProperty]
    public InputModel Input { get; set; }

    public class InputModel
    {

        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if(ModelState.IsValid)
        {
            _logger.LogInformation("Reset Password Requested");
            var user = await _userManager.FindByEmailAsync(Input.Email);
            // Finner ikke brukeren
            //if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            //{
            //    _logger.LogInformation("User dont exist");
            //    // Don't reveal that the user does not exist or is not confirmed
            //    return RedirectToPage("./ForgotPasswordConfirmation");
            //}

            // For more information on how to enable account confirmation and password reset please
            // visit https://go.microsoft.com/fwlink/?LinkID=532713

            await _mediator.Send(new SendEmailNewPassword.Request(Input.Email));

            return RedirectToPage("/Identity/ForgotPasswordConfirmation");
        }

        return Page();
    }
}