using System.Reflection;
using Microsoft.EntityFrameworkCore.DynamicLinq;
using StudentApplication.Common.Attributes;

namespace StudentApplication.Server.Services;

public static class ServiceHelper<T>
{
    /// <summary>
    ///   Fetches a single item via any <see cref="RestKeyAttribute"/>
    /// </summary>
    public static async Task<T?> GetByAnyIdAsync(IQueryable<T> db, object id)
    {
        T? model = default;
        
        // Iterate all keyed fields
        foreach (var keyProperty in KeysProperties)
        {
            object? key;
            try
            {
                // Try to cast the given key to the type of the current field
                key = Convert.ChangeType(id, keyProperty.PropertyType);
            }
            catch (Exception e) when(e is InvalidCastException or FormatException)
            {
                // Key not applicable
                continue;
            }

            model = await db.FirstOrDefaultAsync(keyProperty.Name + "= @0", key);
            if (model != null)
                break;
        }

        return model;
    }
    
    /// <summary>
    ///   All properties of T with the <see cref="RestKeyAttribute"/>
    /// </summary>
    private static IEnumerable<PropertyInfo> KeysProperties =>
         typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(p => p.GetCustomAttribute<RestKeyAttribute>() != null);
}