using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VirtoCommerce.ElasticAppSearch.Core;
using VirtoCommerce.ElasticAppSearch.Core.Models;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Documents;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Engines;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Schema;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Result;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Suggestions;
using VirtoCommerce.ElasticAppSearch.Core.Services;
using VirtoCommerce.ElasticAppSearch.Data.Extensions;
using VirtoCommerce.Platform.Core.Common;
using Document = VirtoCommerce.ElasticAppSearch.Core.Models.Api.Documents.Document;

namespace VirtoCommerce.ElasticAppSearch.Data.Services;

public class ElasticAppSearchApiClient : IElasticAppSearchApiClient
{
    protected const string EnginesEndpoint = "engines";

    protected const string DebugHeader = "X-Enterprise-Search-Debug";
    protected const string RequestIdHeader = "X-Request-ID";

    private readonly ILogger<ElasticAppSearchApiClient> _logger;
    private readonly ElasticAppSearchOptions _options;
    private readonly IHttpClientFactory _httpClientFactory;

    public ElasticAppSearchApiClient(
        IHttpClientFactory httpClientFactory,
        ILogger<ElasticAppSearchApiClient> logger,
        IOptions<ElasticAppSearchOptions> options)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _options = options.Value ?? new ElasticAppSearchOptions();
    }

    #region Engine

    public async Task<bool> GetEngineExistsAsync(string name)
    {
        var response = await GetHttpClient().GetAsync(GetEngineEndpoint(name));
        if (response.StatusCode != HttpStatusCode.NotFound)
        {
            await response.EnsureSuccessStatusCodeAsync<Result>(ModuleConstants.Api.JsonSerializerSettings);
        }

        return response.StatusCode != HttpStatusCode.NotFound;
    }

    public async Task<Engine> GetEngineAsync(string name)
    {
        var response = await GetHttpClient().GetAsync(GetEngineEndpoint(name));
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await response.EnsureSuccessStatusCodeAsync<Result>(ModuleConstants.Api.JsonSerializerSettings);

        return await response.Content.ReadFromJsonAsync<Engine>(ModuleConstants.Api.JsonSerializerSettings);
    }

    public async Task<Engine> CreateEngineAsync(string name, string language, string[] sourceEngines = null)
    {
        var response = await GetHttpClient().PostAsJsonAsync(EnginesEndpoint, new Engine
        {
            Name = name,
            Type = sourceEngines.IsNullOrEmpty() ? EngineType.Default : EngineType.Meta,
            Language = language,
            SourceEngines = sourceEngines
        }, ModuleConstants.Api.JsonSerializerSettings);

        await response.EnsureSuccessStatusCodeAsync<Result>(ModuleConstants.Api.JsonSerializerSettings);

        return await response.Content.ReadFromJsonAsync<Engine>(ModuleConstants.Api.JsonSerializerSettings);
    }

    public async Task<DeleteEngineResult> DeleteEngineAsync(string engineName)
    {
        var response = await GetHttpClient().DeleteAsync(GetEngineEndpoint(engineName));

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return new DeleteEngineResult { Deleted = true };
        }

        await response.EnsureSuccessStatusCodeAsync<Result>(ModuleConstants.Api.JsonSerializerSettings);

        var result = await response.Content.ReadFromJsonAsync<DeleteEngineResult>(ModuleConstants.Api.JsonSerializerSettings);

        return result;
    }

    public async Task<Engine> AddSourceEnginesAsync(string engineName, string[] sourceEngines)
    {
        var response = await GetHttpClient().PostAsJsonAsync(GetSourceEnginesEndpoint(engineName),
            sourceEngines, ModuleConstants.Api.JsonSerializerSettings);

        await response.EnsureSuccessStatusCodeAsync<Result>(ModuleConstants.Api.JsonSerializerSettings);

        return await response.Content.ReadFromJsonAsync<Engine>(ModuleConstants.Api.JsonSerializerSettings);
    }

    public async Task<Engine> DeleteSourceEnginesAsync(string engineName, string[] sourceEngines)
    {
        var response = await GetHttpClient().DeleteAsJsonAsync(GetSourceEnginesEndpoint(engineName),
            sourceEngines, ModuleConstants.Api.JsonSerializerSettings);

        await response.EnsureSuccessStatusCodeAsync<Result>(ModuleConstants.Api.JsonSerializerSettings);

        return await response.Content.ReadFromJsonAsync<Engine>(ModuleConstants.Api.JsonSerializerSettings);
    }

    #endregion

    #region Documents

    public async Task<CreateOrUpdateDocumentResult[]> CreateOrUpdateDocumentsAsync(string engineName, Document[] documents)
    {
        var response = await GetHttpClient().PostAsJsonAsync(GetDocumentsEndpoint(engineName), documents, ModuleConstants.Api.JsonSerializerSettings);

        await response.EnsureSuccessStatusCodeAsync<CreateOrUpdateDocumentResult[]>(ModuleConstants.Api.JsonSerializerSettings);

        return await response.Content.ReadFromJsonAsync<CreateOrUpdateDocumentResult[]>(ModuleConstants.Api.JsonSerializerSettings);
    }

    public async Task<DeleteDocumentResult[]> DeleteDocumentsAsync(string engineName, string[] ids)
    {
        var response = await GetHttpClient().DeleteAsJsonAsync(GetDocumentsEndpoint(engineName), ids, ModuleConstants.Api.JsonSerializerSettings);

        await response.EnsureSuccessStatusCodeAsync<DeleteDocumentResult[]>(ModuleConstants.Api.JsonSerializerSettings);

        return await response.Content.ReadFromJsonAsync<DeleteDocumentResult[]>(ModuleConstants.Api.JsonSerializerSettings);
    }

    #endregion

    #region Schema

    public async Task<Schema> GetSchemaAsync(string engineName)
    {
        var response = await GetHttpClient().GetAsync(GetSchemaEndpoint(engineName));

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await response.EnsureSuccessStatusCodeAsync<Result>();

        var result = await response.Content.ReadFromJsonAsync<Schema>();

        return result;
    }

    public async Task<Schema> UpdateSchemaAsync(string engineName, Schema schema)
    {
        var response = await GetHttpClient().PostAsJsonAsync(GetSchemaEndpoint(engineName), schema, ModuleConstants.Api.JsonSerializerSettings);

        await response.EnsureSuccessStatusCodeAsync<Result>();

        return await response.Content.ReadFromJsonAsync<Schema>(ModuleConstants.Api.JsonSerializerSettings);
    }

    #endregion

    #region Search

    public async Task<SearchResult> SearchAsync(string engineName, SearchQuery query)
    {
        var payload = query.ToJson(ModuleConstants.Api.JsonSerializerSettings);

        var preSearchInfo = PreSearch(payload);
        var response = await GetHttpClient().PostAsync(GetSearchEndpoint(engineName), payload, default);
        PostSearch(preSearchInfo);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await response.EnsureSuccessStatusCodeAsync<Result>(ModuleConstants.Api.JsonSerializerSettings);

        var result = await response.Content.ReadFromJsonAsync<SearchResult>(ModuleConstants.Api.JsonSerializerSettings);

        return result;
    }

    public async Task<SearchResult> SearchAsync(string engineName, string rawQuery)
    {
        var content = new StringContent(rawQuery, Encoding.UTF8, "application/json");

        var preSearchInfo = PreSearch(content);
        var response = await GetHttpClient().PostAsync(GetSearchEndpoint(engineName), content, default);
        PostSearch(preSearchInfo);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await response.EnsureSuccessStatusCodeAsync<Result>(ModuleConstants.Api.JsonSerializerSettings);

        var result = await response.Content.ReadFromJsonAsync<SearchResult>(ModuleConstants.Api.JsonSerializerSettings);

        return result;
    }

    #endregion

    #region Search Explain

    public async Task<SearchExplainResult> SearchExplainAsync(string engineName, SearchQuery query)
    {
        var payload = query.ToJson(ModuleConstants.Api.JsonSerializerSettings);

        var preSearchInfo = PreSearch(payload);
        var response = await GetHttpClient().PostAsync(GetSearchExplainEndpoint(engineName), payload, default);
        PostSearch(preSearchInfo);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await response.EnsureSuccessStatusCodeAsync<Result>(ModuleConstants.Api.JsonSerializerSettings);

        var result = await response.Content.ReadFromJsonAsync<SearchExplainResult>(ModuleConstants.Api.JsonSerializerSettings);

        return result;
    }

    public async Task<SearchExplainResult> SearchExplainAsync(string engineName, string rawQuery)
    {
        var content = new StringContent(rawQuery, Encoding.UTF8, "application/json");

        var preSearchInfo = PreSearch(content);
        var response = await GetHttpClient().PostAsync(GetSearchExplainEndpoint(engineName), content, default);
        PostSearch(preSearchInfo);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await response.EnsureSuccessStatusCodeAsync<Result>(ModuleConstants.Api.JsonSerializerSettings);

        var result = await response.Content.ReadFromJsonAsync<SearchExplainResult>(ModuleConstants.Api.JsonSerializerSettings);

        return result;
    }

    #endregion

    #region GetSearchSettingsAsync

    public async Task<SearchSettings> GetSearchSettingsAsync(string engineName)
    {
        var response = await GetHttpClient().GetAsync(GetSearchSettingsEndpoint(engineName));

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await response.EnsureSuccessStatusCodeAsync<Result>(ModuleConstants.Api.JsonSerializerSettings);

        var result = await response.Content.ReadFromJsonAsync<SearchSettings>(ModuleConstants.Api.JsonSerializerSettings);

        return result;
    }

    #endregion

    public async Task<SuggestionApiResponse> GetSuggestionsAsync(string engineName, SuggestionApiQuery query)
    {
        var payload = query.ToJson(ModuleConstants.Api.JsonSerializerSettings);
        var response = await GetHttpClient().PostAsync(GetSuggestionEndpoint(engineName), payload, cancellationToken: default);

        await response.EnsureSuccessStatusCodeAsync<Result>(ModuleConstants.Api.JsonSerializerSettings);

        var result = await response.Content.ReadFromJsonAsync<SuggestionApiResponse>(ModuleConstants.Api.JsonSerializerSettings);

        return result;
    }


    private static string GetEngineEndpoint(string engineName)
    {
        return $"{EnginesEndpoint}/{engineName}";
    }

    private static string GetSourceEnginesEndpoint(string engineName)
    {
        return $"{GetEngineEndpoint(engineName)}/source_engines";
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

    private static string GetSuggestionEndpoint(string engineName)
    {
        return $"{GetEngineEndpoint(engineName)}/search_explain";
    }

    private static string GetSearchExplainEndpoint(string engineName)
    {
        return $"{GetEngineEndpoint(engineName)}/search_explain";
    }

    private static string GetSearchSettingsEndpoint(string engineName)
    {
        return $"{GetEngineEndpoint(engineName)}/search_settings";
    }

    private PreSearchInfo PreSearch(HttpContent payload)
    {
        var preSearchInfo = new PreSearchInfo();

        if (_options.EnableSearchQueryDebug)
        {
            preSearchInfo.RequestId = Guid.NewGuid().ToString("N");

            if (!payload.Headers.Contains(DebugHeader))
            {
                payload.Headers.Add(DebugHeader, "true");
            }

            if (payload.Headers.Contains(RequestIdHeader))
            {
                payload.Headers.Remove(RequestIdHeader);
            }
            payload.Headers.Add(RequestIdHeader, preSearchInfo.RequestId);

            preSearchInfo.RequestStopWatch = Stopwatch.StartNew();
        }

        return preSearchInfo;
    }

    private void PostSearch(PreSearchInfo preSearchInfo)
    {
        if (preSearchInfo?.RequestId == null || preSearchInfo.RequestStopWatch == null)
        {
            return;
        }

        preSearchInfo.RequestStopWatch.Stop();

        _logger.Log(LogLevel.Debug,
            "Elastic App Search query ID: {RequestId}. Query took: {Elapsed} ms",
            preSearchInfo.RequestId, preSearchInfo.RequestStopWatch.ElapsedMilliseconds);
    }

    private HttpClient GetHttpClient()
    {
        return _httpClientFactory.CreateClient(ModuleConstants.ModuleName);
    }
    private sealed class PreSearchInfo
    {
        public string RequestId { get; set; }

        public Stopwatch RequestStopWatch { get; set; }
    }
}
