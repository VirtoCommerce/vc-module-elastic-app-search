using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VirtoCommerce.ElasticAppSearch.Tests.Api;

public static class JsonHelper
{
    public static string LoadFrom(string fileName)
    {
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
        using var fileReader = File.OpenText(path);
        using var jsonTextReader = new JsonTextReader(fileReader)
        {
            DateParseHandling = DateParseHandling.None
        };
        var jToken = JToken.ReadFrom(jsonTextReader);
        return jToken.ToString(Formatting.None);
    }
}
