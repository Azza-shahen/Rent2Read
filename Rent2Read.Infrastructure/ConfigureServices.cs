using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rent2Read.Infrastructure.persistence;

namespace Rent2Read.Infrastructure
{

    public static class ConfigureServices
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services,
            IConfiguration configuration)
        {
            // Register the ApplicationDbContext in the DI container
            services.AddDbContext<ApplicationDbContext>(options =>

                // Tell EF Core to use SQL Server as the database provider
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"), // Get the connection string from appsettings.json
          builder =>
                        // Specify where the EF Core migrations will be stored=>the same assembly where ApplicationDbContext is defined
                        builder.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)
                )
            );


            services.AddScoped<IApplicationDbContext, ApplicationDbContext>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }

}

