using Microsoft.EntityFrameworkCore;
using UbuhlebethuConnectPro.Web.Data;
using Microsoft.AspNetCore.Identity;

namespace UbuhlebethuConnectPro.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Configure EF Core SQL Server and Identity
            builder.Services.AddDbContext<UbuhlebethuConnectPro.Web.Data.ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddIdentity<Microsoft.AspNetCore.Identity.IdentityUser, Microsoft.AspNetCore.Identity.IdentityRole>(options =>
            {
                // Relax password for prototype convenience
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
            })
            .AddEntityFrameworkStores<UbuhlebethuConnectPro.Web.Data.ApplicationDbContext>()
            .AddDefaultTokenProviders();

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/AccessDenied";
            });

            var app = builder.Build();

            // Seed roles and a default admin user
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var db = services.GetRequiredService<UbuhlebethuConnectPro.Web.Data.ApplicationDbContext>();
                // apply migrations
                db.Database.Migrate();

                var roleManager = services.GetRequiredService<Microsoft.AspNetCore.Identity.RoleManager<Microsoft.AspNetCore.Identity.IdentityRole>>();
                var userManager = services.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<Microsoft.AspNetCore.Identity.IdentityUser>>();

                string[] roles = new[] { "Admin", "Office", "Client", "SiteManager", "Management" };
                foreach (var role in roles)
                {
                    if (!roleManager.RoleExistsAsync(role).GetAwaiter().GetResult())
                    {
                        roleManager.CreateAsync(new Microsoft.AspNetCore.Identity.IdentityRole(role)).GetAwaiter().GetResult();
                    }
                }

                // Create several sample users for prototype roles
                var sampleUsers = new[]
                {
                    (UserName: "admin", Email: "admin@connectpro.local", Password: "admin123", Role: "Admin"),
                    (UserName: "office", Email: "office@connectpro.local", Password: "office123", Role: "Office"),
                    (UserName: "client", Email: "client@connectpro.local", Password: "client123", Role: "Client"),
                    (UserName: "manager", Email: "manager@connectpro.local", Password: "manager123", Role: "Management"),
                    (UserName: "site", Email: "site@connectpro.local", Password: "site123", Role: "SiteManager")
                };

                foreach (var su in sampleUsers)
                {
                    var existing = userManager.FindByNameAsync(su.UserName).GetAwaiter().GetResult();
                    if (existing == null)
                    {
                        var newUser = new Microsoft.AspNetCore.Identity.IdentityUser
                        {
                            UserName = su.UserName,
                            Email = su.Email,
                            EmailConfirmed = true
                        };
                        var createRes = userManager.CreateAsync(newUser, su.Password).GetAwaiter().GetResult();
                        if (createRes.Succeeded)
                        {
                            userManager.AddToRoleAsync(newUser, su.Role).GetAwaiter().GetResult();
                        }
                    }
                }

                // Seed some realistic mock QuoteRequests for demonstration if none exist
                if (!db.QuoteRequests.Any())
                {
                    db.QuoteRequests.AddRange(
                        new UbuhlebethuConnectPro.Web.Models.QuoteRequest
                        {
                            ClientName = "Steve MacDonald",
                            Email = "steve@example.com",
                            Phone = "082 555 1234",
                            ProjectType = "Commercial Build",
                            SiteLocation = "Sandton, Gauteng",
                            EstimatedBudget = 4500000m,
                            ProjectDescription = "Construction of a 3-story office extension.",
                            RequestDate = DateTime.Now.AddDays(-14),
                            Status = "Approved",
                            Reference = "UBU-4592",
                            CreatedBy = "client",
                            IsPaid = false
                        },
                        new UbuhlebethuConnectPro.Web.Models.QuoteRequest
                        {
                            ClientName = "Mogale City Municipality",
                            Email = "procurement@mogalecity.gov.za",
                            Phone = "011 951 2000",
                            ProjectType = "Residential Complex",
                            SiteLocation = "Krugersdorp",
                            EstimatedBudget = 12500000m,
                            ProjectDescription = "Phase 3 of affordable housing units (50 units).",
                            RequestDate = DateTime.Now.AddDays(-5),
                            Status = "Submitted",
                            Reference = "UBU-8810",
                            CreatedBy = "admin",
                            IsPaid = false
                        },
                        new UbuhlebethuConnectPro.Web.Models.QuoteRequest
                        {
                            ClientName = "Transnet Logistics",
                            Email = "facilities@transnet.net",
                            Phone = "031 361 3000",
                            ProjectType = "Industrial",
                            SiteLocation = "Durban Port",
                            EstimatedBudget = 28000000m,
                            ProjectDescription = "Logistics hub warehouse construction.",
                            RequestDate = DateTime.Now.AddDays(-2),
                            Status = "Draft",
                            Reference = "UBU-1029",
                            CreatedBy = "client",
                            IsPaid = false
                        }
                    );
                    db.SaveChanges();
                }
            }

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}
