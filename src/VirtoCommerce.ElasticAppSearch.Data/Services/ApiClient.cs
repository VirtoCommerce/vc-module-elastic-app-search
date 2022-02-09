using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using VirtoCommerce.ElasticAppSearch.Core;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api;

namespace VirtoCommerce.ElasticAppSearch.Data.Services;

public class ApiClient
{
    private readonly HttpClient _httpClient;

    public ApiClient(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient(ModuleConstants.ModuleName);
    }

    public async Task<bool> GetEngineExistsAsync(string name)
    {
        var response = await _httpClient.GetAsync(GetEngineEndpoint(name));
        if (response.StatusCode != HttpStatusCode.NotFound)
        {
            response.EnsureSuccessStatusCode();
        }

        return response.StatusCode != HttpStatusCode.NotFound;
    }

    public async Task<Engine> GetEngineAsync(string name)
    {
        return await _httpClient.GetFromJsonAsync<Engine>(GetEngineEndpoint(name));
    }

    public async Task CreateEngineAsync(string name, string language = null)
    {
        var response = await _httpClient.PostAsJsonAsync(GetEngineEndpoint(name), new Engine
        {
            Name = name,
            Type = EngineType.Default,
            Language = language
        });

        response.EnsureSuccessStatusCode();
    }

    public async Task<DocumentResult[]> CreateOrUpdateDocuments<T>(string engineName, T[] documents)
    {
        var response = await _httpClient.PostAsJsonAsync(GetDocumentsEndpoint(engineName), documents);

        var request = await response.RequestMessage.Content.ReadAsStringAsync();

        var result = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<DocumentResult[]>();
    }

    private static string GetEngineEndpoint(string engineName)
    {
        return $"engines/{engineName}";
    }

    private static string GetDocumentsEndpoint(string engineName)
    {
        return $"{GetEngineEndpoint(engineName)}/documents";
    }
}
