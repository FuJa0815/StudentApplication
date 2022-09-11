using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StudentApplication.Controllers.Abstract;

public class IgnorePropertiesConverter<T> : JsonConverter<T> where T : class
{
    private readonly IEnumerable<PropertyInfo> _properties;

    internal IgnorePropertiesConverter(IEnumerable<PropertyInfo> properties)
    {
        _properties = properties;
    }

    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return JsonSerializer.Deserialize(ref reader, typeToConvert, options) as T;
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        foreach (var prop in value.GetType().GetProperties())
        {
            if (prop.GetGetMethod()?.IsStatic != false)
                continue;
            
            if (prop.GetCustomAttributes(typeof(JsonIgnoreAttribute)).Any())
                continue;

            if (_properties.Contains(prop))
                continue;

            
            writer.WritePropertyName(JsonNamingPolicy.CamelCase.ConvertName(
                prop.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? prop.Name));
            JsonSerializer.Serialize(writer, prop.GetValue(value), prop.PropertyType, options);
        }
        writer.WriteEndObject();
        writer.Flush();
    }
}