namespace VirtoCommerce.ElasticAppSearch.Core.Models;

public class ElasticAppSearchOptions
{
    public string Endpoint { get; set; }

    public string PrivateApiKey { get; set; }

    public bool EnableHttpCompression { get; set; }

    public bool EnableSearchQueryDebug { get; set; }
}
