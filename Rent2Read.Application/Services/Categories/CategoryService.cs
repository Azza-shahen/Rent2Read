using Rent2Read.Domain.Consts;

namespace Rent2Read.Application.Services;
internal class CategoryService(IUnitOfWork _unitOfWork) : ICategoryService
{
    public Category? GetById(int id) => _unitOfWork.Categories.GetById(id);
    public IEnumerable<Category> GetAll()
    {
        return _unitOfWork.Categories.GetAll();
    }

    public Category Add(string name, string createdById)
    {
        var category = new Category
        {
            Name = name,
            CreatedById = createdById
        };

        _unitOfWork.Categories.Add(category);
        _unitOfWork.Complete();

        return category;
    }

    public Category? Update(int id, string name, string updatedById)
    {
        var category = GetById(id);

        if (category is null)
            return null;

        category.Name = name;
        category.LastUpdatedById = updatedById;
        category.LastUpdatedOn = DateTime.Now;

        _unitOfWork.Complete();

        return category;
    }

    public Category? ToggleStatus(int id,string updatedById)
    {
        var category =GetById(id);
        if (category is null)
            return null;

        /*    if (category.IsDeleted)
            {
                category.IsDeleted = false;
            }
            else
            {
                category.IsDeleted = true;
            }
        */

        category.IsDeleted = !category.IsDeleted;
        category.LastUpdatedById = updatedById;
        category.LastUpdatedOn = DateTime.Now;
        _unitOfWork.Complete();
        return category;
    }

    public bool AllowCategory(int id, string name)
    {
        var category = _unitOfWork.Categories.Find(c => c.Name == name);
        var isAllowed = category is null || category.Id.Equals(id);
        return isAllowed;
    }

    public IEnumerable<Category> GetActiveCategories()
    {
        return _unitOfWork.Categories.FindAll(predicate: c => !c.IsDeleted, orderBy: c => c.Name, OrderBy.Ascending);
    }

}

