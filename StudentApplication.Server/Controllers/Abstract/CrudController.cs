using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using StudentApplication.Common.Utils;
using StudentApplication.Server.Data;
using StudentApplication.Server.Services;

namespace StudentApplication.Server.Controllers.Abstract;

/// <summary>
///   Implements GET (by key and multiple), POST, PUT, DELETE methods for a given model.
/// </summary>
/// <typeparam name="T">The model</typeparam>
/// <typeparam name="TKey">The primary key type. Alternate REST-keys can be defined via the <see cref="StudentApplication.Common.Attributes.RestKeyAttribute"/> attribute</typeparam>
[ApiController]
[Route("api/v1/[controller]")]
public abstract class CrudController<T, TKey> : Controller
    , ICreatableController<T, TKey>
    , IFetchableController<T>
    , IUpdatableController<T>
    , IDeletableController<T>
    , IOneFetchableController<T>
    where T : class, IModel<TKey>
    where TKey : IEquatable<TKey>
{
    /// <summary>
    ///   Defines the DbSet used for this endpoint
    /// </summary>
    [ApiExplorerSettings(IgnoreApi = true)]
    public abstract DbSet<T> ModelFromDb(ApplicationDbContext db);

    /// <summary>
    ///   Preprocessing, like Including, can be made here
    /// </summary>
    [ApiExplorerSettings(IgnoreApi = true)]
    public virtual IQueryable<T> QueryableFromModel(DbSet<T> model) => model;

    /// <summary>
    ///   The main service for this class
    /// </summary>
    private ICrudService<T, TKey> Service { get; }

    public CrudController(ICrudService<T, TKey> service)
    {
        // Some functions need to be injected
        service.GetControllerName = () => ControllerContext.ActionDescriptor.ControllerName;
        service.ModelFromDb = ModelFromDb;
        service.QueryableFromModel = QueryableFromModel;
        
        Service = service;
    }
    
    /// <summary>
    ///   Expressions that define properties that will be skipped in serialization of a single model
    /// </summary>
    protected virtual Expression<Func<T, object>>[] GetOneIgnoreProperties { get; } = Array.Empty<Expression<Func<T, object>>>();
    
    /// <summary>
    ///   Expressions that define properties that will be skipped in serialization of a list of model
    /// </summary>
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
            return BadRequest($"Property {sortBy} not found or not sortable");
        
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
    
    /// <summary>
    ///   Utilizes the <see cref="IgnorePropertiesContractResolver"/> to skip unwanted properties without the need for
    ///   DTO for everything.
    /// </summary>
    private static JsonResult ToJson(object obj, params Expression<Func<T, object>>[] expressions)
    {
        var properties = expressions.Select(exp =>
            (exp.Body as MemberExpression ??
             throw new ArgumentException($"Expression '{exp}' does not refer to a property")).Member as PropertyInfo ??
            throw new ArgumentException($"Expression '{exp}' does not refer to a property"));
        
        return new JsonResult(obj, new JsonSerializerSettings
        {
            ContractResolver = new IgnorePropertiesContractResolver(properties)
        });
    }
}