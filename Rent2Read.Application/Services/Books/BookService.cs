using Microsoft.EntityFrameworkCore;
using Rent2Read.Domain.Entities;

namespace Rent2Read.Application.Services
{
    internal class BookService(IUnitOfWork _unitOfWork) : IBookService
    {
        public Book? GetById(int id) => _unitOfWork.Books.GetById(id);

        public (IQueryable<Book> books, int count) GetFiltered(FilterationDto dto)
        {
            IQueryable<Book> books = _unitOfWork.Books.GetDetails();

            if (!string.IsNullOrEmpty(dto.SearchValue))
                books = books.Where(b => b.Title.Contains(dto.SearchValue!) || b.Author!.Name.Contains(dto.SearchValue!));

            // Apply dynamic sorting based on column name and sort direction=>You should use a library like System.Linq.Dynamic.Core
            // not OrderBy from LINQ, because it doesn't understand string as an expression.(b=>b.Title)
            books = books
                .OrderBy($"{dto.SortColumn} {dto.SortColumnDirection}")
                .Skip(dto.Skip)
                .Take(dto.PageSize);// Returns the required part of the data Only.

            var recordsTotal = _unitOfWork.Books.Count();// Total number of books

            return (books, recordsTotal);
        }

        public IQueryable<Book> GetDetails()
        {
            return _unitOfWork.Books.GetDetails();
        }

        public Book Add(Book book, IEnumerable<int> selectedCategories, string createdById)
        {
            book.CreatedById = createdById;

            /* SelectedCategories contains the IDs of all chosen categories.
                 I loop over them to add each one to the book's Categories collection.
                 This ensures the many-to - many relationship is saved properly in the join table.*/
            foreach (var category in selectedCategories)
                book.Categories.Add(new BookCategory { CategoryId = category });

            _unitOfWork.Books.Add(book);
            _unitOfWork.Complete();

            return book;
        }

        public Book Update(Book book, IEnumerable<int> selectedCategories, string updatedById)
        {
            book.LastUpdatedById = updatedById;
            book.LastUpdatedOn = DateTime.Now;
            //book.ImageThumbnailUrl = GetThumbnailUrl(book.ImageUrl!);
            //book.ImagePublicId = imagePublicId;

            foreach (var category in selectedCategories)
                book.Categories.Add(new BookCategory { CategoryId = category });

         
            _unitOfWork.Complete();


            //.NET 6
            // This works in-memory: it requires the copies to be already loaded from the database.
            // Changes will only be saved to the database
            // after calling _context.SaveChanges().

            /*    if (!book.IsAvailableForRental)
                {
                    foreach (var copy in book.Copies)
                        copy.IsAvailableForRental = false;

                }*/
            /*    if (!book.IsAvailableForRental)
                {
                    book.Copies.ToList().ForEach(copy => copy.IsAvailableForRental = false);
                }*/

            //.NET 7
            // This does not require loading the copies into memory. It performs a single SQL UPDATE statement
            //which is faster and more efficient for large datasets.

            /*  if (!book.IsAvailableForRental)
                  _unitOfWork.BookCopies.Where(c => c.BookId == book.Id)
                     .ExecuteUpdate(p => p.SetProperty(c => c.IsAvailableForRental, false));*/

            //.NET 7
            if (!book.IsAvailableForRental)
                _unitOfWork.BookCopies.SetAllAsNotAvailable(book.Id);

            return book;
        }
        public Book? GetWithCategories(int id)
        {
            return _unitOfWork.Books.Find(predicate: b => b.Id == id,
                                       include: b => b.Include(x => x.Categories));
        }

        public bool AllowTitle(int id, string title, int authorId)
        {
            var book = _unitOfWork.Books.Find(b => b.Title == title && b.AuthorId == authorId);
            var IsAllowed = book is null || book.Id.Equals(id);

            return IsAllowed;
        }
        public Book? ToggleStatus(int id, string updatedById)
        {
            var book = GetById(id);

            if (book is null)
                return null;

            book.IsDeleted = !book.IsDeleted;
            book.LastUpdatedById = updatedById;
            book.LastUpdatedOn = DateTime.Now;

            _unitOfWork.Complete();

            return book;
        }

        public IEnumerable<BookDto> GetLastAddedBooks(int numberOfBooks)
        {
            // Get a base query of all books from the repository
            var query = _unitOfWork.Books.GetQueryable();

            // Include Author navigation property so we can access Author.Name
            return query.Include(b => b.Author)

                        // Filter: only non-deleted books
                        .Where(b => !b.IsDeleted)

                        // Order: newest books first (by Id descending)
                        .OrderByDescending(b => b.Id)

                        // Limit: take only the requested number of books
                        .Take(numberOfBooks)

                        // Projection: map each book to a BookDto object
                        .Select(b => new BookDto(
                            b.Id,
                            b.Title,
                            b.ImageThumbnailUrl,
                            b.Author!.Name
                        ))

                        // Materialize query into a list (execute in database)
                        .ToList();
        }

    }
}
