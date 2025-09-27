using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Rent2Read.Infrastructure.persistence
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options),IApplicationDbContext
    {


        protected override void OnModelCreating(ModelBuilder builder)
        {


            // Define a sequence named "SerialNumber" in schema "shared" that starts at 1000001
            builder.HasSequence<int>("SerialNumber", schema: "shared")
                 .StartsAt(1000001);
            //SerialNumber property  automatically gets the next value from the "shared.SerialNumber" sequence
            builder.Entity<BookCopy>()
                .Property(e => e.SerialNumber)
                .HasDefaultValueSql("NEXT VALUE FOR shared.SerialNumber");

            builder.Entity<BookCategory>().HasKey(e => new { e.BookId, e.CategoryId });//Composite Key
            builder.Entity<RentalCopy>().HasKey(e => new { e.RentalId, e.BookCopyId });//Composite Key

            // Apply global query filter: exclude Rentals that are marked as deleted
            builder.Entity<Rental>().HasQueryFilter(e => !e.IsDeleted);
            builder.Entity<RentalCopy>().HasQueryFilter(e => !e.Rental!.IsDeleted);//exclude RentalCopies that belong to a deleted Rental

            base.OnModelCreating(builder);

            var cascadeFKs = builder.Model.GetEntityTypes()//It brings all the entities that are created in the model.
                         .SelectMany(t => t.GetForeignKeys())
                         .Where(fk => fk.DeleteBehavior == DeleteBehavior.Cascade && !fk.IsOwnership);

            foreach (var fk in cascadeFKs)
                fk.DeleteBehavior = DeleteBehavior.Restrict;


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
