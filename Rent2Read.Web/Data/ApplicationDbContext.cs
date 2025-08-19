using Microsoft.AspNetCore.Identity.EntityFrameworkCore;


namespace Rent2Read.Web.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
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
            base.OnModelCreating(builder);
        }
        public DbSet<Author> Authors { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<BookCategory> BookCategories { get; set; }
        public DbSet<BookCopy> BookCopies { get; set; }
        public DbSet<Category> Categories { get; set; }

    }
}
