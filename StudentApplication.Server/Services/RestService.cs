using System.Linq.Dynamic.Core;
using System.Reflection;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.DynamicLinq;
using StudentApplication.Common.Attributes;
using StudentApplication.Common.Models;
using StudentApplication.Common.Utils;
using StudentApplication.Server.Controllers.Abstract;
using StudentApplication.Server.Data;
using StudentApplication.Server.Hub;

namespace StudentApplication.Server.Services;

public interface IRestService<T, TKey>
    where T : class, IWithId<TKey>
    where TKey : IEquatable<TKey>
{
    Func<ApplicationDbContext, DbSet<T>> ModelFromDb { get; set; }
    Func<string> GetControllerName { get; set; }
    Task<T?> GetOne(string id);
    Task<bool> Delete(string id);
    Task<PaginationListResult<T>?> Get(int page, int pageLength, string? sortBy, bool ascending, string? query);
    Task<TKey> Create(T body);
    Task Override(T body);
    Task<T?> Patch(string id, JsonPatchDocument<T> patch);
}

public class RestService<T, TKey> : IRestService<T, TKey>
    where T : class, IWithId<TKey>
    where TKey : IEquatable<TKey>
{
    private IHubContext<NotificationHub> Hub { get; }
    private ApplicationDbContext Db { get; }
    public Func<ApplicationDbContext, DbSet<T>> ModelFromDb { get; set; } = null!;
    public Func<string> GetControllerName { get; set; }
    private string ControllerName => GetControllerName();
    private DbSet<T> Model => ModelFromDb(Db);
    private ILogger Logger { get; }

    public RestService(IHubContext<NotificationHub> hub, ApplicationDbContext db, ILogger<RestService<T, TKey>> logger)
    {
        Hub = hub;
        Db = db;
        Logger = logger;
    }

    public async Task<T?> GetOne(string id)
    {
        var model = await GetByAnyIdAsync(id);
        if (model == null)
        {
            Logger.LogWarning($"{ControllerName} with id {id} not found");
            return null;
        }
        
        Logger.LogDebug($"{ControllerName} with id {id} fetched");
        
        return model;
    }

    public async Task<bool> Delete(string id)
    {
        var model = await GetByAnyIdAsync(id);
        if (model == null)
        {
            Logger.LogWarning($"{ControllerName} with id {id} not found");
            return false;
        }
        
        Logger.LogInformation($"{ControllerName} with id {id} deleted");

        Model.Remove(model);
        await Hub.Clients.Groups(ControllerName, $"{ControllerName}.{model.Id}")
            .SendCoreAsync($"{ControllerName}_delete", new object[] { model.Id });
        await Db.SaveChangesAsync();
        return true;
    }

    public async Task<PaginationListResult<T>?> Get(int page, int pageLength, string? sortBy, bool ascending, string? query)
    {
        var data = Search(query);
        var count = await data.CountAsync();
        if (sortBy != null && SortableProperties.All(p => !string.Equals(p.Name, sortBy, StringComparison.CurrentCultureIgnoreCase)))
            return null;
        data = data.OrderBy(sortBy ?? "Id");
        if (!ascending)
            data = data.Reverse();

        data = data.Skip(page * pageLength).Take(pageLength);

        return new PaginationListResult<T>(await data.ToListAsync(), count);
    }

    public async Task<TKey> Create(T body)
    {
        var model = await Model.AddAsync(body);
        await Db.SaveChangesAsync();
        Logger.LogInformation($"{ControllerName} with id {model.Entity.Id} created");
        await Hub.Clients.Groups(ControllerName)
            .SendCoreAsync($"{ControllerName}_create", new object?[] { model.Entity });
        return model.Entity.Id;
    }

    public async Task Override(T body)
    {
        Logger.LogInformation($"{ControllerName} with id {body.Id} overridden");
        Db.Entry(body).State = EntityState.Modified;
        await Hub.Clients.Groups(ControllerName, $"{ControllerName}.{body.Id}")
            .SendCoreAsync($"{ControllerName}_update", new object?[] { body });
        await Db.SaveChangesAsync();
    }

    public async Task<T?> Patch(string id, JsonPatchDocument<T> patch)
    {
        var entity = await GetByAnyIdAsync(id);
        if (entity == null)
        {
            Logger.LogWarning($"{ControllerName} with id {id} not found");
            return null;
        }

        Logger.LogInformation($"{ControllerName} with id {id} overridden");
        
        patch.ApplyTo(entity);
        Db.Entry(entity).State = EntityState.Modified;
        await Db.SaveChangesAsync();
        await Hub.Clients.Groups(ControllerName, $"{ControllerName}.{id}")
            .SendCoreAsync($"{ControllerName}_update", new object[] { entity });
        return entity;
    }

    private static IEnumerable<PropertyInfo> KeysProperties =>
         typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(p => p.GetCustomAttribute<RestKeyAttribute>() != null);
    
    private static IEnumerable<PropertyInfo> SearchableProperties =>
         typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(p => p.GetCustomAttribute<RestSearchableAttribute>() != null);
    
    private static IEnumerable<PropertyInfo> SortableProperties =>
         typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(p => p.GetCustomAttribute<RestSortableAttribute>() != null);
    
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
    
    private IQueryable<T> Search(string? term)
    {
        if (string.IsNullOrWhiteSpace(term))
            return Model;
        
        var str = string.Join(" || ", SearchableProperties.Select(p => $"{p.Name}.ToLower().Contains(@0)"));
        return Model.Where(str, term.ToLower());
    }
}