using Newtonsoft.Json;

namespace VirtoCommerce.ElasticAppSearch.Core.Extensions
{
    public static class SerializationExtensions
    {
        public static string SerializeJson(this object source, JsonSerializerSettings settings = null)
        {
            if (settings == null)
            {
                settings = new JsonSerializerSettings
                {
                    DefaultValueHandling = DefaultValueHandling.Include,
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Formatting.None,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    TypeNameHandling = TypeNameHandling.None,
                };
            }

            return JsonConvert.SerializeObject(source, settings);
        }
    }
}
