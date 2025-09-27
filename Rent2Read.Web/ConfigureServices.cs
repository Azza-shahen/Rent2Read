using Hangfire;
using HashidsNet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Rent2Read.Web.Core.Mapping;
using Rent2Read.Web.Helpers;
using System.Reflection;
using UoN.ExpressiveAnnotations.NetCore.DependencyInjection;

namespace Rent2Read.Web
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddWebServices(this IServiceCollection services,
            WebApplicationBuilder builder)
        {
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
           
            services.AddDatabaseDeveloperPageExceptionFilter();

            /* services.AddWhatsAppApiClient(builder.Configuration);*/


            // Enable ASP.NET Core Data Protection (used for encrypting cookies, TempData, etc.)
            // Set a unique ApplicationName = "Rent2Read" to avoid key conflicts when sharing the same key storage
            services.AddDataProtection().SetApplicationName(nameof(Rent2Read));
            services.AddSingleton<IHashids>(_ => new Hashids("f1nd1ngn3m0", minHashLength: 11));


            /*services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<IApplicationDbContext>();*/

            services.AddIdentity<ApplicationUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
                //RequireConfirmedAccount = true → users must confirm their email before signing in
                .AddEntityFrameworkStores<ApplicationDbContext>()//store user/role data in the database
                .AddDefaultUI()// use the default Identity UI pages (Login, Register, ForgotPassword, etc.)
                .AddDefaultTokenProviders();//enable token generation for email confirmation, password reset, etc.



            //The SecurityStampValidator is responsible for periodically verifying that the user's current SecurityStamp
            //(stored in a cookie or token) is still valid and matches the SecurityStamp in the database.
            //When the Validator detects a discrepancy it:Signs out the user.Forces it to log in again.
            services.Configure<SecurityStampValidatorOptions>(options =>
               options.ValidationInterval = TimeSpan.Zero);
            //The ValidationInterval specifies the period of time that the Identity will wait before re-verifying the SecurityStamp.
            //TimeSpan.Zero:Verified on every request.This means that any changes made to the user (such as lockout or password reset) will be applied immediately, without waiting.


            // Configure Hangfire to use SQL Server as storage for background jobs
            services.AddHangfire(x => x.UseSqlServerStorage(connectionString));

            // Enable the Hangfire server that will execute the background jobs
            services.AddHangfireServer();
            services.Configure<AuthorizationOptions>(options =>
                // Define a new authorization policy named "AdminOnly"
                options.AddPolicy("AdminsOnly", policy =>
                {
                    // Require that the user must be authenticated (logged in)
                    policy.RequireAuthenticatedUser();
                    // Require that the user must have the "Admin" role
                    policy.RequireRole(AppRoles.Admin);
                })
            );

            services.Configure<IdentityOptions>(options =>
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
            services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, ApplicationUserClaimsPrincipalFactory>();
            services.AddTransient<IImageService, ImageService>();
            services.AddTransient<IEmailSender, EmailSender>();
            services.AddTransient<IEmailBody, EmailBody>();

            services.AddControllersWithViews();

            // Register the ExpressiveAnnotations library for advanced model validation using expressions (e.g., [RequiredIf], [AssertThat])
            services.AddExpressiveAnnotations();

            //Configuration Binding of CloudinarySettings class with data in appsettings.json file 
            services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));
            services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));
            //link MailSettings class to the "MailSettings" section in the appsettings.json


            //services.AddAutoMapper(typeof(MappingProfile));//Registers only from the given type and nearby classes. Limited scope.
            services.AddAutoMapper(Assembly.GetAssembly(typeof(MappingProfile)));
            //Scans the entire assembly for all classes inheriting from Profile. Broader and safer.


            services.AddMvc(options
                // Add a global filter will automatically check for Anti-Forgery tokens on unsafe HTTP methods (POST, PUT, DELETE).protect the app from CSRF attacks.
                => options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute())
            );

            return services;
        }
    }
}
