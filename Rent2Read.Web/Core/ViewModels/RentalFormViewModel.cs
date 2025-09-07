namespace Rent2Read.Web.Core.ViewModels
{
    public class RentalFormViewModel
    {
        public string SubscriberKey { get; set; } = null!;

        public IList<int> SelectedCopies { get; set; } = new List<int>();
        // IList is used instead of IEnumerable because I need index 
        //and not List<> because I don't need to edit data(Read-Only)
    }
}
