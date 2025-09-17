namespace Rent2Read.Web.Core.ViewModels
{
    public class ReturnCopyViewModel
    {
        public int Id { get; set; }  //The ID of the BookCopy (used to identify the copy in DB)

        public bool? IsReturned { get; set; }
        // Can be: null, true, false
        // true  => the copy is returned
        // false => the rental for this copy is extended
        // null  => neither returned nor extended
    }
}
