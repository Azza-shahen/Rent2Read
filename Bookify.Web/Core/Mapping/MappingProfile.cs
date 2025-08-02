namespace Bookify.Web.Core.Mapping
{
    public class MappingProfile : Profile
    {
        /*Inhirtance from Profile class is used to define mapping rules between models.
       It allows you to group related mappings in one place*/
        public MappingProfile()
        {
            //Catgory
            CreateMap<Category, CategoryViewModel>().ReverseMap();
            CreateMap<CategoryFormViewModel, Category>().ReverseMap();
            //Author
            CreateMap<Author, AuthorViewModel>().ReverseMap();
            CreateMap<AuthorFormViewModel, Author>().ReverseMap();


        }

    }
}
