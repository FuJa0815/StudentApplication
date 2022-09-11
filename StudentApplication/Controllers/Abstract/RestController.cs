using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentApplication.Data;
using StudentApplication.Models;

namespace StudentApplication.Controllers.Abstract;

public abstract class RestController<T, TKey> : KeyRestController<T, TKey>
    , ICreatableController<T, TKey>
    , IFetchableController<T, TKey>
    , IUpdatableController<T, TKey>
    where T : class, IIdentifiable<TKey>
    where TKey : IComparable, IEquatable<TKey>
{

    protected RestController(ApplicationDbContext db) : base(db)
    {
    }
    
    protected virtual Expression<Func<T, object>>[] GetListIgnoreProperties { get; } = Array.Empty<Expression<Func<T, object>>>();
    Expression<Func<T, object>>[] IUpdatableController<T, TKey>.IgnoreProperties => GetOneIgnoreProperties;
    Expression<Func<T, object>>[] IFetchableController<T, TKey>.IgnoreProperties => GetListIgnoreProperties;

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
    public virtual async Task<ActionResult<TKey>> Create([FromBody] T body)
    {
        body.Key = default!;
        var id = (await Model.AddAsync(body)).Entity.Key;
        await Db.SaveChangesAsync();
        return id;
    }

    [HttpPut]
    public virtual async Task<ActionResult> Override([FromBody] T body)
    {
        Db.Entry(body).State = EntityState.Modified;
        await Db.SaveChangesAsync();
        return new NoContentResult();
    }

    [HttpPatch]
    public virtual async Task<ActionResult<T>> Patch([FromBody] T body)
    {
        var res = Db.Update(body);
        if (res.State == EntityState.Added)
        {
            Db.ChangeTracker.Clear();
            return new NotFoundResult();
        }

        await Db.SaveChangesAsync();
        return ToJson(res.Entity, GetOneIgnoreProperties);
    }
}