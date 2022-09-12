namespace StudentApplication.Common.Utils;

public class PaginationListResult<T>
{
    public List<T> Items { get; }
    
    public int TotalItems { get; }

    public PaginationListResult(List<T> items, int totalItems)
    {
        Items = items;
        TotalItems = totalItems;
    }
}