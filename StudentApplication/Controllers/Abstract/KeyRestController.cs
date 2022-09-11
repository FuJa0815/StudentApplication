using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentApplication.Data;
using StudentApplication.Models;

namespace StudentApplication.Controllers.Abstract;

[ApiController]
[Route("api/v1/[controller]")]
public abstract class KeyRestController<T, TKey> : Controller
    , IDeletableController<T, TKey>
    , IOneFetchableController<T, TKey>
    where T : class, IIdentifiable<TKey>
    where TKey : IEquatable<TKey>, IComparable
{
    protected KeyRestController(ApplicationDbContext db)
    {
        Db = db;
    }

    [HttpGet("{id}")]
    public virtual async Task<ActionResult<T>> GetOne([FromRoute] TKey id)
    {
        await Model.ForEachAsync(m => Console.WriteLine(m.Key.GetType().ToString()));
        Console.WriteLine(id.GetType().ToString());
        var model = (await Model.ToListAsync()).FirstOrDefault(m => id.ToString() == m.Key.ToString());
        if (model == null)
            return new NotFoundResult();
        
        return ToJson(model, GetOneIgnoreProperties);
    }

    [HttpDelete("{id}")]
    public virtual async Task<ActionResult> Delete([FromRoute] TKey id)
    {
        var obj = await Model.FirstOrDefaultAsync(m => m.Key.Equals(id));
        if (obj == null)
            return new NotFoundResult();

        Model.Remove(obj);
        await Db.SaveChangesAsync();
        return new NoContentResult();
    }

    Expression<Func<T, object>>[] IOneFetchableController<T, TKey>.IgnoreProperties => GetOneIgnoreProperties;
    protected virtual Expression<Func<T, object>>[] GetOneIgnoreProperties { get; } = Array.Empty<Expression<Func<T, object>>>();

    public abstract DbSet<T> Model { get; }
    public ApplicationDbContext Db { get; }

    protected JsonResult ToJson(object obj, params Expression<Func<T, object>>[] expressions)
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