using Hangfire;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Rent2Read.Web.BackgroundTasks;
using Rent2Read.Web.Core.Mapping;
using Rent2Read.Web.Helpers;
using Rent2Read.Web.Seeds;

using System.Reflection;
using UoN.ExpressiveAnnotations.NetCore.DependencyInjection;
using WhatsAppCloudApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddWhatsAppApiClient(builder.Configuration);


// Enable ASP.NET Core Data Protection (used for encrypting cookies, TempData, etc.)
// Set a unique ApplicationName = "Rent2Read" to avoid key conflicts when sharing the same key storage
builder.Services.AddDataProtection().SetApplicationName(nameof(Rent2Read));


/*builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();*/

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
    //RequireConfirmedAccount = true → users must confirm their email before signing in
    .AddEntityFrameworkStores<ApplicationDbContext>()//store user/role data in the database
    .AddDefaultUI()// use the default Identity UI pages (Login, Register, ForgotPassword, etc.)
    .AddDefaultTokenProviders();//enable token generation for email confirmation, password reset, etc.

//The SecurityStampValidator is responsible for periodically verifying that the user's current SecurityStamp
//(stored in a cookie or token) is still valid and matches the SecurityStamp in the database.
//When the Validator detects a discrepancy it:Signs out the user.Forces it to log in again.
builder.Services.Configure<SecurityStampValidatorOptions>(options =>
   options.ValidationInterval = TimeSpan.Zero);
//The ValidationInterval specifies the period of time that the Identity will wait before re-verifying the SecurityStamp.
//TimeSpan.Zero:Verified on every request.This means that any changes made to the user (such as lockout or password reset) will be applied immediately, without waiting.


// Configure Hangfire to use SQL Server as storage for background jobs
builder.Services.AddHangfire(x => x.UseSqlServerStorage(connectionString));

// Enable the Hangfire server that will execute the background jobs
builder.Services.AddHangfireServer();
builder.Services.Configure<AuthorizationOptions>(options =>
    // Define a new authorization policy named "AdminOnly"
    options.AddPolicy("AdminsOnly", policy =>
    {
        // Require that the user must be authenticated (logged in)
        policy.RequireAuthenticatedUser();
        // Require that the user must have the "Admin" role
        policy.RequireRole(AppRoles.Admin);
    })
);

builder.Services.Configure<IdentityOptions>(options =>
{
    /*// Default Password settings.
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;*/
    options.Password.RequiredLength = 8;
    /*  options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(1);
      options.Lockout.MaxFailedAccessAttempts = 1;
      Default User settings.
       options.User.AllowedUserNameCharacters =
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";*/
    options.User.RequireUniqueEmail = false;
});

//It registers the ApplicationUserClaims.. n the Dependency Injection Container
//so that ASP.NET Core Identity will use the Custom Factory I created instead of using the Default UserClaimsPrincipalFactory.
builder.Services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, ApplicationUserClaimsPrincipalFactory>();
builder.Services.AddTransient<IImageService, ImageService>();
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddTransient<IEmailBody, EmailBody>();

builder.Services.AddControllersWithViews();

// Register the ExpressiveAnnotations library for advanced model validation using expressions (e.g., [RequiredIf], [AssertThat])
builder.Services.AddExpressiveAnnotations();

//Configuration Binding of CloudinarySettings class with data in appsettings.json file 
builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));
builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));
//link MailSettings class to the "MailSettings" section in the appsettings.json


//builder.Services.AddAutoMapper(typeof(MappingProfile));//Registers only from the given type and nearby classes. Limited scope.
builder.Services.AddAutoMapper(Assembly.GetAssembly(typeof(MappingProfile)));
//Scans the entire assembly for all classes inheriting from Profile. Broader and safer.

var app = builder.Build();

// Configure the HTTP request pipeline. 
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();


//This code usually runs at application startup to guarantee that the system has the roles and admin user required to work correctly.
var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();
//creates a temporary scope so we can access scoped services (like RoleManager) outside of normal request handling.
using var scope = scopeFactory.CreateScope();

var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

await DefaultRoles.SeedRolesAsync(roleManager);
await DefaultUsers.SeedAdminUserAsync(userManager);

//Hangfire
/*
 * Enable Hangfire Dashboard at "/hangfire" to monitor
 *and manage background jobs with custom title and set it to ReadOnly mode
 *This allows monitoring background jobs without giving permissions to modify or delete them 
 */
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    DashboardTitle = "Rent2Read Dashboard",
    //IsReadOnlyFunc = (DashboardContext context) => true,
    Authorization = new IDashboardAuthorizationFilter[]
    {
        new HangfireAuthorizationFilter("AdminsOnly")
    }
});
// Register a recurring job with Hangfire to send subscription expiration alerts.
// This job runs every day at 2:00 PM (local time) and calls the PrepareExpirationAlert method.
var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
var emailBody = scope.ServiceProvider.GetRequiredService<IEmailBody>();
var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();

var hangfireTasks = new HangfireTasks(dbContext, emailBody, emailSender);
RecurringJob.AddOrUpdate(
    recurringJobId: "PrepareExpirationAlertJob",   // Unique ID for the recurring job
    methodCall: () => hangfireTasks.PrepareExpirationAlert(),
    cronExpression: "0 14 * * *",                  // Run every day at 14:00
    options: new RecurringJobOptions
    {
        TimeZone = TimeZoneInfo.Local
    });

RecurringJob.AddOrUpdate(
    recurringJobId: "RentalsExpirationAlertJob",
    methodCall: () => hangfireTasks.RentalsExpirationAlert(),
    cronExpression: "0 14 * * *",
    options: new RecurringJobOptions
    {
        TimeZone = TimeZoneInfo.Local
    });







app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
