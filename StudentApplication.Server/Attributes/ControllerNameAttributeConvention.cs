using System.Diagnostics.Contracts;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using StudentApplication.Common.Attributes;
using StudentApplication.Server.Controllers.Abstract;

namespace StudentApplication.Server.Attributes;

public class ControllerNameAttributeConvention : IControllerModelConvention
{
    public void Apply(ControllerModel controller)
    {
        var attribute = GetRestControllerType(controller.ControllerType)?.GenericTypeArguments?.First()?.GetCustomAttribute<RestEndpointAttribute>();
        if (attribute == null)
            return;
        controller.ControllerName = attribute.Url;
    }

    [Pure]
    private Type? GetRestControllerType(Type? t)
    {
        if (t == null)
            return null;
        var compareTo = t;
        if (t.IsGenericType)
            compareTo = t.GetGenericTypeDefinition();
        return compareTo == typeof(RestController<,>) ? t : GetRestControllerType(t.BaseType);
        
    }
}
