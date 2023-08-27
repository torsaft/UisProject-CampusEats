using CampusEats.Core.Identity.Application.Pipeline;
using CampusEats.Core.Identity.Domain;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;


namespace CampusEats.Pages.Identity;

public class ProfileModel : PageModel
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly IMediator _mediator;

    public ProfileModel(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IMediator mediator)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _mediator = mediator;
    }

    [TempData]
    public string StatusMessage { get; set; } = default!;

    [BindProperty]
    [Display(Name = "Your Courier Application status")]
    public RequestStatus? RequestStatus { get; set; }

    [BindProperty]
    public InputModel Input { get; set; } = default!;

    public class InputModel
    {
        [Display(Name = "Email Address")]
        public string Email { get; set; } = default!;

        [Display(Name = "Full Name")]
        public string FullName { get; set; } = default!;

        [Display(Name = "Phone number")]
        public string PhoneNumber { get; set; } = default!;
    }

    [BindProperty]
    public bool IsAdmin { get; set; } = false;

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if(user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }
        if((await _userManager.GetRolesAsync(user)).Contains(Roles.Admin.ToString()))
        {
            IsAdmin = true;
        }

        RequestStatus = user.RequestStatus;

        Input = new InputModel
        {
            Email = user.Email,
            FullName = user.FullName ?? "",
            PhoneNumber = user.PhoneNumber ?? "",
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if(user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        if(!ModelState.IsValid)
        {
            return RedirectToPage();
        }

        var result = await _mediator.Send(new EditUserInfo.Request(user.Id, Input.FullName, Input.PhoneNumber));
        if(result.Succeeded)
        {
            StatusMessage = "Profile updated successfully!";
            await _signInManager.RefreshSignInAsync(user);
        }

        return RedirectToPage();
    }
    public async Task<IActionResult> OnPostCourierAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        await _mediator.Send(new RequestForCourier.Request(user.Id));
        return RedirectToPage();
    }
}


