using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using VirtoCommerce.ElasticAppSearch.Core.Models;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Core.Services;

namespace VirtoCommerce.ElasticAppSearch.Data.Services;

public class ElasticAppSearchProvider: ISearchProvider
{
    private readonly ElasticAppSearchOptions _elasticAppSearchOptions;
    private readonly SearchOptions _searchOptions;
    private readonly ISettingsManager _settingsManager;
    private readonly ElasticAppSearchApiClient _elasticAppSearch;

    public ElasticAppSearchProvider(
        IOptions<SearchOptions> searchOptions,
        IOptions<ElasticAppSearchOptions> elasticAppSearchOptions,
        ISettingsManager settingsManager,
        ElasticAppSearchApiClient elasticAppSearch)
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
        var engineExists = await CheckEngineExistsAsync(engineName);
        if (!engineExists)
        {
            // TODO
        }
        return new IndexingResult();
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

    protected virtual async Task<bool> CheckEngineExistsAsync(string engineName)
    {
        return await _elasticAppSearch.GetEngineExistsAsync(engineName);
    }
}
