using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Rent2Read.Application.Services;
using Rent2Read.Domain.Dtos;

namespace Rent2Read.Web.Controllers
{
    [Authorize(Roles = AppRoles.Reception)]
    public class RentalsController(IRentalService _rentalService
                                    ,ISubscriberService _subscriberService
                                    , IBookCopyService _bookCopyService
                                   , IDataProtectionProvider provider
                                   , IMapper _mapper) : Controller

    {
        private readonly IDataProtector _dataProtector = provider.CreateProtector("MySecureKey");

        #region Details
        public IActionResult Details(int id)
        {
            var rental = _rentalService.GetQueryableDetails(id);

            if (rental is null)
                return NotFound();

            // Project the rental entity (with related data) into a RentalViewModel 
            // using AutoMapper, then select the one with the matching id
            var viewModel = _mapper.ProjectTo<RentalViewModel>(rental).SingleOrDefault(r => r.Id == id); ;

            /*return viewModel is null ? NotFound() : View(viewModel);*/
            return View(viewModel);

        }
        #endregion
        #region Create

        [HttpGet]
        public IActionResult Create(string sKey)
        {
            var subscriberId = int.Parse(_dataProtector.Unprotect(sKey));
 
            var (errorMessage, maxAllowedCopies) = _subscriberService.CanRent(subscriberId);

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

        public IActionResult Create(RentalFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Form", model);

            //Decrypt the SubscriberKey to get the real subscriberId
            var subscriberId = int.Parse(_dataProtector.Unprotect(model.SubscriberKey));

            var (errorMessage, maxAllowedCopies) = _subscriberService.CanRent(subscriberId);

            if (!string.IsNullOrEmpty(errorMessage))
                return View("NotAllowedRental", errorMessage);

            var (rentalsError, copies) = _bookCopyService.CanBeRented(model.SelectedCopies, subscriberId);

            if (!string.IsNullOrEmpty(rentalsError))
                return View("NotAllowedRental", rentalsError);

            //Create a new Rental and attach the valid copies
            var rental = _rentalService.Add(subscriberId, copies, User.GetUserId());

            return RedirectToAction(nameof(Details), new { id = rental.Id });
        }

        #endregion
        #region Edit
        public IActionResult Edit(int id)
        {
            var rental = _rentalService.GetDetails(id);

            if (rental is null || rental.CreatedOn.Date != DateTime.Today)
                //.Date removes the time part and keeps only the day without being affected by hours, minutes, or seconds.
                return NotFound();

            var (errorMessage, maxAllowedCopies) = _subscriberService.CanRent(rental.SubscriberId, rental.Id);

            if (!string.IsNullOrEmpty(errorMessage))
                return View("NotAllowedRental", errorMessage);


            // Get the IDs of the rented book copies for this rental
            var currentCopiesIds = rental.RentalCopies.Select(c => c.BookCopyId).ToList();

            // Retrieve the BookCopy entities from the database that match these IDs
            var currentCopies = _bookCopyService.GetRentalCopies(currentCopiesIds);

            var viewModel = new RentalFormViewModel
            {
                SubscriberKey = _dataProtector.Protect(rental.SubscriberId.ToString()),
                //OR
                //SubscriberKey = _dataProtector.Protect(rental.SubscriberId.ToString()),
                MaxAllowedCopies = maxAllowedCopies,
                CurrentCopies = _mapper.Map<IEnumerable<BookCopyViewModel>>(currentCopies)
            };

            return View("Form", viewModel);
        }

        [HttpPost]

        public IActionResult Edit(RentalFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Form", model);

            var rental = _rentalService.GetDetails(model.Id ?? 0);

            if (rental is null || rental.CreatedOn.Date != DateTime.Today)
                return NotFound();

            var subscriberId = int.Parse(_dataProtector.Unprotect(model.SubscriberKey));

            var (errorMessage, maxAllowedCopies) =_subscriberService.CanRent(rental.SubscriberId, rental.Id);

            if (!string.IsNullOrEmpty(errorMessage))
                return View("NotAllowedRental", errorMessage);

            var (rentalsError, copies) = _bookCopyService.CanBeRented(model.SelectedCopies, rental.SubscriberId, rental.Id);

            if (!string.IsNullOrEmpty(rentalsError))
                return View("NotAllowedRental", rentalsError);

            _rentalService.Update(rental.Id, copies, User.GetUserId());


            return RedirectToAction(nameof(Details), new { id = rental.Id });
        }
        #endregion
        #region Return
        [HttpGet]
        public IActionResult Return(int id)
        {
            var rental = _rentalService.GetDetails(id);

            if (rental is null || rental.CreatedOn.Date == DateTime.Today)
                return NotFound();

            var subscriber = _subscriberService.GetSubscriberWithSubscriptions(rental.SubscriberId);

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

        public IActionResult Return(RentalReturnFormViewModel model)
        {
            var rental = _rentalService.GetDetails(model.Id);

            if (rental is null || rental.CreatedOn.Date == DateTime.Today)
                return NotFound();//Error 404

            var copies = _mapper.Map<IList<RentalCopyViewModel>>(rental.RentalCopies.Where(c => !c.ReturnDate.HasValue).ToList());
            if (!ModelState.IsValid)
            {
                model.Copies = copies;
                return View(model);
            }

            var subscriber = _subscriberService.GetSubscriberWithSubscriptions(rental.SubscriberId);

            if (model.SelectedCopies.Any(c => c.IsReturned.HasValue && !c.IsReturned.Value))
            //If any copy is selected and marked as "extend" (IsReturned = false)
            {
                var error = _rentalService.ValidateExtendedCopies(rental, subscriber!);

                if (!string.IsNullOrEmpty(error))
                {
                    model.Copies = copies;
                    ModelState.AddModelError("", error);
                    return View(model);
                }
            }
            var copiesDto = _mapper.Map<IList<ReturnCopyDto>>(model.SelectedCopies);

            _rentalService.Return(rental, copiesDto, model.PenaltyPaid, User.GetUserId());
            return RedirectToAction(nameof(Details), new { id = rental.Id });
        }
        #endregion
        #region GetCopyDetails

        [HttpPost]
        public IActionResult GetCopyDetails(SearchFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var copy = _bookCopyService.GetActiveCopyBySerialNumber(model.Value);

            if (copy is null)
                return NotFound(Errors.InvalidSerialNumber);

            if (!copy.IsAvailableForRental || !copy.Book!.IsAvailableForRental)
                return BadRequest(Errors.NotAvailableRental);


            //Check that the copy is not in rental
            var copyIsInRental = _bookCopyService.CopyIsInRental(copy.Id);

            if (copyIsInRental)
                return BadRequest(Errors.CopyIsInRental);

            var viewModel = _mapper.Map<BookCopyViewModel>(copy);

            return PartialView("_CopyDetails", viewModel);
        }
        #endregion
        #region MarkAsDeleted

        [HttpPost]
        public IActionResult MarkAsDeleted(int id)
        {

            var rental = _rentalService.MarkAsDeleted(id,User.GetUserId());

            if (rental is null)
                return NotFound();

            var copiesCount = _rentalService.GetNumberOfCopies(id);
            return Ok(copiesCount);
        }
        #endregion



    }
}
