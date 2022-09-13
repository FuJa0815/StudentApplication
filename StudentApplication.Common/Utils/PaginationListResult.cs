namespace StudentApplication.Common.Utils;

/// <summary>
///   Response from the server whenever a GET request for multiple items is made
/// </summary>
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