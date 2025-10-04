namespace Rent2Read.Application.Services
{
    public interface IAuthorService
    {
        IEnumerable<Author> GetAll();
        Author? GetById(int id);
        Author Add(string name, string createdById);

        public Author? Update(int id, string name, string updatedById);

        public Author? ToggleStatus(int id, string updatedById);

        public bool AllowAuthor(int id, string name);

        IEnumerable<Author> GetActiveAuthors();

    }
}
