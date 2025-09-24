namespace Rent2Read.Web.Core.ViewModels
{
    public class PaginationViewModel
    {
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }

        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;

        // calculate the first page number to display in the pagination bar
        public int Start
        {
            get
            {
                var start = 1; // default: start from page 1

                // if total pages exceed maximum pagination number (>10)
                if (TotalPages > (int)ReportsConfigurations.MaxPaginationNumber)
                    start = PageNumber - 9 < 1 ? 1 : PageNumber - 9;// try to center around current page (PageNumber - 9)

                return start;
            }
        }

        // calculate the last page number to display in the pagination bar
        public int End
        {
            get
            {
                var end = TotalPages; // default: show till the last page
                var maxPages = (int)ReportsConfigurations.MaxPaginationNumber;

                // if total pages exceed maxPages, adjust the end value
                if (TotalPages > maxPages)
                    end = Start + maxPages > TotalPages ? TotalPages : Start + maxPages;// if (Start + maxPages) is beyond TotalPages → stop at TotalPages

                return end;
            }
        }
    }

}
