using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rent2Read.Domain.Dtos;
    public record ReturnCopyDto(
     int Id,
     bool? IsReturned
 );

