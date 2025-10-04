using Microsoft.EntityFrameworkCore;
using Rent2Read.Domain.Consts;
using Rent2Read.Domain.Entities;
using Rent2Read.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Rent2Read.Application.Services;

internal class RentalService(IUnitOfWork _unitOfWork) : IRentalService
{
    public IQueryable<Rental?> GetQueryableDetails(int id)
    {

        return _unitOfWork.Rentals.GetQueryable()
                .Include(r => r.RentalCopies)
                .ThenInclude(c => c.BookCopy)
                .ThenInclude(c => c!.Book);

    }

    public Rental? GetDetails(int id)
    {
        return _unitOfWork.Rentals.GetQueryable()
                .Include(r => r.RentalCopies)
                .ThenInclude(c => c.BookCopy)
                .SingleOrDefault(r => r.Id == id);
    }

    public Rental Add(int subscriberId, ICollection<RentalCopy> copies, string createdById)
    {
        // Create a new Rental and attach the valid copies
        var rental = new Rental()
        {
            SubscriberId = subscriberId,
            RentalCopies = copies,
            CreatedById = createdById
        };
        //Add rental to subscriber
        _unitOfWork.Rentals.Add(rental);
        _unitOfWork.Complete();

        return rental;
    }

    public Rental Update(int id, ICollection<RentalCopy> copies, string updatedById)
    {
        var rental = _unitOfWork.Rentals.GetById(id);

        rental!.RentalCopies = copies;
        rental.LastUpdatedById = updatedById;
        rental.LastUpdatedOn = DateTime.Now;

        _unitOfWork.Complete();

        return rental;
    }

    public string? ValidateExtendedCopies(Rental rental, Subscriber subscriber)
    {
        string error = string.Empty;

        if (subscriber!.IsBlackListed)
            error = Errors.RentalNotAllowedForBlackListed;

        else if (subscriber!.Subscriptions.Last().EndDate < rental.StartDate.AddDays((int)RentalsConfigurations.MaxRentalDuration))
            error = Errors.RentalNotAllowedForInactive;

        else if (rental.StartDate.AddDays((int)RentalsConfigurations.RentalDuration) < DateTime.Today)
            error = Errors.ExtendNotAllowed;

        return error;
    }

    public void Return(Rental rental, IList<ReturnCopyDto> copies, bool penaltyPaid, string updatedById)
    {
        var isUpdated = false;

        foreach (var copy in copies)
        {
            if (!copy.IsReturned.HasValue) continue;//If no action chosen for this copy => skip

            var currentCopy = rental.RentalCopies.SingleOrDefault(c => c.BookCopyId == copy.Id);

            if (currentCopy is null) continue;

            if (copy.IsReturned.HasValue && copy.IsReturned.Value)// If marked as returned
            {
                if (currentCopy.ReturnDate.HasValue) continue;//If already returned before

                currentCopy.ReturnDate = DateTime.Now;
                isUpdated = true;
            }

            if (copy.IsReturned.HasValue && !copy.IsReturned.Value)// If marked as extended
            {
                if (currentCopy.ExtendedOn.HasValue) continue;// If already extended before

                currentCopy.ExtendedOn = DateTime.Now;
                currentCopy.EndDate = currentCopy.RentalDate.AddDays((int)RentalsConfigurations.MaxRentalDuration);
                isUpdated = true;
            }
        }

        if (isUpdated)
        {
            rental.LastUpdatedOn = DateTime.Now;
            rental.LastUpdatedById = updatedById;//Record which user did the update (logged in user)
            rental.PenaltyPaid = penaltyPaid;

            _unitOfWork.Complete();
        }
    }
    public Rental? MarkAsDeleted(int id, string deletedById)
    {
        var rental = _unitOfWork.Rentals.GetById(id);

        if (rental is null || rental.CreatedOn.Date != DateTime.Today)
            return null;

        rental.IsDeleted = true;
        rental.LastUpdatedOn = DateTime.Now;
        rental.LastUpdatedById = deletedById;

        _unitOfWork.Complete();

        return rental;
    }
    public int GetNumberOfCopies(int id)
    {
        return _unitOfWork.RentalCopies.Count(c => c.RentalId == id);
    }

    public IEnumerable<RentalCopy> GetAllByCopyId(int copyId)
    {
        return _unitOfWork.RentalCopies
                .FindAll(predicate: c => c.BookCopyId == copyId,
                    include: c => c.Include(x => x.Rental)!.ThenInclude(x => x!.Subscriber)!,
                    orderBy: c => c.RentalDate,
                    orderByDirection: OrderBy.Descending);

        /*  var copyHistory = _dbContext.RentalCopies
    .Include(c => c.Rental)
    .ThenInclude(r => r!.Subscriber)
    .Where(c => c.BookCopyId == id)
    .OrderByDescending(c => c.RentalDate)
    .ToList();*/
    }
}

