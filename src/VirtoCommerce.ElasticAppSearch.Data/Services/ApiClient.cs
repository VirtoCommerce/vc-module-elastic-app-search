using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using VirtoCommerce.ElasticAppSearch.Core;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api;
using VirtoCommerce.ElasticAppSearch.Data.Extensions;

namespace VirtoCommerce.ElasticAppSearch.Data.Services;

public class ApiClient
{
    private const string EnginesEndpoint = "engines";

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
            await response.EnsureSuccessStatusCodeAsync();
        }

        return response.StatusCode != HttpStatusCode.NotFound;
    }

    public async Task<Engine> GetEngineAsync(string name)
    {
        return await _httpClient.GetFromJsonAsync<Engine>(GetEngineEndpoint(name));
    }

    public async Task CreateEngineAsync(string name, string language)
    {
        var response = await _httpClient.PostAsJsonAsync(EnginesEndpoint, new Engine
        {
            Name = name,
            Type = EngineType.Default,
            Language = language
        });

        await response.EnsureSuccessStatusCodeAsync();
    }

    public async Task<DocumentResult[]> CreateOrUpdateDocuments<T>(string engineName, T[] documents)
    {
        var response = await _httpClient.PostAsJsonAsync(GetDocumentsEndpoint(engineName), documents, new JsonSerializerSettings());

        await response.EnsureSuccessStatusCodeAsync();

        return await response.Content.ReadFromJsonAsync<DocumentResult[]>();
    }

    private static string GetEngineEndpoint(string engineName)
    {
        return $"{EnginesEndpoint}/{engineName}";
    }

    private static string GetDocumentsEndpoint(string engineName)
    {
        return $"{GetEngineEndpoint(engineName)}/documents";
    }
}
