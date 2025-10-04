using Rent2Read.Application.Common.Interfaces;

namespace Rent2Read.Application.Services
{
    public interface IBookService
    {
        Book? GetById(int id);
        (IQueryable<Book> books, int count) GetFiltered(FilterationDto dto);
        IQueryable<Book> GetDetails();
        Book Add(Book book, IEnumerable<int> selectedCategories, string createdById);
        Book Update(Book book, IEnumerable<int> selectedCategories, string updatedById);
        Book? ToggleStatus(int id, string updatedById);
        bool AllowTitle(int id, string title, int authorId);
       Book? GetWithCategories(int id);

        IEnumerable<BookDto> GetLastAddedBooks(int numberOfBooks);
    }
}
