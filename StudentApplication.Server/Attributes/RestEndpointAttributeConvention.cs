using System.Diagnostics.Contracts;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using StudentApplication.Common.Attributes;
using StudentApplication.Server.Controllers.Abstract;

namespace StudentApplication.Server.Attributes;

/// <summary>
///   Defines a controller naming convention based upon the <see cref="RestEndpointAttribute"/> on the model
/// </summary>
public class RestEndpointAttributeConvention : IControllerModelConvention
{
    public void Apply(ControllerModel controller)
    {
        var attribute = GetRestControllerType(controller.ControllerType)?.GenericTypeArguments.First().GetCustomAttribute<RestEndpointAttribute>();
        // No special controller handling is done when no RestEndpointAttribute was found
        if (attribute == null)
            return;
        controller.ControllerName = attribute.Url;
    }

    /// <summary>
    ///   Recursive method that looks for an inherited <see cref="RestController{T,TKey}"/> and returns it.
    /// </summary>
    [Pure]
    private static Type? GetRestControllerType(Type? t)
    {
        if (t == null)
            return null;
        var compareTo = t;
        if (t.IsGenericType)
            compareTo = t.GetGenericTypeDefinition();
        return compareTo == typeof(RestController<,>) ? t : GetRestControllerType(t.BaseType);
        
    }
}
