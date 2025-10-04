using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rent2Read.Application.Services
{
    public interface IRentalService
    {
        IQueryable<Rental?> GetQueryableDetails(int id);
        Rental? GetDetails(int id);
        Rental Add(int subscriberId, ICollection<RentalCopy> copies, string createdById);
        Rental Update(int id, ICollection<RentalCopy> copies, string updatedById);

        string? ValidateExtendedCopies(Rental rental, Subscriber subscriber);
        void Return(Rental rental, IList<ReturnCopyDto> copies, bool penaltyPaid, string updatedById);
        Rental? MarkAsDeleted(int id, string deletedById);
        int GetNumberOfCopies(int id);
        IEnumerable<RentalCopy> GetAllByCopyId(int copyId);
    }
}
