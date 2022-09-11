using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace StudentApplication.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ControllerNameAttribute : Attribute
{
    public string Name { get; set; }

    public ControllerNameAttribute(string name)
    {
        Name = name;
    }
}

public class ControllerNameAttributeConvention : IControllerModelConvention
{
    public void Apply(ControllerModel controller)
    {
        var controllerNameAttribute = controller.Attributes.OfType<ControllerNameAttribute>().SingleOrDefault();
        if (controllerNameAttribute != null)
        {
            controller.ControllerName = controllerNameAttribute.Name;
        }
    }
}
