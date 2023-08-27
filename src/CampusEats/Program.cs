using CampusEats.Core.Common;
using CampusEats.Core.Identity.Application.Services;
using CampusEats.Core.Notifications.Application.Services;
using CampusEats.Core.Ordering.Application.Services;
using CampusEats.Core.Ordering.Domain;
using CampusEats.Core.Ordering.Domain.Validators;
using CampusEats.Core.Products.Domain.Dto;
using CampusEats.Core.Products.Domain.Validators;
using CampusEats.Helpers;
using MediatR;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Stripe;
using System.Globalization;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

// Add services to the container.
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromSeconds(60); // Tiden er kort for testing
    options.Cookie.Name = ".CampusEatsContext.Session"; // Lagt til
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddScoped<IValidator<ProductDto>, FoodItemNameValidator>();
builder.Services.AddScoped<IValidator<ProductDto>, FoodItemDescriptionValidator>();
builder.Services.AddScoped<IValidator<ProductDto>, FoodItemPriceValidator>();

builder.Services.AddScoped<IValidator<Location>, LocationBuildingValidator>();
builder.Services.AddScoped<IValidator<Location>, LocationRoomNumberValidator>();
builder.Services.AddScoped<IValidator<Location>, LocationNotesValidator>();

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AllowAnonymousToPage("/Index");
    options.Conventions.AllowAnonymousToFolder("/Cart");

    options.Conventions.AuthorizePage("/Identity/Profile", "Customer");
    options.Conventions.AuthorizePage("/Identity/Logout", "Customer");
    options.Conventions.AuthorizeFolder("/Orders", "Customer");

    options.Conventions.AuthorizeFolder("/Deliveries", "Courier");

    options.Conventions.AllowAnonymousToPage("/Admin/Login");
    options.Conventions.AuthorizeFolder("/Products", "Admin");
    options.Conventions.AuthorizeFolder("/Admin", "Admin");

});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddDbContexts(builder.Configuration.GetConnectionString("DefaultConnection"));
builder.Services.AddAuth(builder.Configuration.GetSection("Authentication"));

builder.Services.AddMediatR(Assembly.GetExecutingAssembly());
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
builder.Services.AddScoped<IOrderingService, OrderingService>();

builder.Services.AddTransient<IEmailSender, EmailSender>();

builder.Services.ConfigureApplicationCookie(o =>
{
    o.ExpireTimeSpan = TimeSpan.FromDays(5);
    o.SlidingExpiration = true;
});

var app = builder.Build();

if(!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    await app.UseIdentitySeedData();
    await app.UseSeedData();
}
app.UseExceptionHandler("/Error");

var supportedCultures = new[]
{
    new CultureInfo("nb-NO"),
};

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("nb-NO"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

app.Use((context, next) =>
{
    context.Request.Scheme = "https";
    return next(context);
});

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

app.MapRazorPages();

app.Run();

public partial class Program { }
