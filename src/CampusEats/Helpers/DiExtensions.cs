using CampusEats.Core.Identity.Domain;
using CampusEats.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Helpers;

public static class DiExtensions
{
    public static void AddDbContexts(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<CampusEatsContext>(options => options.UseNpgsql(connectionString));
        services.AddDbContext<IdentityContext>(options => options.UseNpgsql(connectionString));
    }

    public static void AddAuth(this IServiceCollection services, IConfigurationSection authSection)
    {
        services.AddDefaultIdentity<AppUser>(options => options.SignIn.RequireConfirmedAccount = false)
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<IdentityContext>()
            .AddDefaultTokenProviders();

        services.Configure<IdentityOptions>(options =>
        {
            // Lockout settings.
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromSeconds(10);

            // User settings.
            options.User.RequireUniqueEmail = true;
        });

        string AdminRoleName = Roles.Admin.ToString();
        string CourierRoleName = Roles.Courier.ToString();
        string CustomerRoleName = Roles.Customer.ToString();
        services.AddAuthorization(options =>
        {
            options.AddPolicy("Admin", policy => policy.RequireRole(AdminRoleName));
            options.AddPolicy("Courier", policy => policy.RequireRole(AdminRoleName, CourierRoleName));
            options.AddPolicy("Customer", policy => policy.RequireRole(AdminRoleName, CourierRoleName, CustomerRoleName));
        });

        services.AddAuthentication()
           .AddGoogle(options =>
           {
               IConfigurationSection googleAuthNSection = authSection.GetSection("Google");
               options.ClientId = googleAuthNSection["ClientId"];
               options.ClientSecret = googleAuthNSection["ClientSecret"];
           })
           .AddGitHub(options =>
           {
               IConfigurationSection githubAuthNSection = authSection.GetSection("Github");
               options.ClientId = githubAuthNSection["ClientId"];
               options.ClientSecret = githubAuthNSection["ClientSecret"];
           });

        services.ConfigureApplicationCookie(options =>
        {
            // Cookie settings
            options.Cookie.HttpOnly = true;
            options.ExpireTimeSpan = TimeSpan.FromMinutes(30);

            options.LoginPath = "/Identity/Login";
            options.AccessDeniedPath = "/Identity/AccessDenied";
            options.SlidingExpiration = true;
        });
    }
}


