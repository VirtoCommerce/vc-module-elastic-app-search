using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using VirtoCommerce.ElasticAppSearch.Core;
using VirtoCommerce.ElasticAppSearch.Data.Extensions;
using VirtoCommerce.ElasticAppSearch.Data.Models;

namespace VirtoCommerce.ElasticAppSearch.Data.Services;

public class ApiClient
{
    private const string EnginesEndpoint = "engines";

    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerSettings JsonSerializerSettings = new()
    {
        // Elastic App Search API use camelCase in JSON
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
        Converters = new List<JsonConverter> { new StringEnumConverter(new CamelCaseNamingStrategy()) },

        // Elastic App Search API doesn't support fraction in seconds (probably bug in their ISO 8160 specification support)
        DateFormatString = "yyyy'-'MM'-'dd'T'HH':'mm':'ssK",
        DateTimeZoneHandling = DateTimeZoneHandling.Utc
    };

    public ApiClient(IHttpClientFactory httpClientFactory)
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

    public async Task<CreateOrUpdateDocumentResult[]> CreateOrUpdateDocuments<T>(string engineName, T[] documents)
    {
        var response = await _httpClient.PostAsJsonAsync(GetDocumentsEndpoint(engineName), documents, JsonSerializerSettings);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<CreateOrUpdateDocumentResult[]>(JsonSerializerSettings);
    }

    public async Task<DeleteDocumentResult[]> DeleteDocuments(string engineName, string[] ids)
    {
        var response = await _httpClient.DeleteAsJsonAsync(GetDocumentsEndpoint(engineName), ids, JsonSerializerSettings);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<DeleteDocumentResult[]>(JsonSerializerSettings);
    }

    public async Task<Schema> UpdateSchema(string engineName, Schema schema)
    {
        var response = await _httpClient.PostAsJsonAsync(GetSchemaEndpoint(engineName), schema, JsonSerializerSettings);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<Schema>(JsonSerializerSettings);
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
}
