using Microsoft.AspNetCore.Identity.EntityFrameworkCore;


namespace Bookify.Web.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
    {


        public DbSet<Author> Authors { get; set; }
        public DbSet<Category> Categories { get; set; }

    }
}
