using CampusEats.Core.Products.Application;
using CampusEats.Core.Products.Domain.Dto;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Core.Pages.Products;

public sealed class CreateModel : PageModel
{
    private readonly IMediator _mediator;

    public CreateModel(IMediator mediator)
    {
        _mediator = mediator ?? throw new System.ArgumentNullException(nameof(mediator));
    }

    [BindProperty]
    public ProductDto ProductDto { get; set; } = new ProductDto();
    public string[] Errors { get; private set; } = Array.Empty<string>();

    public async Task<IActionResult> OnPostAsync()
    {
        var response = await _mediator.Send(new CreateProduct.Request(ProductDto));
        if(response.Success) return RedirectToPage("Edit", new { Id = response.Data });

        Errors = response.Errors;
        return Page();
    }
}
