using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace StudentApplication.Server.Controllers.Abstract;

/// <summary>
///   Custom contract resolver that skips the serialization of given properties
/// </summary>
public class IgnorePropertiesContractResolver : DefaultContractResolver
{
    private readonly IEnumerable<PropertyInfo> _properties;

    /// <param name="properties">Properties that will be skipped in serialization</param>
    public IgnorePropertiesContractResolver(IEnumerable<PropertyInfo> properties)
    {
        _properties = properties;
    }
    
    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        var prop = base.CreateProperty(member, memberSerialization);
        prop.ShouldSerialize = _ => ShouldSerialize(member);
        return prop;
    }

    private bool ShouldSerialize(MemberInfo memberInfo)
    {
        var propertyInfo = memberInfo as PropertyInfo;
        if (propertyInfo == null)
            return false;

        return !_properties.Any(p => p.MetadataToken == propertyInfo.MetadataToken && p.DeclaringType == propertyInfo.DeclaringType);
    }
}