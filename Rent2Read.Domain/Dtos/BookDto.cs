using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rent2Read.Domain.Dtos;
    public record BookDto(
    
        int Id,
        string Title,
        string? ImageThumbnailUrl,
        string Name
   );

