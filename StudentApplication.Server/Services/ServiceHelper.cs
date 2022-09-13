using System.Reflection;
using Microsoft.EntityFrameworkCore.DynamicLinq;
using StudentApplication.Common.Attributes;

namespace StudentApplication.Server.Services;

public static class ServiceHelper<T>
{
    public static async Task<T?> GetByAnyIdAsync(IQueryable<T> db, object id)
    {
        T? model = default;
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

            model = await db.FirstOrDefaultAsync(keyProperty.Name + "= @0", key);
            if (model != null)
                break;
        }

        return model;
    }
    
    private static IEnumerable<PropertyInfo> KeysProperties =>
         typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(p => p.GetCustomAttribute<RestKeyAttribute>() != null);
}