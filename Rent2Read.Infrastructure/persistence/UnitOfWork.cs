using Rent2Read.Infrastructure.persistence.Repositories;

namespace Rent2Read.Infrastructure.persistence
{
    internal class UnitOfWork(ApplicationDbContext _dbContext) : IUnitOfWork
    {
        public IBaseRepository<Author> Authors => new BaseRepository<Author>(_dbContext);
        public IBookRepository Books => new BookRepository(_dbContext);
        public IBookCopyRepository BookCopies => new BookCopyRepository(_dbContext);
        public IBaseRepository<Category> Categories => new BaseRepository<Category>(_dbContext);
        public IBaseRepository<Area> Areas => new BaseRepository<Area>(_dbContext);
        public IBaseRepository<BookCategory> BookCategories => new BaseRepository<BookCategory>(_dbContext);
        public IBaseRepository<Governorate> Governorates => new BaseRepository<Governorate>(_dbContext);
        public IBaseRepository<Rental> Rentals => new BaseRepository<Rental>(_dbContext);
        public IBaseRepository<RentalCopy> RentalCopies => new BaseRepository<RentalCopy>(_dbContext);
        public IBaseRepository<Subscriber> Subscribers => new BaseRepository<Subscriber>(_dbContext);
        public IBaseRepository<Subscription> Subscriptions => new BaseRepository<Subscription>(_dbContext);


        public int Complete()
        {
            return _dbContext.SaveChanges();
        }
    }
}
