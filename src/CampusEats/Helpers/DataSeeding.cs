using Bogus;
using CampusEats.Core.Identity.Domain;
using CampusEats.Core.Ordering.Domain;
using CampusEats.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Product = CampusEats.Core.Products.Domain.Product;

namespace CampusEats.Helpers;

public static class DataSeeding
{
    public static async Task UseSeedData(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<CampusEatsContext>();
        db.Database.Migrate();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

        await SeedProducts(db);
        await SeedOrders(db, userManager); // Also calls SeedDeliveries
    }

    private static async Task SeedProducts(CampusEatsContext db, int productsToGenerate = 30)
    {
        if(db.Products.Any()) return;

        var lorem = new Bogus.DataSets.Lorem();
        var random = new Randomizer();
        var products = Enumerable.Range(1, productsToGenerate).Select(_ =>
        {
            return new Product(lorem.Sentence(3, 2), random.Decimal(50, 250) + .99m, lorem.Sentence(10, 10));
        }).ToArray();

        db.Products.AddRange(products);
        await db.SaveChangesAsync();
    }

    private static async Task SeedOrders(CampusEatsContext db, UserManager<AppUser> userManager, int ordersToGenerate = 30)
    {
        if(db.Orders.Any()) return;

        var random = new Random();
        var customers = await userManager.GetUsersInRoleAsync(Roles.Customer.ToString()) as List<AppUser>;

        var orders = new Faker<Order>().CustomInstantiator(f =>
        {
            var user = customers!.OrderBy(c => c.Id).Skip(random.Next(0, customers!.Count)).First();
            Location location = new(f.Lorem.Sentence(1), f.Lorem.Sentence(1), f.Lorem.Sentence(1));
            Customer customer = new(user.FullName!, user.Email);

            var startDate = new DateTime(2022, 09, 09);
            var endDate = DateTime.UtcNow;

            TimeSpan timeSpan = endDate - startDate;
            TimeSpan newSpan = new TimeSpan(0, random.Next(0, (int)timeSpan.TotalMinutes), 0);
            DateTime newDate = DateTime.SpecifyKind((startDate + newSpan), DateTimeKind.Utc);

            var order = new Order(location, customer, "");
            order.OrderDate = newDate;
            return order;
        })
        .FinishWith((f, o) =>
        {
            var numOfProducts = db.Products.Count();
            var numOfOrderLines = random.Next(1, 5);
            for(int i = 0; i < numOfOrderLines; i++)
            {
                var product = db.Products.OrderBy(p => p.Id).Skip(random.Next(0, numOfProducts)).First();
                o.AddOrderLine(product.Id, product.Price, random.Next(1, 5), product.Name);
            }

            o.Place();
            if(f.Random.Bool(0.3f))
            {
                o.Cancel();
            }
        }).Generate(ordersToGenerate);

        db.Orders.AddRange(orders);
        await db.SaveChangesAsync();
        await SeedDeliveries(db, userManager, ordersToGenerate / 2);
    }

    private static async Task SeedDeliveries(CampusEatsContext db, UserManager<AppUser> userManager, int deliveriesToAssign = 15)
    {
        if(!db.Deliveries.Any()) return;

        var random = new Random();
        var numOfDeliveries = await db.Deliveries.CountAsync();
        var couriers = await userManager.GetUsersInRoleAsync(Roles.Courier.ToString()) as List<AppUser>;
        for(int i = 0; i < deliveriesToAssign; i++)
        {
            // Assign Courier
            var courier = couriers!.OrderBy(c => c.Id).Skip(random.Next(0, couriers!.Count)).First();
            var delivery = await db.Deliveries.OrderBy(d => d.Id).Skip(random.Next(0, numOfDeliveries)).FirstAsync();
            delivery.AssignCourier(courier.Id, courier.Email);

            // Move corresponding order into next phase(s)
            var steps = random.Next(0, 2);
            if(steps > 0)
            {
                var order = await db.Orders.OrderBy(o => o.Id).FirstOrDefaultAsync(o => o.Id == delivery.OrderId);
                for(var j = 0; j < steps; j++)
                {
                    order!.MoveToNextPhase();
                }
            }
        }
        await db.SaveChangesAsync();
    }
}

public static class IdentityDataSeeding
{
    public static async Task UseIdentitySeedData(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();

        var iDb = scope.ServiceProvider.GetRequiredService<IdentityContext>();
        iDb.Database.Migrate();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        await SeedRoles(roleManager);
        await SeedUsers(userManager);
        await CreateAdmin(userManager);
    }

    private static async Task SeedRoles(RoleManager<IdentityRole> roleManager)
    {
        if(roleManager.Roles.Any()) return;

        var roleNames = Enum.GetNames(typeof(Roles));
        foreach(var roleName in roleNames)
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    private static async Task SeedUsers(UserManager<AppUser> userManager, string password = "Test1234!")
    {
        if(userManager.Users.Count() > 5)
            return;

        var random = new Random();
        var users = new Faker<AppUser>(locale: "nb_NO")
                .RuleFor(u => u.FullName, f => f.Name.FullName())
                .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.FullName!.Split(" ").FirstOrDefault(), u.FullName.Split(" ").LastOrDefault()))
                .RuleFor(u => u.UserName, (_, u) => u.Email)
                .RuleFor(u => u.PhoneNumber, f => f.Phone.PhoneNumber())
                .FinishWith((f, u) =>
                {
                    Console.WriteLine("User Created! Id={0}, email={1}", u.Id, u.Email);
                }).Generate(20);

        foreach(var user in users)
        {
            await userManager.CreateAsync(user, password);
            var randRole = Enum.GetName(typeof(Roles), random.Next(1, 3));
            if(randRole != Roles.Customer.ToString())
            {
                user.RequestStatus = RequestStatus.Confirmed;
            }
            else
            {
                user.RequestStatus = (RequestStatus)random.Next(0, 3);
                if(user.RequestStatus == RequestStatus.Confirmed)
                {
                    user.RequestStatus = null;
                }
            }

            await userManager.AddToRoleAsync(user, randRole);
            await userManager.UpdateAsync(user);
        }
    }

    private static async Task CreateAdmin(UserManager<AppUser> userManager, string password = "Test-123")
    {
        var adminEmail = "admin@admin.com";
        if(!userManager.Users.Any(u => u.Email == adminEmail))
        {
            var admin = new AppUser
            {
                Id = "9d08ab94-b1f8-44e7-b76d-f37dc6b3a23d", // please keep this
                FullName = "admin",
                Email = adminEmail,
                UserName = adminEmail,
                PhoneNumber = "6969696969",
                FirstLogin = true,
                RequestStatus = null,
            };

            await userManager.CreateAsync(admin, password);
            await userManager.AddToRoleAsync(admin, Roles.Admin.ToString());
        }
    }
}
