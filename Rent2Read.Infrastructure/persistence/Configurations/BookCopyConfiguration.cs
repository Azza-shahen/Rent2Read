namespace Rent2Read.Infrastructure.persistence.Configurations
{
    internal class BookCopyConfiguration : IEntityTypeConfiguration<BookCopy>
    {
        public void Configure(EntityTypeBuilder<BookCopy> builder)
        {
            //SerialNumber property  automatically gets the next value from the "shared.SerialNumber" sequence
            builder.Property(e => e.SerialNumber)
                .HasDefaultValueSql("NEXT VALUE FOR shared.SerialNumber");

            builder.Property(e => e.CreatedOn).HasDefaultValueSql("GETDATE()");
        }
    }
}
