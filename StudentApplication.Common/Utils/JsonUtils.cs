using System.Diagnostics.Contracts;
using Newtonsoft.Json;

namespace StudentApplication.Common.Utils;

public static class JsonUtils
{
    /// <summary>
    ///   JSON deserializes a server response
    /// </summary>
    [Pure]
    public static async Task<TResp> ReadResultAsync<TResp>(HttpResponseMessage response)
    {
        response.EnsureSuccessStatusCode();
        var stream = await response.Content.ReadAsStreamAsync();
        using StreamReader sr = new(stream);
        using JsonReader reader = new JsonTextReader(sr);
        JsonSerializer serializer = new();
        var obj = serializer.Deserialize<TResp>(reader);
        if (obj == null)
            throw new HttpRequestException("Could not deserialize JSON");
        return obj;
    }
}