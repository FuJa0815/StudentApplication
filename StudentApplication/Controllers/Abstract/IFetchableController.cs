using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;

namespace StudentApplication.Controllers.Abstract;

public interface IFetchableController<T> : IWithModel<T>
    where T : class
{
    protected Expression<Func<T, object>>[] IgnoreProperties { get; }
    
    [HttpGet]
    public ActionResult<IEnumerable<T>> Get(
        [FromQuery] int page = 0,
        [FromQuery] int pageLength = int.MaxValue,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool ascending = true);
}
