using Microsoft.Extensions.DependencyInjection;
using Rent2Read.Application.Services;

namespace Rent2Read.Application
{

    public static class ConfigureServices
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {

            services.AddScoped<IAuthorService, AuthorService>();
            services.AddScoped<IBookCopyService, BookCopyService>();
            services.AddScoped<IBookService, BookService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IRentalService, RentalService>();
            services.AddScoped<ISubscriberService, SubscriberService>();

            return services;
        }
    }

}

