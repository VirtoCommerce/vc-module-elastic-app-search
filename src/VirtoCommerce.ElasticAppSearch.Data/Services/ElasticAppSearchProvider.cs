using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using VirtoCommerce.ElasticAppSearch.Core.Models;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Core.Services;

namespace VirtoCommerce.ElasticAppSearch.Data.Services;

public class ElasticAppSearchProvider: ISearchProvider
{
    private readonly ElasticAppSearchOptions _elasticAppSearchOptions;
    private readonly SearchOptions _searchOptions;
    private readonly ISettingsManager _settingsManager;
    private readonly ApiClient _elasticAppSearch;

    public ElasticAppSearchProvider(
        IOptions<SearchOptions> searchOptions,
        IOptions<ElasticAppSearchOptions> elasticAppSearchOptions,
        ISettingsManager settingsManager,
        ApiClient elasticAppSearch)
    {
        if (searchOptions == null)
        {
            throw new ArgumentNullException(nameof(searchOptions));
        }

        if (elasticAppSearchOptions == null)
        {
            throw new ArgumentNullException(nameof(elasticAppSearchOptions));
        }

        _elasticAppSearchOptions = elasticAppSearchOptions.Value;
        _searchOptions = searchOptions.Value;

        _settingsManager = settingsManager;
        _elasticAppSearch = elasticAppSearch;
    }

    public async Task DeleteIndexAsync(string documentType)
    {
        await Task.CompletedTask;
    }

    public async Task<IndexingResult> IndexAsync(string documentType, IList<IndexDocument> documents)
    {
        var engineName = GetEngineName(documentType);
        var engineExists = await GetEngineExistsAsync(engineName);
        if (!engineExists)
        {
            await CreateEngineAsync(engineName);
        }

        var temporarySimplifiedDocuments = documents.Select(x => new TemporaryDoc { Id = x.Id, Test = "test" }).ToArray();
        var documentResults = await CreateOrUpdateDocumentsAsync(engineName, temporarySimplifiedDocuments);
        var result = new IndexingResult
        {
            Items = documentResults.Select(documentResult => new IndexingResultItem
            {
                Id = documentResult.Id,
                Succeeded = documentResult.Errors.Length == 0,
                ErrorMessage = string.Join(Environment.NewLine, documentResult.Errors)
            }).ToArray()
        };
        return result;
    }

    public async Task<IndexingResult> RemoveAsync(string documentType, IList<IndexDocument> documents)
    {
        return new IndexingResult();
    }

    public async Task<SearchResponse> SearchAsync(string documentType, SearchRequest request)
    {
        return new SearchResponse();
    }

    protected virtual string GetEngineName(string documentType)
    {
        return string.Join("-", _searchOptions.Scope, documentType).ToLowerInvariant();
    }

    protected virtual async Task<bool> GetEngineExistsAsync(string name)
    {
        return await _elasticAppSearch.GetEngineExistsAsync(name);
    }

    protected virtual async Task CreateEngineAsync(string name)
    {
        await _elasticAppSearch.CreateEngineAsync(name);
    }

    protected virtual async Task<DocumentResult[]> CreateOrUpdateDocumentsAsync<T>(string engineName, T[] documents)
    {
        return await _elasticAppSearch.CreateOrUpdateDocuments(engineName, documents);
    }

    private class TemporaryDoc
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("test")]
        public string Test { get; set; }
    }
}
