using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace StudentApplication.Server.Controllers.Abstract;

public interface IUpdatableController<T>
    where T : class
{
    protected Expression<Func<T, object>>[] IgnoreProperties { get; }

    [HttpPut]
    public Task<ActionResult> Override([FromBody] T body);
}