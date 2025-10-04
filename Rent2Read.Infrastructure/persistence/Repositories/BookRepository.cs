namespace Rent2Read.Infrastructure.persistence.Repositories;

internal class BookRepository : BaseRepository<Book>, IBookRepository
{
    public BookRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public IQueryable<Book> GetDetails()
    {
        return _dbContext.Books
                .Include(b => b.Author)
                .Include(b => b.Copies)
                .Include(b => b.Categories)
                .ThenInclude(c => c.Category);
    }
}