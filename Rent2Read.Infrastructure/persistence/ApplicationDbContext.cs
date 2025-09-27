using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.Reflection;

namespace Rent2Read.Infrastructure.persistence
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options), IApplicationDbContext
    {


        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Define a sequence named "SerialNumber" in schema "shared" that starts at 1000001
            builder.HasSequence<int>("SerialNumber", schema: "shared")
                 .StartsAt(1000001);


            //This automatically applies all IEntityTypeConfiguration<T> implementations found in the current assembly(where DbContext exists). 
            // Instead of registering each entity configuration manually, EF Core will scan the assembly and apply configurations for all entities at once.
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            var cascadeFKs = builder.Model.GetEntityTypes()//It brings all the entities that are created in the model.
                         .SelectMany(t => t.GetForeignKeys())
                         .Where(fk => fk.DeleteBehavior == DeleteBehavior.Cascade && !fk.IsOwnership);

            foreach (var fk in cascadeFKs)
                fk.DeleteBehavior = DeleteBehavior.Restrict;

            base.OnModelCreating(builder);
        }
        public DbSet<Area> Areas { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<BookCategory> BookCategories { get; set; }
        public DbSet<BookCopy> BookCopies { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Governorate> Governorates { get; set; }
        public DbSet<Rental> Rentals { get; set; }
        public DbSet<RentalCopy> RentalCopies { get; set; }
        public DbSet<Subscriber> Subscribers { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }

    }
}
