
#nullable disable

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CampusEats.Pages.Identity;


[AllowAnonymous]
public class ForgotPasswordConfirmation : PageModel
{

    public void OnGet()
    {
    }
}