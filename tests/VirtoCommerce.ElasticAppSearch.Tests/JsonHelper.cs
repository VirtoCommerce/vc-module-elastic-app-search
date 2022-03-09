using System;
using System.IO;
using Newtonsoft.Json.Linq;

namespace VirtoCommerce.ElasticAppSearch.Tests;

public static class JsonHelper
{
    public static string LoadFrom(string fileName)
    {
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
        var json = File.ReadAllText(path);
        // Validate file
        JToken.Parse(json);
        return json;
    }
}
