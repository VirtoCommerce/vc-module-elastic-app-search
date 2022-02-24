using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using VirtoCommerce.ElasticAppSearch.Core;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Documents;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Engines;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Schema;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search;
using VirtoCommerce.ElasticAppSearch.Data.Extensions;

namespace VirtoCommerce.ElasticAppSearch.Data.Services;

public class ElasticAppApiClient : IElasticAppApiClient
{
    private const string EnginesEndpoint = "engines";

    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerSettings JsonSerializerSettings = new()
    {
        // Elastic App Search API use camelCase in JSON
        ContractResolver = new DefaultContractResolver { NamingStrategy = new SnakeCaseNamingStrategy() },
        Converters = new List<JsonConverter> { new StringEnumConverter(new CamelCaseNamingStrategy()) },

        // Elastic App Search API doesn't support fraction in seconds (probably bug in their ISO 8160 specification support)
        DateFormatString = "yyyy'-'MM'-'dd'T'HH':'mm':'ssK",
        DateTimeZoneHandling = DateTimeZoneHandling.Utc,
    };

    public ElasticAppApiClient(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient(ModuleConstants.ModuleName);
    }

    public async Task<bool> GetEngineExistsAsync(string name)
    {
        var response = await _httpClient.GetAsync(GetEngineEndpoint(name));
        if (response.StatusCode != HttpStatusCode.NotFound)
        {
            await response.EnsureSuccessStatusCodeAsync<Result>(JsonSerializerSettings);
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
        }, JsonSerializerSettings);

        await response.EnsureSuccessStatusCodeAsync<Result>(JsonSerializerSettings);

        return await response.Content.ReadFromJsonAsync<Engine>(JsonSerializerSettings);
    }

    public async Task<CreateOrUpdateDocumentResult[]> CreateOrUpdateDocumentsAsync<T>(string engineName, T[] documents)
    {
        var response = await _httpClient.PostAsJsonAsync(GetDocumentsEndpoint(engineName), documents, JsonSerializerSettings);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<CreateOrUpdateDocumentResult[]>(JsonSerializerSettings);
    }

    public async Task<DeleteDocumentResult[]> DeleteDocumentsAsync(string engineName, string[] ids)
    {
        var response = await _httpClient.DeleteAsJsonAsync(GetDocumentsEndpoint(engineName), ids, JsonSerializerSettings);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<DeleteDocumentResult[]>(JsonSerializerSettings);
    }

    public async Task<Schema> UpdateSchemaAsync(string engineName, Schema schema)
    {
        var response = await _httpClient.PostAsJsonAsync(GetSchemaEndpoint(engineName), schema, JsonSerializerSettings);

        await response.EnsureSuccessStatusCodeAsync<Result>();

        return await response.Content.ReadFromJsonAsync<Schema>(JsonSerializerSettings);
    }

    public async Task<SearchResult> SearchAsync(string engineName, SearchQuery query)
    {
        var response = await _httpClient.PostAsJsonAsync(GetSearchEndpoint(engineName), query, JsonSerializerSettings);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<SearchResult>(JsonSerializerSettings);
    }

    public async Task<SearchResult> SearchAsync(string engineName, string rawQuery)
    {
        var response = await _httpClient.PostAsJsonAsync(GetSearchEndpoint(engineName), rawQuery);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<SearchResult>(JsonSerializerSettings);
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
