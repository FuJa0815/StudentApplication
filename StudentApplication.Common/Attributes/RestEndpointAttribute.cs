namespace StudentApplication.Common.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class RestEndpointAttribute : Attribute
{
    public string Url { get; }
    public RestEndpointAttribute(string url)
    {
        Url = url;
    }
}