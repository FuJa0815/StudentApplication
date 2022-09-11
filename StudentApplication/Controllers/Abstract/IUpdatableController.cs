using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using StudentApplication.Models;

namespace StudentApplication.Controllers.Abstract;

public interface IUpdatableController<T, TKey> : IWithModel<T, TKey>
    where T : class, IIdentifiable<TKey>
    where TKey : IComparable, IEquatable<TKey>
{
    protected Expression<Func<T, object>>[] IgnoreProperties { get; }
    
    [HttpPut]
    public Task<ActionResult> Override([FromBody] T body);


    [HttpPatch]
    public Task<ActionResult<T>> Patch([FromBody] T body);
}