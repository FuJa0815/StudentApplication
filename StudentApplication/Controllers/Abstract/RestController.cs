using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentApplication.Data;
using StudentApplication.Models;
using StudentApplication.Services;

namespace StudentApplication.Controllers.Abstract;

[ApiController]
[Route("api/v1/[controller]")]
public abstract class RestController<T, TKey> : Controller
    , ICreatableController<T, TKey>
    , IFetchableController<T>
    , IUpdatableController<T>
    , IDeletableController<T>
    , IOneFetchableController<T>
    where T : class, IWithId<TKey>
    where TKey : IEquatable<TKey>
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public abstract DbSet<T> ModelFromDb(ApplicationDbContext db);
    
    private IRestService<T, TKey> Service { get; }

    protected RestController(IRestService<T, TKey> service)
    {
        service.GetControllerName = () => ControllerContext.ActionDescriptor.ControllerName;
        service.ModelFromDb = ModelFromDb;
        Service = service;
    }
    
    protected virtual Expression<Func<T, object>>[] GetOneIgnoreProperties { get; } = Array.Empty<Expression<Func<T, object>>>();
    protected virtual Expression<Func<T, object>>[] GetListIgnoreProperties { get; } = Array.Empty<Expression<Func<T, object>>>();
    Expression<Func<T, object>>[] IUpdatableController<T>.IgnoreProperties => GetOneIgnoreProperties;
    Expression<Func<T, object>>[] IOneFetchableController<T>.IgnoreProperties => GetOneIgnoreProperties;
    Expression<Func<T, object>>[] IFetchableController<T>.IgnoreProperties => GetListIgnoreProperties;

    [HttpGet("{id}")]
    public virtual async Task<ActionResult<T>> GetOne([FromRoute] string id)
    {
        var model = await Service.GetOne(id);
        if (model == null)
            return NotFound();
        return ToJson(model, GetOneIgnoreProperties);
    }

    [HttpDelete("{id}")]
    public virtual async Task<ActionResult> Delete([FromRoute] string id)
    {
        var done = await Service.Delete(id);
        if (!done)
            return NotFound("Id not found");
        return NoContent();
    }

    [HttpGet]
    public virtual async Task<ActionResult<PaginationListResult<T>>> Get(
        [FromQuery] int page = 0,
        [FromQuery] int pageLength = int.MaxValue,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool ascending = true,
        [FromQuery] string? query = "")
    {
        var data = await Service.Get(page, pageLength, sortBy, ascending, query);
        if (data == null)
            return BadRequest($"Property {sortBy} not found");
        
        return ToJson(data, GetListIgnoreProperties);
    }

    [HttpPost]
    public virtual async Task<ActionResult<TKey>> Create([FromBody] T body)
    {
        var id = await Service.Create(body);
        return Ok(id);
    }

    [HttpPut]
    public virtual async Task<ActionResult> Override([FromBody] T body)
    {
        await Service.Override(body);
        return NoContent();
    }

    [HttpPatch("{id}")]
    public virtual async Task<ActionResult<T>> Patch([FromRoute] string id, [FromBody] JsonPatchDocument<T> patch)
    {
        var newObj = await Service.Patch(id, patch);
        if (newObj == null)
            return NotFound("Id not found");
        return ToJson(newObj, GetOneIgnoreProperties);
    }
    
    private static JsonResult ToJson(object obj, params Expression<Func<T, object>>[] expressions)
    {
        var properties = expressions.Select(exp =>
            (exp.Body as MemberExpression ??
             throw new ArgumentException($"Expression '{exp}' does not refer to a property")).Member as PropertyInfo ??
            throw new ArgumentException($"Expression '{exp}' does not refer to a property"));
        
        return new JsonResult(obj, new JsonSerializerOptions
        {
            Converters = { new IgnorePropertiesConverter<T>(properties) }
        });
    }
}