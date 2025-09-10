namespace Rent2Read.Web.Core.ViewModels
{
    public class RentalFormViewModel
    {
        public string SubscriberKey { get; set; } = null!;

        public IList<int> SelectedCopies { get; set; } = new List<int>();
        // IList is used instead of IEnumerable because I need index 
        //and not List<> because I don't need to edit data(Read-Only)

        public int? MaxAllowedCopies { get; set; } //Maximum copies user can add (e.g. if subscriber already rented 2, then max = 1)
    }
}
