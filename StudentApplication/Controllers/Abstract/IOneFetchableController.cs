using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using StudentApplication.Models;

namespace StudentApplication.Controllers.Abstract;

public interface IOneFetchableController<T, TKey> : IWithModel<T, TKey>
    where T : class, IIdentifiable<TKey>
    where TKey : IEquatable<TKey>
{
    protected Expression<Func<T, object>>[] IgnoreProperties { get; }
    
    [HttpGet("{id}")]
    public Task<ActionResult<T>> GetOne([FromRoute] TKey id);
}