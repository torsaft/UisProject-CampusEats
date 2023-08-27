#nullable disable

using CampusEats.Core.Identity.Domain;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;


namespace CampusEats.Pages.Admin;

public class LoginModel : PageModel
{

    private readonly SignInManager<AppUser> _signInManager;
    private readonly ILogger<LoginModel> _logger;

    public LoginModel(SignInManager<AppUser> signInManager, ILogger<LoginModel> logger)
    {
        _signInManager = signInManager;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; }

    public string ReturnUrl { get; set; }

    [TempData]
    public string ErrorMessage { get; set; }


    public class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
    public async Task OnGetAsync(string returnUrl = null)
    {


        if(!string.IsNullOrEmpty(ErrorMessage))
        {
            ModelState.AddModelError(string.Empty, ErrorMessage);
        }

        returnUrl ??= Url.Content("~/");

        // Clear the existing external cookie to ensure a clean login process
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);


        ReturnUrl = returnUrl;
    }

    public async Task<IActionResult> OnPostAsync(string returnUrl = null)
    {
        returnUrl ??= Url.Content("/Admin/Index");

        if(ModelState.IsValid)
        {
            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, set lockoutOnFailure: true
            var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);
            if(result.Succeeded)
            {
                var admin = await _signInManager.UserManager.FindByEmailAsync(Input.Email);
                var roles = await _signInManager.UserManager.GetRolesAsync(admin);

                if(!roles.Contains(Roles.Admin.ToString()))
                {
                    await _signInManager.SignOutAsync();
                    return RedirectToPage("/Identity/Login");
                }
                if(admin.FirstLogin)
                {
                    HttpContext.Session.SetString("email", Input.Email);
                    return RedirectToPage("/Admin/ResetPassword");
                }

                _logger.LogInformation("Admin logged in.");
                return RedirectToPage("/Admin/Index");
            }

            else
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return Page();
            }
        }

        // If we got this far, something failed, redisplay form
        return Page();
    }
}
