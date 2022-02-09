using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using VirtoCommerce.ElasticAppSearch.Core;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api;

namespace VirtoCommerce.ElasticAppSearch.Data.Services;

public class ElasticAppSearchApiClient
{
    private readonly HttpClient _httpClient;

    public ElasticAppSearchApiClient(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient(ModuleConstants.ModuleName);
    }

    public async Task<bool> GetEngineExistsAsync(string engineName)
    {
        var response = await _httpClient.GetAsync($"engines/{engineName}");
        if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.NotFound)
        {
            throw new HttpRequestException(await response.Content.ReadAsStringAsync(), null, response.StatusCode);
        }

        return response.StatusCode != HttpStatusCode.NotFound;
    }

    public async Task<Engine> GetEngineAsync(string engineName)
    {
        return await _httpClient.GetFromJsonAsync<Engine>($"engines/{engineName}");
    }
}
