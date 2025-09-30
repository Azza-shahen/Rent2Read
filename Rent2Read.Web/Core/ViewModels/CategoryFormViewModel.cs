namespace Rent2Read.Web.Core.ViewModels;

public class CategoryFormViewModel
{
    public int Id { get; set; }


    [/*MaxLength(100, ErrorMessage = Errors.MaxLength), */Display(Name = "Category")
        /* , RegularExpression(RegexPatterns.CharactersOnly_Eng, ErrorMessage = Errors.OnlyEnglishLetters)*/]
    [Remote("AllowItem", "Categories", AdditionalFields = "Id", ErrorMessage = Errors.Duplicated)]
    //This Attribute means your validation logic needs to call the server, like checking for uniqueness in the database.
    /* AdditionalFields=> to send more values(like Id) along with the field.
             This is useful in Edit forms to avoid false duplicate errors.
    Example: While editing a category, it checks if the name exists in other categories,
    not in the one being edited (by using Id).*/
    public string Name { get; set; } = null!;
}

