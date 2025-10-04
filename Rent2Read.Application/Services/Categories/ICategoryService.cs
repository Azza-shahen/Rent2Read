namespace Rent2Read.Application.Services
{
    public interface ICategoryService
    {
        Category? GetById(int id);
        IEnumerable<Category> GetAll();
        Category Add(string name, string createdById);
        Category? Update(int id, string name, string updatedById);
        Category? ToggleStatus(int id, string updatedById);
        bool AllowCategory(int id, string name);
        IEnumerable<Category> GetActiveCategories();
    }
}
