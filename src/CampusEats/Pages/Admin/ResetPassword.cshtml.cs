#nullable disable

using CampusEats.Core.Identity.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CampusEats.Pages.Admin;

public class ResetPasswordModel : PageModel
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger<ResetPasswordModel> _logger;

    public ResetPasswordModel(UserManager<AppUser> userManager, ILogger<ResetPasswordModel> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }


    [BindProperty]
    public InputModel Input { get; set; }


    public class InputModel
    {

        [Required]
        [EmailAddress]
        public string Email { get; set; }


        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; }


        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

    }

    public IActionResult OnGet()
    {
        Input = new InputModel
        {
            Email = HttpContext.Session.GetString("email") ?? ""
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if(!ModelState.IsValid)
        {
            return Page();
        }

        var admin = await _userManager.FindByEmailAsync(Input.Email);
        if(admin == null)
        {
            // Don't reveal that the user does not exist
            _logger.LogInformation("User dont exist");
            return RedirectToPage("/Identity/ResetPasswordConfirmation");
        }
        var Code = await _userManager.GeneratePasswordResetTokenAsync(admin);
        var result = await _userManager.ResetPasswordAsync(admin, Code, Input.Password);
        if(result.Succeeded)
        {
            if(admin.FirstLogin)
            {
                admin.FirstLogin = false;
                await _userManager.UpdateAsync(admin);
            }
            _logger.LogInformation("Password Reset Completed");

            HttpContext.Session.Remove("email");
            return RedirectToPage("/Admin/Index");
        }

        foreach(var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
        return Page();
    }
}