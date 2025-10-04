using Microsoft.EntityFrameworkCore;
using Rent2Read.Domain.Consts;
using Rent2Read.Domain.Entities;
using Rent2Read.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rent2Read.Application.Services
{
    internal class SubscriberService(IUnitOfWork _unitOfWork) : ISubscriberService
    {
        public Subscriber? GetSubscriberWithRentals(int id)
        {
            //Fetch the subscriber from the database including Subscriptions and Rentals (with RentalCopies)
            return _unitOfWork.Subscribers.GetQueryable()
                    .Include(s => s.Subscriptions)
                    .Include(s => s.Rentals)
                    .ThenInclude(r => r.RentalCopies)
                    .SingleOrDefault(s => s.Id == id);
        }
        public Subscriber? GetSubscriberWithSubscriptions(int id)
        {
            return _unitOfWork.Subscribers.GetQueryable()
                    .Include(s => s.Subscriptions)
                    .SingleOrDefault(s => s.Id == id);
        }
        public (string errorMessage, int? maxAllowedCopies) CanRent(int id, int? rentalId = null)
        {
            var subscriber = GetSubscriberWithRentals(id);

            //Validate the subscriber (check if they can still rent or reached the max allowed)
            if (subscriber is null)
                return (errorMessage: Errors.NotFoundSubscriber, maxAllowedCopies: null);

            if (subscriber.IsBlackListed)
                return (errorMessage: Errors.BlackListedSubscriber, maxAllowedCopies: null);

            if (subscriber.Subscriptions.Last().EndDate < DateTime.Today.AddDays((int)RentalsConfigurations.RentalDuration))
                return (errorMessage: Errors.InactiveSubscriber, maxAllowedCopies: null);

            // Count copies that are not yet returned
            var currentRentals = subscriber.Rentals
                .Where(r => rentalId == null || r.Id != rentalId)
                .SelectMany(r => r.RentalCopies)
                .Count(c => !c.ReturnDate.HasValue);

            var availableCopiesCount = (int)RentalsConfigurations.MaxAllowedCopies - currentRentals;

            if (availableCopiesCount.Equals(0))
                return (errorMessage: Errors.MaxCopiesReached, maxAllowedCopies: null);

            return (errorMessage: string.Empty, maxAllowedCopies: availableCopiesCount);
        }

    }
}
