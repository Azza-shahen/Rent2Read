using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Rent2Read.Web.Core.Mapping;
using Rent2Read.Web.Data;
using Rent2Read.Web.Helpers;
using Rent2Read.Web.Seeds;
using System.Reflection;
using UoN.ExpressiveAnnotations.NetCore.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

/*builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();*/

builder.Services.AddIdentity<ApplicationUser,IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
   //RequireConfirmedAccount = true → users must confirm their email before signing in
    .AddEntityFrameworkStores<ApplicationDbContext>()//store user/role data in the database
    .AddDefaultUI()// use the default Identity UI pages (Login, Register, ForgotPassword, etc.)
    .AddDefaultTokenProviders();//enable token generation for email confirmation, password reset, etc.

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
    options.Lockout.MaxFailedAccessAttempts = 1;*/
    // Default User settings.
    /*  options.User.AllowedUserNameCharacters =
              "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";*/
    options.User.RequireUniqueEmail = false;
});

//It registers the ApplicationUserClaims.. n the Dependency Injection Container
//so that ASP.NET Core Identity will use the Custom Factory I created instead of using the Default UserClaimsPrincipalFactory.
builder.Services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, ApplicationUserClaimsPrincipalFactory>();


builder.Services.AddControllersWithViews();

// Register the ExpressiveAnnotations library for advanced model validation using expressions (e.g., [RequiredIf], [AssertThat])
builder.Services.AddExpressiveAnnotations();

//Configuration Binding of CloudinarySettings class with data in appsettings.json file 
builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));

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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
