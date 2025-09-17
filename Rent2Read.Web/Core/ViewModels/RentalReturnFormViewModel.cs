using UoN.ExpressiveAnnotations.NetCore.Attributes;

namespace Rent2Read.Web.Core.ViewModels
{
    public class RentalReturnFormViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Penalty Paid?")]
        [AssertThat("(TotalDelayInDays == 0 && PenaltyPaid == false) || PenaltyPaid == true", ErrorMessage = Errors.PenaltyShouldBePaid)]
        public bool PenaltyPaid { get; set; }
        public IList<RentalCopyViewModel> Copies { get; set; } = new List<RentalCopyViewModel>();
        //Collection of selected copies for return operations=>IList=>as we need index and not need to edit
        public List<ReturnCopyViewModel> SelectedCopies { get; set; } = new List<ReturnCopyViewModel>();

        public bool AllowExtend { get; set; }

        public int TotalDelayInDays
        {
            get
            {
                // Sum the delay days from all copies
                return Copies.Sum(c => c.DelayInDays);
            }
        }

    }
}
