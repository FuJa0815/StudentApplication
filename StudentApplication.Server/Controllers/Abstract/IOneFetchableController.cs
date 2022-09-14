using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace StudentApplication.Server.Controllers.Abstract;

public interface IOneFetchableController<T>
    where T : class
{
    protected Expression<Func<T, object>>[] IgnoreProperties { get; }
    
    [HttpGet("{id}")]
    public Task<ActionResult<T>> GetOne([FromRoute] string id);
}