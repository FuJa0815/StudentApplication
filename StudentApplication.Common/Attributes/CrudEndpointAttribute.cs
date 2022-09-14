namespace StudentApplication.Common.Attributes;

/// <summary>
///   Used by <see cref="StudentApplication.Common.Utils.IModel{T}"/>s to define the REST url for this model
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class CrudEndpointAttribute : Attribute
{
    public string Url { get; }
    public CrudEndpointAttribute(string url)
    {
        Url = url;
    }
}