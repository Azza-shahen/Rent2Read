using Hangfire;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Rent2Read.Infrastructure;
using Rent2Read.Web;
using Rent2Read.Web.BackgroundTasks;
using Rent2Read.Web.Seeds;
using Serilog;
using Serilog.Context;



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddWebServices(builder);


//Add Serilog(a logging library)
Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).CreateLogger();
builder.Host.UseSerilog();

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
app.UseStatusCodePagesWithReExecute("/Home/Error", "?statusCode={0}");
app.UseExceptionHandler("/Home/Error");

app.UseHttpsRedirection();
app.UseStaticFiles();

// Configure cookie policy for the application
app.UseCookiePolicy(new CookiePolicyOptions
{
    // Force all cookies to be sent only over HTTPS (never over HTTP).
    // This helps protect cookies from being intercepted during transmission (man-in-the-middle attacks).
    Secure = CookieSecurePolicy.Always
});
app.Use(async (dbContext, next) =>
{
    dbContext.Response.Headers["X-Frame-Options"] = "Deny";

    await next();
});


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


app.Use(async (dbContext, next) =>
{
    // Push a property called "UserId" into the Serilog LogContext
    // It reads the current logged-in user's ID from the ClaimsPrincipal (authentication system)
    LogContext.PushProperty("UserId", dbContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

    LogContext.PushProperty("UserName", dbContext.User.FindFirst(ClaimTypes.Name)?.Value);

    await next(); // Call the next middleware in the pipeline
});

// Enable Serilog's built-in request logging middleware This automatically logs details about each HTTP request (method, path, status code, timing, etc.)
app.UseSerilogRequestLogging();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
