using CampusEats.Core.Common;
using CampusEats.Core.Products.Application;
using CampusEats.Core.Products.Domain.Dto;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Core.Pages.Products;

public sealed class EditModel : PageModel
{
    private readonly IMediator _mediator;


    public EditModel(IMediator mediator)
    {
        _mediator = mediator ?? throw new System.ArgumentNullException(nameof(mediator));
    }
    [BindProperty]
    public ProductDto Product { get; set; } = new();
    public string[] Errors { get; private set; } = Array.Empty<string>();

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        try
        {
            Product = await _mediator.Send(new GetProductById.Request(id));
            return Page();
        }
        catch(EntityNotFoundException)
        {
            return NotFound();
        }
    }

    public async Task<IActionResult> OnPostAsync(Guid id)
    {
        var result = await _mediator.Send(new EditProduct.Request(Product));
        if(result.Success) return RedirectToPage("/Products/Index");

        Errors = result.Errors;
        return Page();
    }
}
