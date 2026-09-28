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

            // =========================================================
            // SERVICES
            // =========================================================

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddSignalR();

            

            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(10);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            });

            // Configure EF Core SQL Server and Identity
            builder.Services.AddDbContext<UbuhlebethuConnectPro.Web.Data.ApplicationDbContext>(
                options =>
                    options.UseSqlServer(
                        builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddIdentity<
                Microsoft.AspNetCore.Identity.IdentityUser,
                Microsoft.AspNetCore.Identity.IdentityRole>(options =>
                {
                    // -------------------------------------------------
                    // Password settings
                    // -------------------------------------------------

                    // Relax password requirements for prototype convenience
                    options.Password.RequireDigit = false;
                    options.Password.RequiredLength = 6;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireLowercase = false;

                    // -------------------------------------------------
                    // Lockout settings
                    // -------------------------------------------------

                    options.Lockout.DefaultLockoutTimeSpan =
                        TimeSpan.FromMinutes(5);

                    options.Lockout.MaxFailedAccessAttempts = 5;

                    options.Lockout.AllowedForNewUsers = true;
                })
                .AddEntityFrameworkStores<
                    UbuhlebethuConnectPro.Web.Data.ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // =========================================================
            // APPLICATION COOKIE & JWT
            // =========================================================

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/AccessDenied";
            });

            builder.Services.AddAuthentication()
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "UbuhleBethu",
                        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "UbuhleBethuApp",
                        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                            System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "ThisIsASecretKeyForUbuhleBethuConnectPro12345!"))
                    };
                });

            var app = builder.Build();

            // =========================================================
            // DATABASE / ROLE / USER SEEDING
            // =========================================================

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                var db =
                    services.GetRequiredService<
                        UbuhlebethuConnectPro.Web.Data.ApplicationDbContext>();

                // Apply migrations
                db.Database.Migrate();

                var roleManager =
                    services.GetRequiredService<
                        Microsoft.AspNetCore.Identity.RoleManager<
                            Microsoft.AspNetCore.Identity.IdentityRole>>();

                var userManager =
                    services.GetRequiredService<
                        Microsoft.AspNetCore.Identity.UserManager<
                            Microsoft.AspNetCore.Identity.IdentityUser>>();

                // -----------------------------------------------------
                // Create application roles
                // -----------------------------------------------------

                string[] roles =
                {
                    "Admin",
                    "Office",
                    "Client",
                    "SiteManager",
                    "Management"
                };

                foreach (var role in roles)
                {
                    if (!roleManager
                        .RoleExistsAsync(role)
                        .GetAwaiter()
                        .GetResult())
                    {
                        roleManager
                            .CreateAsync(
                                new Microsoft.AspNetCore.Identity.IdentityRole(role))
                            .GetAwaiter()
                            .GetResult();
                    }
                }

                // -----------------------------------------------------
                // Create sample users for prototype roles
                // -----------------------------------------------------

                var sampleUsers = new[]
                {
                    (
                        UserName: "admin",
                        Email: "admin@connectpro.local",
                        Password: "admin123",
                        Role: "Admin"
                    ),
                    (
                        UserName: "office",
                        Email: "office@connectpro.local",
                        Password: "office123",
                        Role: "Office"
                    ),
                    (
                        UserName: "client",
                        Email: "client@connectpro.local",
                        Password: "client123",
                        Role: "Client"
                    ),
                    (
                        UserName: "manager",
                        Email: "manager@connectpro.local",
                        Password: "manager123",
                        Role: "Management"
                    ),
                    (
                        UserName: "site",
                        Email: "site@connectpro.local",
                        Password: "site123",
                        Role: "SiteManager"
                    )
                };

                foreach (var su in sampleUsers)
                {
                    var existing =
                        userManager
                            .FindByNameAsync(su.UserName)
                            .GetAwaiter()
                            .GetResult();

                    if (existing == null)
                    {
                        var newUser =
                            new Microsoft.AspNetCore.Identity.IdentityUser
                            {
                                UserName = su.UserName,
                                Email = su.Email,
                                EmailConfirmed = true
                            };

                        var createRes =
                            userManager
                                .CreateAsync(newUser, su.Password)
                                .GetAwaiter()
                                .GetResult();

                        if (createRes.Succeeded)
                        {
                            userManager
                                .AddToRoleAsync(newUser, su.Role)
                                .GetAwaiter()
                                .GetResult();
                        }
                    }
                }

                // -----------------------------------------------------
                // Seed realistic mock QuoteRequests
                // -----------------------------------------------------

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
                            ProjectDescription =
                                "Construction of a 3-story office extension.",
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
                            ProjectDescription =
                                "Phase 3 of affordable housing units (50 units).",
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
                            ProjectDescription =
                                "Logistics hub warehouse construction.",
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

            // =========================================================
            // HTTP REQUEST PIPELINE
            // =========================================================

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");

                // The default HSTS value is 30 days.
                // You may want to change this for production scenarios.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseSession();

            // =========================================================
            // AUTHENTICATION
            // =========================================================

            app.UseAuthentication();
            // =========================================================
            // MFA ENFORCEMENT
            // =========================================================
            //
            // If a user has authenticated with their password but has
            // not configured MFA, they are required to complete MFA
            // setup before accessing protected application pages.
            //
            // Static files such as CSS, JavaScript, images and fonts
            // are explicitly allowed through so the application's
            // normal styling and branding continue to work.
            // =========================================================

            app.Use(async (context, next) =>
            {
                var path = context.Request.Path;

                // ---------------------------------------------------------
                // Allow static files through without MFA interception
                // ---------------------------------------------------------

                var isStaticFile =
                    path.StartsWithSegments("/css") ||
                    path.StartsWithSegments("/js") ||
                    path.StartsWithSegments("/images") ||
                    path.StartsWithSegments("/fonts") ||
                    path.StartsWithSegments("/lib") ||
                    path.StartsWithSegments("/favicon.ico") ||
                    path.StartsWithSegments("/_content");

                if (isStaticFile)
                {
                    await next();
                    return;
                }

                // ---------------------------------------------------------
                // Check authenticated user's MFA status
                // ---------------------------------------------------------

                if (context.User.Identity?.IsAuthenticated == true)
                {
                    var userManager =
                        context.RequestServices
                            .GetRequiredService<UserManager<IdentityUser>>();

                    var user =
                        await userManager.GetUserAsync(context.User);

                    if (user != null && !user.TwoFactorEnabled)
                    {
                        // -------------------------------------------------
                        // Pages allowed during MFA setup
                        // -------------------------------------------------

                        var allowedPaths = new[]
                        {
                "/Account/SetupMfa",
                "/Account/EnableMfa",
                "/Account/MfaEnabled",
                "/Account/Logout",
                "/Account/AccessDenied"
            };

                        var isAllowedPath =
                            allowedPaths.Any(
                                allowedPath =>
                                    path.StartsWithSegments(allowedPath));

                        // -------------------------------------------------
                        // Redirect everything else to MFA setup
                        // -------------------------------------------------

                        if (!isAllowedPath)
                        {
                            context.Response.Redirect(
                                "/Account/SetupMfa");

                            return;
                        }
                    }
                }

                await next();
            });

            // =========================================================
            // AUTHORIZATION
            // =========================================================

            app.UseAuthorization();

            // =========================================================
            // STATIC FILES / ROUTING / HUBS
            // =========================================================

            app.MapStaticAssets();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.MapHub<UbuhlebethuConnectPro.Web.Hubs.SyncHub>("/synchub");

            // =========================================================
            // RUN APPLICATION
            // =========================================================

            app.Run();
        }
    }
}