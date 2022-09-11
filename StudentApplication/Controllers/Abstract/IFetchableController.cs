using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using StudentApplication.Models;

namespace StudentApplication.Controllers.Abstract;

public interface IFetchableController<T, TKey> : IWithModel<T, TKey>
    where T : class, IIdentifiable<TKey>
    where TKey : IComparable, IEquatable<TKey>
{
    protected Expression<Func<T, object>>[] IgnoreProperties { get; }
    
    [HttpGet]
    public ActionResult<IEnumerable<T>> Get(
        [FromQuery] int page = 0,
        [FromQuery] int pageLength = int.MaxValue,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool ascending = true);
}
