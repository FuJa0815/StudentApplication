using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using StudentApplication.Common.Attributes;
using StudentApplication.Server.Controllers.Abstract;

namespace StudentApplication.Server.Attributes;

/// <summary>
///   Defines a controller naming convention based upon the <see cref="CrudEndpointAttribute"/> on the model
/// </summary>
public class CrudEndpointAttributeConvention : IControllerModelConvention
{
    public void Apply(ControllerModel controller)
    {
        var attribute = GetCrudControllerType(controller.ControllerType)?.GenericTypeArguments.First().GetCustomAttribute<CrudEndpointAttribute>();
        // No special controller handling is done when no CrudEndpointAttribute was found
        if (attribute == null)
            return;
        controller.ControllerName = attribute.Url;
    }

    /// <summary>
    ///   Recursive method that looks for an inherited <see cref="CrudController{T,TKey}"/> and returns it.
    /// </summary>
    [Pure]
    private static Type? GetCrudControllerType(Type? t)
    {
        if (t == null)
            return null;
        var compareTo = t;
        if (t.IsGenericType)
            compareTo = t.GetGenericTypeDefinition();
        return compareTo == typeof(CrudController<,>) ? t : GetCrudControllerType(t.BaseType);
        
    }
}
