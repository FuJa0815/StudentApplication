using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.DynamicLinq;
using StudentApplication.Attributes;
using StudentApplication.Data;

namespace StudentApplication.Controllers.Abstract;

[ApiController]
[Route("api/v1/[controller]")]
public abstract class RestController<T> : Controller
    , ICreatableController<T>
    , IFetchableController<T>
    , IUpdatableController<T>
    , IDeletableController<T>
    , IOneFetchableController<T>
    where T : class
{
    public ApplicationDbContext Db { get; }
    public abstract DbSet<T> Model { get; }
 
    protected RestController(ApplicationDbContext db)
    {
        Db = db;
    }
    
    protected virtual Expression<Func<T, object>>[] GetOneIgnoreProperties { get; } = Array.Empty<Expression<Func<T, object>>>();
    protected virtual Expression<Func<T, object>>[] GetListIgnoreProperties { get; } = Array.Empty<Expression<Func<T, object>>>();
    Expression<Func<T, object>>[] IUpdatableController<T>.IgnoreProperties => GetOneIgnoreProperties;
    Expression<Func<T, object>>[] IOneFetchableController<T>.IgnoreProperties => GetOneIgnoreProperties;
    Expression<Func<T, object>>[] IFetchableController<T>.IgnoreProperties => GetListIgnoreProperties;

    [HttpGet("{id}")]
    public virtual async Task<ActionResult<T>> GetOne([FromRoute] string id)
    {
        var model = await GetByAnyIdAsync(id);
        if (model == null)
            return new NotFoundResult();
        
        return ToJson(model, GetOneIgnoreProperties);
    }

    [HttpDelete("{id}")]
    public virtual async Task<ActionResult> Delete([FromRoute] string id)
    {
        var model = await GetByAnyIdAsync(id);
        if (model == null)
            return new NotFoundResult();

        Model.Remove(model);
        await Db.SaveChangesAsync();
        return new NoContentResult();
    }

    [HttpGet]
    public virtual ActionResult<IEnumerable<T>> Get([FromQuery] int page = 0, [FromQuery] int pageLength = int.MaxValue,
        [FromQuery] string? sortBy = null, [FromQuery] bool ascending = true)
    {
        IEnumerable<T> data = Model;
        if (sortBy != null)
        {
            var prop = typeof(T).GetProperty(sortBy, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public);
            if (prop == null) return new BadRequestResult();
            data = data.OrderBy(prop.GetValue);
            if (!ascending)
                data = data.Reverse();
        }
        data = data.Skip(page * pageLength).Take(pageLength);

        return ToJson(data, GetListIgnoreProperties);
    }

    [HttpPost]
    public virtual async Task<ActionResult<object>> Create([FromBody] T body)
    {
        await Model.AddAsync(body);
        await Db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPut]
    public virtual async Task<ActionResult> Override([FromBody] T body)
    {
        Db.Entry(body).State = EntityState.Modified;
        await Db.SaveChangesAsync();
        return new NoContentResult();
    }

    [HttpPatch("{id}")]
    public virtual async Task<ActionResult<T>> Patch([FromRoute] string id, [FromBody] JsonPatchDocument<T> patch)
    {
        var entity = await GetByAnyIdAsync(id);
        if (entity == null)
            return NotFound();

        patch.ApplyTo(entity);
        Db.Entry(entity).State = EntityState.Modified;
        
        await Db.SaveChangesAsync();
        return ToJson(entity, GetOneIgnoreProperties);
    }
    
    private static IEnumerable<PropertyInfo> KeysProperties =>
         typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(p => p.GetCustomAttribute<RestKeyAttribute>() != null);

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


    private async Task<T?> GetByAnyIdAsync(object id)
    {
        T? model = null;
        foreach (var keyProperty in KeysProperties)
        {
            object? key;
            try
            {
                key = Convert.ChangeType(id, keyProperty.PropertyType);
            }
            catch (Exception e) when(e is InvalidCastException or FormatException)
            {
                // Key not applicable
                continue;
            }

            model = await Model.FirstOrDefaultAsync(keyProperty.Name + "= @0", key);
            if (model != null)
                break;
        }

        return model;
    }
}