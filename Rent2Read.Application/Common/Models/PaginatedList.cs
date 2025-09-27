namespace Rent2Read.Application.Common.Models;

// This class is used to implement pagination (splitting data into pages).
public class PaginatedList<T> : List<T>
{
    public int PageNumber { get; private set; }// The current page number (which page the user is on).
    public int TotalPages { get; private set; }

    public PaginatedList(List<T> items, int count, int pageNumber, int pageSize)
    {
        PageNumber = pageNumber;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);//how many items should be displayed per page.Ceiling to round up if there is a remainder.

        AddRange(items);//Add all the items for the current page into the inherited List<T>.
    }

    public bool HasPreviousPage => PageNumber > 1;//get property=>True if the current page number is greater than 1.
    public bool HasNextPage => PageNumber < TotalPages;

    public static PaginatedList<T> Create(IQueryable<T> source, int pageNumber, int pageSize)
    {
        var count = source.Count();//Count the total number of items in the source.

        /* Get the items for the current page:
          Skip → skips items that belong to previous pages.
         Take → fetch only the number of items for the current page(pageSize).*/

        var items = source
             .Skip((pageNumber - 1) * pageSize)
             .Take(pageSize)
             .ToList();

        return new PaginatedList<T>(items, count, pageNumber, pageSize);
    }
}
