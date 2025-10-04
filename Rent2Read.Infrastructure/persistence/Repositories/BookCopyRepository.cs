namespace Rent2Read.Infrastructure.persistence.Repositories;
internal class BookCopyRepository : BaseRepository<BookCopy>, IBookCopyRepository
{
    public BookCopyRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public void SetAllAsNotAvailable(int bookId)
    {
        _dbContext.BookCopies.Where(c => c.BookId == bookId)
                    .ExecuteUpdate(p => p.SetProperty(c => c.IsAvailableForRental, false));
    }
}