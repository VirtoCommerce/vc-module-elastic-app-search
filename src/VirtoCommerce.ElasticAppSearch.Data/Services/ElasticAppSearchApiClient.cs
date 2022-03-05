using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using VirtoCommerce.ElasticAppSearch.Core;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Documents;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Engines;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Schema;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Result;
using VirtoCommerce.ElasticAppSearch.Core.Services;
using VirtoCommerce.ElasticAppSearch.Data.Extensions;

namespace VirtoCommerce.ElasticAppSearch.Data.Services;

public class ElasticAppSearchApiClient : IElasticAppSearchApiClient
{
    private const string EnginesEndpoint = "engines";

    private readonly HttpClient _httpClient;

    public ElasticAppSearchApiClient(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient(ModuleConstants.ModuleName);
    }

    public async Task<bool> GetEngineExistsAsync(string name)
    {
        var response = await _httpClient.GetAsync(GetEngineEndpoint(name));
        if (response.StatusCode != HttpStatusCode.NotFound)
        {
            await response.EnsureSuccessStatusCodeAsync<Result>(ModuleConstants.Api.JsonSerializerSettings);
        }

        return response.StatusCode != HttpStatusCode.NotFound;
    }

    public async Task<Engine> CreateEngineAsync(string name, string language)
    {
        var response = await _httpClient.PostAsJsonAsync(EnginesEndpoint, new Engine
        {
            Name = name,
            Type = EngineType.Default,
            Language = language
        }, ModuleConstants.Api.JsonSerializerSettings);

        await response.EnsureSuccessStatusCodeAsync<Result>(ModuleConstants.Api.JsonSerializerSettings);

        return await response.Content.ReadFromJsonAsync<Engine>(ModuleConstants.Api.JsonSerializerSettings);
    }

    public async Task<CreateOrUpdateDocumentResult[]> CreateOrUpdateDocumentsAsync<T>(string engineName, T[] documents)
    {
        var response = await _httpClient.PostAsJsonAsync(GetDocumentsEndpoint(engineName), documents, ModuleConstants.Api.JsonSerializerSettings);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<CreateOrUpdateDocumentResult[]>(ModuleConstants.Api.JsonSerializerSettings);
    }

    public async Task<DeleteDocumentResult[]> DeleteDocumentsAsync(string engineName, string[] ids)
    {
        var response = await _httpClient.DeleteAsJsonAsync(GetDocumentsEndpoint(engineName), ids, ModuleConstants.Api.JsonSerializerSettings);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<DeleteDocumentResult[]>(ModuleConstants.Api.JsonSerializerSettings);
    }

    public async Task<Schema> UpdateSchemaAsync(string engineName, Schema schema)
    {
        var response = await _httpClient.PostAsJsonAsync(GetSchemaEndpoint(engineName), schema, ModuleConstants.Api.JsonSerializerSettings);

        await response.EnsureSuccessStatusCodeAsync<Result>();

        return await response.Content.ReadFromJsonAsync<Schema>(ModuleConstants.Api.JsonSerializerSettings);
    }

    public async Task<SearchResult> SearchAsync(string engineName, SearchQuery query)
    {
        var response = await _httpClient.PostAsJsonAsync(GetSearchEndpoint(engineName), query, ModuleConstants.Api.JsonSerializerSettings);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<SearchResult>(ModuleConstants.Api.JsonSerializerSettings);
    }

    public async Task<SearchResult> SearchAsync(string engineName, string rawQuery)
    {
        var content = new StringContent(rawQuery, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(GetSearchEndpoint(engineName), content);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<SearchResult>(ModuleConstants.Api.JsonSerializerSettings);
    }

    private static string GetEngineEndpoint(string engineName)
    {
        return $"{EnginesEndpoint}/{engineName}";
    }

    private static string GetDocumentsEndpoint(string engineName)
    {
        return $"{GetEngineEndpoint(engineName)}/documents";
    }

    private static string GetSchemaEndpoint(string engineName)
    {
        return $"{GetEngineEndpoint(engineName)}/schema";
    }

    private static string GetSearchEndpoint(string engineName)
    {
        return $"{GetEngineEndpoint(engineName)}/search";
    }
}