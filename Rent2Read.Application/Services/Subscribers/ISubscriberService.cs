using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rent2Read.Application.Services
{
    public interface ISubscriberService
    {
        Subscriber? GetSubscriberWithRentals(int id);
        Subscriber? GetSubscriberWithSubscriptions(int id);
        (string errorMessage, int? maxAllowedCopies) CanRent(int id, int? rentalId = null);
    }
}
