using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Rent2Read.Web.Controllers
{
    [Authorize(Roles =AppRoles.Reception)]
    public class RentalsController(ApplicationDbContext _dbContext
                                   , IDataProtectionProvider provider
                                   , IMapper _mapper) : Controller

    {
        private readonly IDataProtector _dataProtector = provider.CreateProtector("MySecureKey");

        #region Create

        [HttpGet]
        public IActionResult Create(string sKey)
        {
            var subscriberId=int.Parse(_dataProtector.Unprotect(sKey));
            var subscriber = _dbContext.Subscribers
                                                 .Include(s => s.Subscriptions)
                                                 .Include(s => s.Rentals)
                                                 .ThenInclude(r => r.RentalCopies)
                                                 .SingleOrDefault(s => s.Id == subscriberId);

            if (subscriber is null) 
                return NotFound();

             var (errorMessage, maxAllowedCopies) = ValidateSubscriber(subscriber);

            if(!string.IsNullOrEmpty(errorMessage))
                return View("NotAllowedRental",errorMessage);

            var viewModel = new RentalFormViewModel
            {
                SubscriberKey = sKey,
                MaxAllowedCopies = maxAllowedCopies
            };

            return View(viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(RentalFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            //Decrypt the SubscriberKey to get the real subscriberId
            var subscriberId = int.Parse(_dataProtector.Unprotect(model.SubscriberKey));

            //Fetch the subscriber from the database including Subscriptions and Rentals (with RentalCopies)
            var subscriber = _dbContext.Subscribers
                .Include(s => s.Subscriptions)
                .Include(s => s.Rentals)
                .ThenInclude(r => r.RentalCopies)
                .SingleOrDefault(s => s.Id == subscriberId);

            if (subscriber is null)
                return NotFound();//Error 404

            //Validate the subscriber (check if they can still rent or reached the max allowed)
            var (errorMessage, maxAllowedCopies) = ValidateSubscriber(subscriber);

            if (!string.IsNullOrEmpty(errorMessage))
                return View("NotAllowedRental", errorMessage);

            //Get all selected book copies from DB including Book and Rentals
            var selectedCopies = _dbContext.BookCopies
                .Include(c => c.Book)
                .Include(c => c.Rentals)
                .Where(c => model.SelectedCopies.Contains(c.SerialNumber))
                .ToList();

            //Get the list of BookIds that the subscriber is currently renting (not yet returned)
            var currentSubscriberRentals = _dbContext.Rentals
                .Include(r => r.RentalCopies)
                .ThenInclude(c => c.BookCopy)
                .Where(r => r.SubscriberId == subscriberId)
                .SelectMany(r => r.RentalCopies)
                .Where(c => !c.ReturnDate.HasValue)
                .Select(c => c.BookCopy!.BookId)
                .ToList();

            //List to hold valid copies for the new rental
            List<RentalCopy> copies = new();

            foreach (var copy in selectedCopies)
            {
                if (!copy.IsAvailableForRental || !copy.Book!.IsAvailableForRental)
                    return View("NotAllowedRental", Errors.NotAvailableRental);

                //If this copy is already rented and not returned
                if (copy.Rentals.Any(c => !c.ReturnDate.HasValue))
                    return View("NotAllowedRental", Errors.CopyIsInRental);

                //If subscriber already has a copy of the same book rented
                if (currentSubscriberRentals.Any(bookId => bookId == copy.BookId))
                    return View("NotAllowedRental", $"This subscriber already has a copy for '{copy.Book.Title}' Book");

                //Copy is valid => add it to the rental
                copies.Add(new RentalCopy { BookCopyId = copy.Id });
            }
            //Create a new Rental and attach the valid copies
            Rental rental = new()
            {
                RentalCopies = copies,
                CreatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value
            };

            //Add rental to subscriber
            subscriber.Rentals.Add(rental);
            _dbContext.SaveChanges();

            return Ok();  //Return success response
        }

  

        #endregion
        #region GetCopyDetails

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GetCopyDetails(SearchFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var copy = _dbContext.BookCopies
                .Include(c => c.Book)
                .SingleOrDefault(c => c.SerialNumber.ToString() == model.Value && !c.IsDeleted && !c.Book!.IsDeleted);

            if (copy is null)
                return NotFound(Errors.InvalidSerialNumber);

            if (!copy.IsAvailableForRental || !copy.Book!.IsAvailableForRental)
                return BadRequest(Errors.NotAvailableRental);


            //Check that copy is not in rental
            var copyIsInRental = _dbContext.RentalCopies.Any(c => c.BookCopyId == copy.Id && !c.ReturnDate.HasValue);

            if (copyIsInRental)
                return BadRequest(Errors.CopyIsInRental);


            var viewModel = _mapper.Map<BookCopyViewModel>(copy);

            return PartialView("_CopyDetails", viewModel);
        } 
        #endregion

        private (string errorMessage ,int? maxAllowedCopies) ValidateSubscriber(Subscriber subscriber)
        {

            if (subscriber.IsBlackListed)
                return (errorMessage:Errors.BlackListedSubscriber, maxAllowedCopies: null);

            if (subscriber.Subscriptions.Last().EndDate < DateTime.Today.AddDays((int)RentalsConfigurations.RentalDuration))
                return (errorMessage: Errors.InactiveSubscriber, maxAllowedCopies: null);


            // Count copies that are not yet returned
            var currentRentals = subscriber.Rentals
                                            .SelectMany(r => r.RentalCopies)
                                            .Count(c => !c.ReturnDate.HasValue);
            var availableCopiesCount = (int)RentalsConfigurations.MaxAllowedCopies - currentRentals;

            if (availableCopiesCount.Equals(obj: 0))
                return (errorMessage: Errors.MaxCopiesReached, maxAllowedCopies:null);

          return (errorMessage: string.Empty, maxAllowedCopies: availableCopiesCount);

        }

    }
}
