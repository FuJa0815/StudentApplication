using System.Linq.Expressions;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace StudentApplication.Server.Controllers.Abstract;

public interface IUpdatableController<T>
    where T : class
{
    protected Expression<Func<T, object>>[] IgnoreProperties { get; }

    [HttpPut]
    public Task<ActionResult> Override([FromBody] T body);


    [HttpPatch("{id}")]
    public Task<ActionResult<T>> Patch([FromRoute] string id, [FromBody] JsonPatchDocument<T> patch);
}