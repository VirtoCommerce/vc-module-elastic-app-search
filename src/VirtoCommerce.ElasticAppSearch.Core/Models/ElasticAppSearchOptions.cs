using System.Collections.Generic;

namespace VirtoCommerce.ElasticAppSearch.Core.Models;

public class ElasticAppSearchOptions
{
    public string Endpoint { get; set; }

    public string PrivateApiKey { get; set; }

    public bool EnableHttpCompression { get; set; }

    public bool EnableSearchQueryDebug { get; set; }

    /// <summary>
    /// Gets or sets Kibana base URL for accessing the Kibana Dashboard from the application menu. 
    /// </summary>
    public string KibanaBaseUrl { get; set; }

    /// <summary>
    /// Gets or sets the path to the App Search engine in the Kibana Dashboard. Default value: /app/enterprise_search/app_search/engines/.
    /// </summary>
    public string KibanaPath { get; set; } = "/app/enterprise_search/app_search/engines/";

    public List<BoostPreset> BoostPresets { get; set; } = new();
}
