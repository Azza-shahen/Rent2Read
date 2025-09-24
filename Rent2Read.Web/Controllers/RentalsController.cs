using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;

namespace Rent2Read.Web.Controllers
{
    [Authorize(Roles = AppRoles.Reception)]
    public class RentalsController(ApplicationDbContext _dbContext
                                   , IDataProtectionProvider provider
                                   , IMapper _mapper) : Controller

    {
        private readonly IDataProtector _dataProtector = provider.CreateProtector("MySecureKey");

        #region Details
        public IActionResult Details(int id)
        {
            var rental = _dbContext.Rentals
                .Include(r => r.RentalCopies)
                .ThenInclude(c => c.BookCopy)
                .ThenInclude(c => c!.Book)
                .SingleOrDefault(r => r.Id == id);

            if (rental is null)
                return NotFound();

            var viewModel = _mapper.Map<RentalViewModel>(rental);

            return View(viewModel);
        }
        #endregion
        #region Create

        [HttpGet]
        public IActionResult Create(string sKey)
        {
            var subscriberId = int.Parse(_dataProtector.Unprotect(sKey));
            var subscriber = _dbContext.Subscribers
                                                 .Include(s => s.Subscriptions)
                                                 .Include(s => s.Rentals)
                                                 .ThenInclude(r => r.RentalCopies)
                                                 .SingleOrDefault(s => s.Id == subscriberId);

            if (subscriber is null)
                return NotFound();

            var (errorMessage, maxAllowedCopies) = ValidateSubscriber(subscriber);

            if (!string.IsNullOrEmpty(errorMessage))
                return View("NotAllowedRental", errorMessage);

            var viewModel = new RentalFormViewModel
            {
                SubscriberKey = sKey,
                MaxAllowedCopies = maxAllowedCopies
            };

            return View("Form", viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(RentalFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Form", model);

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

            var (rentalsError, copies) = ValidateCopies(model.SelectedCopies, subscriberId);

            if (!string.IsNullOrEmpty(rentalsError))
                return View("NotAllowedRental", rentalsError);

            //Create a new Rental and attach the valid copies
            Rental rental = new()
            {
                RentalCopies = copies,
                CreatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value
            };

            //Add rental to subscriber
            subscriber.Rentals.Add(rental);
            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Details), new { id = rental.Id });
        }



        #endregion
        #region Edit
        public IActionResult Edit(int id)
        {
            var rental = _dbContext.Rentals
                                      .Include(r => r.RentalCopies)
                                      .ThenInclude(c => c.BookCopy)
                                      .FirstOrDefault(r => r.Id == id);

            if (rental is null || rental.CreatedOn.Date != DateTime.Today)
                //.Date removes the time part and keeps only the day without being affected by hours, minutes, or seconds.
                return NotFound();

            var subscriber = _dbContext.Subscribers
                                      .Include(s => s.Subscriptions)
                                      .Include(s => s.Rentals)
                                      .ThenInclude(r => r.RentalCopies)
                                      .SingleOrDefault(s => s.Id == rental.SubscriberId);


            var (errorMessage, maxAllowedCopies) = ValidateSubscriber(subscriber!, rental.Id);

            if (!string.IsNullOrEmpty(errorMessage))
                return View("NotAllowedRental", errorMessage);

            // Get the IDs of the rented book copies for this rental
            var currentCopiesIds = rental.RentalCopies.Select(c => c.BookCopyId).ToList();

            // Retrieve the BookCopy entities from the database that match these IDs
            var currentCopies = _dbContext.BookCopies
                                                   .Where(c => currentCopiesIds.Contains(c.Id))
                                                   .Include(c => c.Book)
                                                   .ToList();


            var viewModel = new RentalFormViewModel
            {
                SubscriberKey = _dataProtector.Protect(subscriber!.Id.ToString()),
                //OR
                //SubscriberKey = _dataProtector.Protect(rental.SubscriberId.ToString()),
                MaxAllowedCopies = maxAllowedCopies,
                CurrentCopies = _mapper.Map<IEnumerable<BookCopyViewModel>>(currentCopies)
            };

            return View("Form", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(RentalFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Form", model);

            var rental = _dbContext.Rentals
                .Include(r => r.RentalCopies)
                .SingleOrDefault(r => r.Id == model.Id);

            if (rental is null || rental.CreatedOn.Date != DateTime.Today)
                return NotFound();

            var subscriberId = int.Parse(_dataProtector.Unprotect(model.SubscriberKey));

            var subscriber = _dbContext.Subscribers
                .Include(s => s.Subscriptions)
                .Include(s => s.Rentals)
                .ThenInclude(r => r.RentalCopies)
                .SingleOrDefault(s => s.Id == subscriberId);

            var (errorMessage, maxAllowedCopies) = ValidateSubscriber(subscriber!, model.Id);

            if (!string.IsNullOrEmpty(errorMessage))
                return View("NotAllowedRental", errorMessage);

            var (rentalsError, copies) = ValidateCopies(model.SelectedCopies, subscriberId, rental.Id);

            if (!string.IsNullOrEmpty(rentalsError))
                return View("NotAllowedRental", rentalsError);

            rental.RentalCopies = copies;
            rental.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            rental.LastUpdatedOn = DateTime.Now;

            _dbContext.SaveChanges();

            return RedirectToAction(nameof(Details), new { id = rental.Id });
        }
        #endregion
        #region Return
        [HttpGet]
        public IActionResult Return(int id)
        {
            var rental = _dbContext.Rentals
                                     .Include(r => r.RentalCopies)
                                     .ThenInclude(c => c.BookCopy)
                                     .ThenInclude(c => c!.Book)
                                     .FirstOrDefault(r => r.Id == id);

            if (rental is null || rental.CreatedOn.Date == DateTime.Today)
                return NotFound();

            var subscriber = _dbContext.Subscribers
                                      .Include(s => s.Subscriptions)
                                      .SingleOrDefault(s => s.Id == rental.SubscriberId);

            var viewModel = new RentalReturnFormViewModel
            {
                Id = id,
                Copies = _mapper.Map<IList<RentalCopyViewModel>>(rental.RentalCopies.Where(c => !c.ReturnDate.HasValue).ToList()),
                SelectedCopies = rental.RentalCopies.Where(c => !c.ReturnDate.HasValue).Select(c => new ReturnCopyViewModel { Id = c.BookCopyId, IsReturned = c.ExtendedOn.HasValue ? false : null }).ToList(),

                AllowExtend = !subscriber!.IsBlackListed//Subscriber must not be blacklisted
                             && subscriber.Subscriptions.Last().EndDate >= rental.StartDate.AddDays((int)RentalsConfigurations.MaxRentalDuration)
                             //latest subscription is still valid at least until 14 days after the rental start date.
                             && rental.StartDate.AddDays((int)RentalsConfigurations.RentalDuration) >= DateTime.Today
                //must not extend in the second week
            };
            return View(viewModel);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Return(RentalReturnFormViewModel model)
        {
            var rental = _dbContext.Rentals
                                     .Include(r => r.RentalCopies)
                                     .ThenInclude(c => c.BookCopy)
                                     .ThenInclude(c => c!.Book)
                                     .FirstOrDefault(r => r.Id == model.Id);

            if (rental is null || rental.CreatedOn.Date == DateTime.Today)
                return NotFound();//Error 404

            var copies = _mapper.Map<IList<RentalCopyViewModel>>(rental.RentalCopies.Where(c => !c.ReturnDate.HasValue).ToList());
            if (!ModelState.IsValid)
            {
                model.Copies = copies;
                return View(model);
            }

            var subscriber = _dbContext.Subscribers
                                        .Include(s => s.Subscriptions)
                                        .SingleOrDefault(s => s.Id == rental.SubscriberId);

            if (model.SelectedCopies.Any(c => c.IsReturned.HasValue && !c.IsReturned.Value))
            //If any copy is selected and marked as "extend" (IsReturned = false)
            {
                string error = string.Empty;

                if (subscriber!.IsBlackListed)
                    error = Errors.RentalNotAllowedForBlackListed;

                else if (subscriber!.Subscriptions.Last().EndDate < rental.StartDate.AddDays((int)RentalsConfigurations.MaxRentalDuration))//If the subscriber's last subscription expires before max rental duration
                    error = Errors.RentalNotAllowedForInactive;

                else if (rental.StartDate.AddDays((int)RentalsConfigurations.RentalDuration) < DateTime.Today)
                    error = Errors.ExtendNotAllowed;

                if (!string.IsNullOrEmpty(error))
                {
                    model.Copies = copies;
                    ModelState.AddModelError("", error);
                    return View(model);
                }
            }
            var isUpdated = false;

            foreach (var copy in model.SelectedCopies)
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
                rental.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;//Record which user did the update (logged in user)
                rental.PenaltyPaid = model.PenaltyPaid;

                _dbContext.SaveChanges();
            }

            return RedirectToAction(nameof(Details), new { id = rental.Id });
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
        #region MarkAsDeleted

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MarkAsDeleted(int id)
        {

            var rental = _dbContext.Rentals.Find(id);

            if (rental is null || rental.CreatedOn.Date != DateTime.Today)
                return NotFound();
            rental.IsDeleted = true;
            rental.LastUpdatedOn = DateTime.Now;
            rental.LastUpdatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

            _dbContext.SaveChanges();

            var copiesCount = _dbContext.RentalCopies.Count(r => r.RentalId == id);
            return Ok(copiesCount);
        }
        #endregion



        private (string errorMessage, int? maxAllowedCopies) ValidateSubscriber(Subscriber subscriber, int? rentalId = null)
        {

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

            if (availableCopiesCount.Equals(obj: 0))
                return (errorMessage: Errors.MaxCopiesReached, maxAllowedCopies: null);

            return (errorMessage: string.Empty, maxAllowedCopies: availableCopiesCount);

        }

        private (string errorMessage, ICollection<RentalCopy> copies) ValidateCopies(IEnumerable<int> selectedSerials, int subscriberId, int? rentalId = null)
        {
            //Get all selected book copies from DB including Book and Rentals
            var selectedCopies = _dbContext.BookCopies
                .Include(c => c.Book)
                .Include(c => c.Rentals)
                .Where(c => selectedSerials.Contains(c.SerialNumber))
                .ToList();

            //Get the list of BookIds that the subscriber is currently renting (not yet returned)
            var currentSubscriberRentals = _dbContext.Rentals
                .Include(r => r.RentalCopies)
                .ThenInclude(c => c.BookCopy)
                .Where(r => r.SubscriberId == subscriberId && (rentalId == null || r.Id != rentalId))
                .SelectMany(r => r.RentalCopies)
                .Where(c => !c.ReturnDate.HasValue)
                .Select(c => c.BookCopy!.BookId)
                .ToList();


            //List to hold valid copies for the new rental
            List<RentalCopy> copies = new();

            foreach (var copy in selectedCopies)
            {
                if (!copy.IsAvailableForRental || !copy.Book!.IsAvailableForRental)
                    return (errorMessage: Errors.NotAvailableRental, copies);

                //If this copy is already rented and not returned
                if (copy.Rentals.Any(c => !c.ReturnDate.HasValue && (rentalId == null || c.RentalId != rentalId)))
                    return (errorMessage: Errors.CopyIsInRental, copies);

                //If subscriber already has a copy of the same book rented
                if (currentSubscriberRentals.Any(bookId => bookId == copy.BookId))
                    return (errorMessage: $"This subscriber already has a copy for '{copy.Book.Title}' Book", copies);

                //Copy is valid => add it to the rental
                copies.Add(new RentalCopy { BookCopyId = copy.Id });
            }
            return (errorMessage: string.Empty, copies);
        }


    }
}
