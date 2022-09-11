using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;

namespace StudentApplication.Server.Controllers.Abstract;

public interface IFetchableController<T>
    where T : class
{
    protected Expression<Func<T, object>>[] IgnoreProperties { get; }
    
    [HttpGet]
    public Task<ActionResult<PaginationListResult<T>>> Get(
        [FromQuery] int page = 0,
        [FromQuery] int pageLength = int.MaxValue,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool ascending = true,
        [FromQuery] string? query = "");
}

public class PaginationListResult<T>
{
    public IEnumerable<T> Items { get; }
    
    public int TotalItems { get; }

    public PaginationListResult(IEnumerable<T> items, int totalItems)
    {
        Items = items;
        TotalItems = totalItems;
    }
}
