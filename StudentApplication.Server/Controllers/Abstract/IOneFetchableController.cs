using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;

namespace StudentApplication.Server.Controllers.Abstract;

public interface IOneFetchableController<T>
    where T : class
{
    protected Expression<Func<T, object>>[] IgnoreProperties { get; }
    
    [HttpGet("{id}")]
    public Task<ActionResult<T>> GetOne([FromRoute] string id);
}