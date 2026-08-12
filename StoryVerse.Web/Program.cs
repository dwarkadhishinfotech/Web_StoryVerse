//using Microsoft.AspNetCore.Identity;
//using Microsoft.EntityFrameworkCore;
//using Serilog;
//using StoryVerse.Core.Entities.Identity;
//using StoryVerse.Infrastructure.Data;

//var builder = WebApplication.CreateBuilder(args);

//// Configure Serilog
//Log.Logger = new LoggerConfiguration()
//    .ReadFrom.Configuration(builder.Configuration)
//    .Enrich.FromLogContext()
//    .CreateLogger();

//builder.Host.UseSerilog();

//try
//{
//    Log.Information("Starting StoryVerse Web Application");

//    // Add services to the container.
//    builder.Services.AddControllersWithViews();
//    builder.Services.AddScoped<StoryVerse.Web.Services.IDropdownService, StoryVerse.Web.Services.DropdownService>();

//    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
//        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

//    builder.Services.AddDbContext<ApplicationDbContext>(options =>
//        options.UseSqlServer(connectionString));

//    builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => 
//    {
//        // Password settings
//        options.Password.RequireDigit = true;
//        options.Password.RequireLowercase = true;
//        options.Password.RequireNonAlphanumeric = true;
//        options.Password.RequireUppercase = true;
//        options.Password.RequiredLength = 8;
//        options.Password.RequiredUniqueChars = 1;

//        // Lockout settings
//        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
//        options.Lockout.MaxFailedAccessAttempts = 5;
//        options.Lockout.AllowedForNewUsers = true;

//        // User settings
//        options.User.RequireUniqueEmail = true;
//        options.SignIn.RequireConfirmedAccount = false; // We can set this to true later when email verification is fully implemented
//    })
//    .AddEntityFrameworkStores<ApplicationDbContext>()
//    .AddDefaultTokenProviders();

//    builder.Services.ConfigureApplicationCookie(options =>
//    {
//        options.Cookie.HttpOnly = true;
//        options.ExpireTimeSpan = TimeSpan.FromDays(30);
//        options.LoginPath = "/login";
//        options.AccessDeniedPath = "/Identity/Account/AccessDenied";
//        options.SlidingExpiration = true;
//    });

//    var app = builder.Build();

//    // Configure the HTTP request pipeline.
//    if (!app.Environment.IsDevelopment())
//    {
//        app.UseExceptionHandler("/Home/Error");
//        app.UseHsts();
//    }

//    //app.UseHttpsRedirection();
//    app.UseStaticFiles();

//    app.UseRouting();

//    app.UseAuthentication();
//    app.UseAuthorization();

//    app.MapControllerRoute(
//        name: "areas",
//        pattern: "{area:exists}/{controller=Account}/{action=Login}/{id?}");

//    app.MapControllerRoute(
//        name: "default",
//        pattern: "{controller=Home}/{action=Index}/{id?}");

//    // Seed Roles and Data
//    using (var scope = app.Services.CreateScope())
//    {
//        var services = scope.ServiceProvider;
//        try
//        {
//            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
//            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
//            var context = services.GetRequiredService<ApplicationDbContext>();
//            //await context.Database.MigrateAsync();

//            await StoryVerse.Infrastructure.Data.DbSeeder.SeedRolesAsync(roleManager);
//            await StoryVerse.Infrastructure.Data.DbSeeder.SeedDataAsync(context, userManager);
//        }
//        catch (Exception ex)
//        {
//            Log.Error(ex, "An error occurred while seeding the database.");
//        }
//    }

//    app.Run();
//}
//catch (Exception ex) when (ex.GetType().Name != "HostAbortedException")
//{
//    Log.Fatal(ex, "Application terminated unexpectedly");
//}
//finally
//{
//    Log.CloseAndFlush();
//}

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using StoryVerse.Core.Entities.Identity;
using StoryVerse.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// SERILOG
// ============================================================

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();


// ============================================================
// SERVICES
// ============================================================

// MVC
builder.Services.AddControllersWithViews();

// Application Services
builder.Services.AddScoped<
    StoryVerse.Web.Services.IDropdownService,
    StoryVerse.Web.Services.DropdownService>();
builder.Services.AddScoped<
    StoryVerse.Web.Services.IQuoteService,
    StoryVerse.Web.Services.QuoteService>();
builder.Services.AddScoped<
    StoryVerse.Web.Services.IActiveStoryService,
    StoryVerse.Web.Services.ActiveStoryService>();


// ============================================================
// DATABASE
// ============================================================

var connectionString = builder.Configuration
    .GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));


// ============================================================
// ASP.NET CORE IDENTITY
// ============================================================

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;
    options.Password.RequiredUniqueChars = 1;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan =
        TimeSpan.FromMinutes(15);

    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.RequireUniqueEmail = true;

    // Email confirmation
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// ============================================================
// EXTERNAL AUTHENTICATION PROVIDERS
// ============================================================

builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? "YOUR_GOOGLE_CLIENT_ID";
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? "YOUR_GOOGLE_CLIENT_SECRET";
    });



// ============================================================
// AUTHENTICATION COOKIE
// ============================================================

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;

    // SET TO 1 HOUR SESSION TIMEOUT
    options.ExpireTimeSpan = TimeSpan.FromHours(1);

    options.LoginPath = "/login";

    options.AccessDeniedPath =
        "/Identity/Account/AccessDenied";

    options.SlidingExpiration = true;
});


// ============================================================
// BUILD APPLICATION
// ============================================================

var app = builder.Build();


// ============================================================
// HTTP REQUEST PIPELINE
// ============================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");

    // Enable after HTTPS/SSL is configured.
    // app.UseHsts();
}

// Enable after HTTPS/SSL is configured.
// app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();


// ============================================================
// ROUTING
// ============================================================

// Areas
app.MapControllerRoute(
    name: "areas",
    pattern:
        "{area:exists}/{controller=Account}/{action=Login}/{id?}");

// Default
app.MapControllerRoute(
    name: "default",
    pattern:
        "{controller=Home}/{action=Index}/{id?}");


// Ensure database schema matches new domain columns (e.g. MonthlyWordCountGoal in DI_TRN_UserGoals, Content in DI_TRN_WebChapters)
using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.Database.ExecuteSqlRaw(@"
            IF EXISTS (SELECT 1 FROM sys.tables WHERE name = N'DI_TRN_UserGoals')
            BEGIN
                IF NOT EXISTS (
                    SELECT 1 FROM sys.columns 
                    WHERE object_id = OBJECT_ID(N'[DI_TRN_UserGoals]') 
                    AND name = N'MonthlyWordCountGoal'
                )
                BEGIN
                    ALTER TABLE [DI_TRN_UserGoals] ADD [MonthlyWordCountGoal] INT NOT NULL DEFAULT 50000;
                END
            END

            IF EXISTS (SELECT 1 FROM sys.tables WHERE name = N'DI_TRN_WebChapters')
            BEGIN
                IF NOT EXISTS (
                    SELECT 1 FROM sys.columns 
                    WHERE object_id = OBJECT_ID(N'[DI_TRN_WebChapters]') 
                    AND name = N'Content'
                )
                BEGIN
                    ALTER TABLE [DI_TRN_WebChapters] ADD [Content] NVARCHAR(MAX) NULL;
                END
            END
        ");
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "Automatic schema migration check for database columns failed.");
    }
}

Log.Information("Starting StoryVerse Web Application");

app.Run();