using Microsoft.AspNetCore.Mvc.Rendering;

namespace Rent2Read.Web.Core.Mapping
{
    public class MappingProfile : Profile
    {
        /*Inhirtance from Profile class is used to define mapping rules between models.
       It allows you to group related mappings in one place*/
        public MappingProfile()
        {
            //Categories
            CreateMap<Category, CategoryViewModel>().ReverseMap();
            CreateMap<CategoryFormViewModel, Category>().ReverseMap();
            CreateMap<Category, SelectListItem>()
                .ForMember(dest => dest.Text, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Id));

            //Authors
            CreateMap<Author, AuthorViewModel>().ReverseMap();
            CreateMap<AuthorFormViewModel, Author>().ReverseMap();
            CreateMap<Author, SelectListItem>()
               .ForMember(dest => dest.Text, opt => opt.MapFrom(src => src.Name))
               .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Id));

            //Books
            CreateMap<BookFormViewModel, Book>()
                .ReverseMap()
                .ForMember(dest => dest.Categories, opt => opt.Ignore());

            CreateMap<Book, BookViewModel>()
                 .ForMember(dest => dest.Author, opt => opt.MapFrom(src => src.Author!.Name))
                 .ForMember(dest => dest.Categories, opt => opt.MapFrom(src => src.Categories.Select(c => c.Category!.Name).ToList()));

        }

    }
}
