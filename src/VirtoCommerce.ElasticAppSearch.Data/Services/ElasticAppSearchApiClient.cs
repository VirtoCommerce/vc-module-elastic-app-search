using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VirtoCommerce.ElasticAppSearch.Core;
using VirtoCommerce.ElasticAppSearch.Core.Models;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Curations;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Documents;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Engines;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Pagination;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Schema;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Result;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Suggestions;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Synonyms;
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

    public async Task<bool> GetEngineExistsAsync(string name, CancellationToken cancellationToken = default)
    {
        var response = await GetHttpClient().GetAsync(GetEngineEndpoint(name), cancellationToken);
        if (response.StatusCode != HttpStatusCode.NotFound)
        {
            await response.EnsureSuccessStatusCodeAsync<Result>(ModuleConstants.Api.JsonSerializerSettings);
        }

        return response.StatusCode != HttpStatusCode.NotFound;
    }

    public async Task<Engine> GetEngineAsync(string name, CancellationToken cancellationToken = default)
    {
        var response = await GetHttpClient().GetAsync(GetEngineEndpoint(name), cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await response.EnsureSuccessStatusCodeAsync<Result>(ModuleConstants.Api.JsonSerializerSettings);

        return await response.Content.ReadFromJsonAsync<Engine>(ModuleConstants.Api.JsonSerializerSettings, cancellationToken: cancellationToken);
    }

    public async Task<Engine> CreateEngineAsync(string name, string language, string[] sourceEngines = null, CancellationToken cancellationToken = default)
    {
        var response = await GetHttpClient().PostAsJsonAsync(EnginesEndpoint, new Engine
        {
            Name = name,
            Type = sourceEngines.IsNullOrEmpty() ? EngineType.Default : EngineType.Meta,
            Language = language,
            SourceEngines = sourceEngines
        }, ModuleConstants.Api.JsonSerializerSettings,
        cancellationToken: cancellationToken);

        await response.EnsureSuccessStatusCodeAsync<Result>(ModuleConstants.Api.JsonSerializerSettings);

        return await response.Content.ReadFromJsonAsync<Engine>(ModuleConstants.Api.JsonSerializerSettings, cancellationToken: cancellationToken);
    }

    public async Task<DeleteEngineResult> DeleteEngineAsync(string engineName, CancellationToken cancellationToken = default)
    {
        var response = await GetHttpClient().DeleteAsync(GetEngineEndpoint(engineName), cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return new DeleteEngineResult { Deleted = true };
        }

        await response.EnsureSuccessStatusCodeAsync<Result>(ModuleConstants.Api.JsonSerializerSettings);

        var result = await response.Content.ReadFromJsonAsync<DeleteEngineResult>(ModuleConstants.Api.JsonSerializerSettings, cancellationToken: cancellationToken);

        return result;
    }

    public async Task<Engine> AddSourceEnginesAsync(string engineName, string[] sourceEngines, CancellationToken cancellationToken = default)
    {
        var response = await GetHttpClient().PostAsJsonAsync(GetSourceEnginesEndpoint(engineName),
            sourceEngines, ModuleConstants.Api.JsonSerializerSettings,
            cancellationToken: cancellationToken);

        await response.EnsureSuccessStatusCodeAsync<Result>(ModuleConstants.Api.JsonSerializerSettings);

        return await response.Content.ReadFromJsonAsync<Engine>(ModuleConstants.Api.JsonSerializerSettings, cancellationToken: cancellationToken);
    }

    public async Task<Engine> DeleteSourceEnginesAsync(string engineName, string[] sourceEngines, CancellationToken cancellationToken = default)
    {
        var response = await GetHttpClient().DeleteAsJsonAsync(GetSourceEnginesEndpoint(engineName),
            sourceEngines, ModuleConstants.Api.JsonSerializerSettings,
            cancellationToken: cancellationToken);

        await response.EnsureSuccessStatusCodeAsync<Result>(ModuleConstants.Api.JsonSerializerSettings);

        return await response.Content.ReadFromJsonAsync<Engine>(ModuleConstants.Api.JsonSerializerSettings, cancellationToken: cancellationToken);
    }

    #endregion

    #region Documents

    public async Task<CreateOrUpdateDocumentResult[]> CreateOrUpdateDocumentsAsync(string engineName, Document[] documents, CancellationToken cancellationToken = default)
    {
        var response = await GetHttpClient().PostAsJsonAsync(GetDocumentsEndpoint(engineName), documents, ModuleConstants.Api.JsonSerializerSettings, cancellationToken: cancellationToken);

        await response.EnsureSuccessStatusCodeAsync<CreateOrUpdateDocumentResult[]>(ModuleConstants.Api.JsonSerializerSettings);

        return await response.Content.ReadFromJsonAsync<CreateOrUpdateDocumentResult[]>(ModuleConstants.Api.JsonSerializerSettings, cancellationToken: cancellationToken);
    }

    public async Task<DeleteDocumentResult[]> DeleteDocumentsAsync(string engineName, string[] ids, CancellationToken cancellationToken = default)
    {
        var response = await GetHttpClient().DeleteAsJsonAsync(GetDocumentsEndpoint(engineName), ids, ModuleConstants.Api.JsonSerializerSettings, cancellationToken: cancellationToken);

        await response.EnsureSuccessStatusCodeAsync<DeleteDocumentResult[]>(ModuleConstants.Api.JsonSerializerSettings);

        return await response.Content.ReadFromJsonAsync<DeleteDocumentResult[]>(ModuleConstants.Api.JsonSerializerSettings, cancellationToken: cancellationToken);
    }

    #endregion

    #region Schema

    public async Task<Schema> GetSchemaAsync(string engineName, CancellationToken cancellationToken = default)
    {
        var response = await GetHttpClient().GetAsync(GetSchemaEndpoint(engineName), cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await response.EnsureSuccessStatusCodeAsync<Result>();

        var result = await response.Content.ReadFromJsonAsync<Schema>(cancellationToken: cancellationToken);

        return result;
    }

    public async Task<Schema> UpdateSchemaAsync(string engineName, Schema schema, CancellationToken cancellationToken = default)
    {
        var response = await GetHttpClient().PostAsJsonAsync(GetSchemaEndpoint(engineName), schema, ModuleConstants.Api.JsonSerializerSettings, cancellationToken: cancellationToken);

        await response.EnsureSuccessStatusCodeAsync<Result>();

        return await response.Content.ReadFromJsonAsync<Schema>(ModuleConstants.Api.JsonSerializerSettings, cancellationToken: cancellationToken);
    }

    #endregion

    #region Search

    public async Task<SearchResult> SearchAsync(string engineName, SearchQuery query, CancellationToken cancellationToken = default)
    {
        var payload = query.ToJson(ModuleConstants.Api.JsonSerializerSettings);

        var preSearchInfo = PreSearch(payload);
        var response = await GetHttpClient().PostAsync(GetSearchEndpoint(engineName), payload, cancellationToken);
        PostSearch(preSearchInfo);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await response.EnsureSuccessStatusCodeAsync<Result>(ModuleConstants.Api.JsonSerializerSettings);

        var result = await response.Content.ReadFromJsonAsync<SearchResult>(ModuleConstants.Api.JsonSerializerSettings, cancellationToken: cancellationToken);

        return result;
    }

    public async Task<SearchResult> SearchAsync(string engineName, string rawQuery, CancellationToken cancellationToken = default)
    {
        var content = new StringContent(rawQuery, Encoding.UTF8, "application/json");

        var preSearchInfo = PreSearch(content);
        var response = await GetHttpClient().PostAsync(GetSearchEndpoint(engineName), content, cancellationToken);
        PostSearch(preSearchInfo);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await response.EnsureSuccessStatusCodeAsync<Result>(ModuleConstants.Api.JsonSerializerSettings);

        var result = await response.Content.ReadFromJsonAsync<SearchResult>(ModuleConstants.Api.JsonSerializerSettings, cancellationToken: cancellationToken);

        return result;
    }

    #endregion

    #region Search Explain

    public async Task<SearchExplainResult> SearchExplainAsync(string engineName, SearchQuery query, CancellationToken cancellationToken = default)
    {
        var payload = query.ToJson(ModuleConstants.Api.JsonSerializerSettings);

        // SearchQueryDebug in PreSearch(payload) is not working with SearchExplain
        var response = await GetHttpClient().PostAsync(GetSearchExplainEndpoint(engineName), payload, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await response.EnsureSuccessStatusCodeAsync<Result>(ModuleConstants.Api.JsonSerializerSettings);

        var result = await response.Content.ReadFromJsonAsync<SearchExplainResult>(ModuleConstants.Api.JsonSerializerSettings, cancellationToken: cancellationToken);

        return result;
    }

    public async Task<SearchExplainResult> SearchExplainAsync(string engineName, string rawQuery, CancellationToken cancellationToken = default)
    {
        var content = new StringContent(rawQuery, Encoding.UTF8, "application/json");

        // SearchQueryDebug in PreSearch(content) is not working with SearchExplain
        var response = await GetHttpClient().PostAsync(GetSearchExplainEndpoint(engineName), content, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await response.EnsureSuccessStatusCodeAsync<Result>(ModuleConstants.Api.JsonSerializerSettings);

        var result = await response.Content.ReadFromJsonAsync<SearchExplainResult>(ModuleConstants.Api.JsonSerializerSettings, cancellationToken: cancellationToken);

        return result;
    }

    public async Task<ElasticSearchExplainResult> ElasticSearchExplainAsync(string engineName, string rawQuery, CancellationToken cancellationToken = default)
    {
        var content = new StringContent(rawQuery, Encoding.UTF8, "application/json");

        // SearchQueryDebug in PreSearch(content) does not affect ElasticSearch request
        var response = await GetHttpClient().PostAsync(GetElasticSearchExplainEndpoint(engineName), content, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await response.EnsureSuccessStatusCodeAsync<Result>(ModuleConstants.Api.JsonSerializerSettings);

        var result = await response.Content.ReadFromJsonAsync<ElasticSearchExplainResult>(ModuleConstants.Api.JsonSerializerSettings, cancellationToken: cancellationToken);

        return result;
    }

    #endregion

    #region GetSearchSettingsAsync

    public async Task<SearchSettings> GetSearchSettingsAsync(string engineName, CancellationToken cancellationToken = default)
    {
        var response = await GetHttpClient().GetAsync(GetSearchSettingsEndpoint(engineName), cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await response.EnsureSuccessStatusCodeAsync<Result>(ModuleConstants.Api.JsonSerializerSettings);

        var result = await response.Content.ReadFromJsonAsync<SearchSettings>(ModuleConstants.Api.JsonSerializerSettings, cancellationToken: cancellationToken);

        return result;
    }

    #endregion

    #region Curations

    public async Task<CurationSearchResult> GetCurationsAsync(string engineName, int skip, int take, CancellationToken cancellationToken = default)
    {
        var response = await GetHttpClient().GetAsync(GetCurationsEndpoint(engineName, skip, take), cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await response.EnsureSuccessStatusCodeAsync<Result>(ModuleConstants.Api.JsonSerializerSettings);

        var result = await response.Content.ReadFromJsonAsync<PaginatedResponse>(ModuleConstants.Api.JsonSerializerSettings, cancellationToken: cancellationToken);

        return new CurationSearchResult
        {
            Results = result.Results.ToObject<Curation[]>(),
            TotalCount = result.Meta.Page.TotalResults,
        };
    }

    public async Task<Curation> GetCurationAsync(string engineName, string curationId, bool skipAnalytics = true, CancellationToken cancellationToken = default)
    {
        var response = await GetHttpClient().GetAsync(GetCurationEndpoint(engineName, curationId, skipAnalytics), cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await response.EnsureSuccessStatusCodeAsync<Result>(ModuleConstants.Api.JsonSerializerSettings);

        var result = await response.Content.ReadFromJsonAsync<Curation>(ModuleConstants.Api.JsonSerializerSettings, cancellationToken: cancellationToken);

        return result;
    }

    #endregion

    #region Suggestions

    public async Task<SuggestionApiResponse> GetSuggestionsAsync(string engineName, SuggestionApiQuery query, CancellationToken cancellationToken = default)
    {
        var payload = query.ToJson(ModuleConstants.Api.JsonSerializerSettings);
        var response = await GetHttpClient().PostAsync(GetSuggestionEndpoint(engineName), payload, cancellationToken: cancellationToken);

        await response.EnsureSuccessStatusCodeAsync<Result>(ModuleConstants.Api.JsonSerializerSettings);

        var result = await response.Content.ReadFromJsonAsync<SuggestionApiResponse>(ModuleConstants.Api.JsonSerializerSettings, cancellationToken: cancellationToken);

        return result;
    }

    #endregion

    #region Synonyms

    public async Task<SynonymApiResponse> GetSynonymsAsync(string engineName, SynonymApiQuery query, CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri(GetSynonymsEndpoint(engineName), UriKind.RelativeOrAbsolute),
            Content = query?.ToJson(ModuleConstants.Api.JsonSerializerSettings),
        };

        var response = await GetHttpClient().SendAsync(request, cancellationToken: cancellationToken);

        await response.EnsureSuccessStatusCodeAsync<Result>(ModuleConstants.Api.JsonSerializerSettings);
        var result = await response.Content.ReadFromJsonAsync<SynonymApiResponse>(ModuleConstants.Api.JsonSerializerSettings, cancellationToken: cancellationToken);

        return result;
    }

    public async Task<SynonymApiDocument> GetSynonymSetAsync(string engineName, string id, CancellationToken cancellationToken = default)
    {
        var response = await GetHttpClient().GetAsync(GetSynonymsEndpoint(engineName, id), cancellationToken: cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await response.EnsureSuccessStatusCodeAsync<Result>(ModuleConstants.Api.JsonSerializerSettings);
        var result = await response.Content.ReadFromJsonAsync<SynonymApiDocument>(ModuleConstants.Api.JsonSerializerSettings, cancellationToken: cancellationToken);

        return result;
    }

    public async Task<SynonymApiDocument> CreateSynonymSetAsync(string engineName, SynonymSet synonymSet, CancellationToken cancellationToken = default)
    {
        var payload = synonymSet.ToJson(ModuleConstants.Api.JsonSerializerSettings);
        var response = await GetHttpClient().PostAsync(GetSynonymsEndpoint(engineName), payload, cancellationToken: cancellationToken);

        await response.EnsureSuccessStatusCodeAsync<Result>(ModuleConstants.Api.JsonSerializerSettings);
        var result = await response.Content.ReadFromJsonAsync<SynonymApiDocument>(ModuleConstants.Api.JsonSerializerSettings, cancellationToken: cancellationToken);

        return result;
    }

    public async Task<SynonymApiDocument> UpdateSynonymSetAsync(string engineName, string id, SynonymSet synonymSet, CancellationToken cancellationToken = default)
    {
        var payload = synonymSet.ToJson(ModuleConstants.Api.JsonSerializerSettings);
        var response = await GetHttpClient().PutAsync(GetSynonymsEndpoint(engineName, id), payload, cancellationToken: cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await response.EnsureSuccessStatusCodeAsync<Result>(ModuleConstants.Api.JsonSerializerSettings);
        var result = await response.Content.ReadFromJsonAsync<SynonymApiDocument>(ModuleConstants.Api.JsonSerializerSettings, cancellationToken: cancellationToken);

        return result;
    }

    public async Task<DeleteDocumentResult> DeleteSynonymSetAsync(string engineName, string id, CancellationToken cancellationToken = default)
    {
        var response = await GetHttpClient().DeleteAsync(GetSynonymsEndpoint(engineName, id), cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return new DeleteDocumentResult { Deleted = true };
        }

        await response.EnsureSuccessStatusCodeAsync<Result>(ModuleConstants.Api.JsonSerializerSettings);

        var result = await response.Content.ReadFromJsonAsync<DeleteDocumentResult>(ModuleConstants.Api.JsonSerializerSettings, cancellationToken: cancellationToken);

        return result;
    }

    #endregion

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
        return $"{GetEngineEndpoint(engineName)}/query_suggestion";
    }

    private static string GetSearchExplainEndpoint(string engineName)
    {
        return $"{GetEngineEndpoint(engineName)}/search_explain";
    }

    private static string GetElasticSearchExplainEndpoint(string engineName)
    {
        return $"{GetEngineEndpoint(engineName)}/elasticsearch/_search?explain=true";
    }

    private static string GetSearchSettingsEndpoint(string engineName)
    {
        return $"{GetEngineEndpoint(engineName)}/search_settings";
    }

    private static string GetCurationsEndpoint(string engineName, int skip, int take = 1)
    {
        if (take <= 0)
        {
            take = 1;
        }

        return $"{GetEngineEndpoint(engineName)}/curations?page%5Bcurrent%5D={skip / take + 1}&page%5Bsize%5D={take}";
    }

    private static string GetCurationEndpoint(string engineName, string curationName, bool skipAnalytics = true)
    {
        return $"{GetEngineEndpoint(engineName)}/curations/{curationName}" + (skipAnalytics ? "?skip_record_analytics=true" : "");
    }

    private static string GetSynonymsEndpoint(string engineName, string id = null)
    {
        return $"{GetEngineEndpoint(engineName)}/synonyms{(!string.IsNullOrEmpty(id) ? $"/{id}" : null)}";
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
