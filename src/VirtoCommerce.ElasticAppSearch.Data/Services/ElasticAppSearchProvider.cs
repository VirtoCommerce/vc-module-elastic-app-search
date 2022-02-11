using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Core.Services;

namespace VirtoCommerce.ElasticAppSearch.Data.Services;

public class ElasticAppSearchProvider: ISearchProvider
{
    private readonly SearchOptions _searchOptions;
    private readonly ApiClient _elasticAppSearch;

    public ElasticAppSearchProvider(
        IOptions<SearchOptions> searchOptions,
        ApiClient elasticAppSearch)
    {
        if (searchOptions == null)
        {
            throw new ArgumentNullException(nameof(searchOptions));
        }
        
        _searchOptions = searchOptions.Value;
        
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

        #region Temporary proof-of-concept

        var simplifiedDocuments = documents.Select(x => new { id = x.Id, test = "test" }).ToArray();

        var documentResults = new List<DocumentResult>();
        for (var i = 0; i < simplifiedDocuments.Length; i += 100)
        {
            var range = new Range(i, Math.Min(simplifiedDocuments.Length - 1, i + 100 - 1));
            documentResults.AddRange(await CreateOrUpdateDocumentsAsync(engineName, simplifiedDocuments[range]));
        }

        #endregion

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

    public Task<IndexingResult> RemoveAsync(string documentType, IList<IndexDocument> documents)
    {
        return Task.FromResult(new IndexingResult());
    }

    public Task<SearchResponse> SearchAsync(string documentType, SearchRequest request)
    {
        return Task.FromResult(new SearchResponse());
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
}
