namespace StudentApplication.Common.Attributes;

/// <summary>
///   Used by <see cref="StudentApplication.Common.Utils.IModel{T}"/>s to define the REST url for this model
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class RestEndpointAttribute : Attribute
{
    public string Url { get; }
    public RestEndpointAttribute(string url)
    {
        Url = url;
    }
}