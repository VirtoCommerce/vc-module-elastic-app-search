using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using VirtoCommerce.ElasticAppSearch.Core;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Documents;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Schema;
using VirtoCommerce.ElasticAppSearch.Core.Services;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Core.Services;

namespace VirtoCommerce.ElasticAppSearch.Data.Services;

public class ElasticAppSearchProvider: ISearchProvider
{
    private readonly SearchOptions _searchOptions;
    private readonly ApiClient _elasticAppSearch;
    private readonly IDocumentConverter _documentConverter;
    private readonly ISearchQueryBuilder _searchQueryBuilder;
    private readonly ISearchResponseBuilder _searchResponseBuilder;

    public ElasticAppSearchProvider(
        IOptions<SearchOptions> searchOptions,
        ApiClient elasticAppSearch,
        IDocumentConverter documentConverter,
        ISearchQueryBuilder searchQueryBuilder,
        ISearchResponseBuilder searchResponseBuilder)
    {
        if (searchOptions == null)
        {
            throw new ArgumentNullException(nameof(searchOptions));
        }
        
        _searchOptions = searchOptions.Value;
        
        _elasticAppSearch = elasticAppSearch;
        _documentConverter = documentConverter;

        _searchQueryBuilder = searchQueryBuilder;
        _searchResponseBuilder = searchResponseBuilder;
    }

    #region ISearchProvider implementation

    public async Task DeleteIndexAsync(string documentType)
    {
        await Task.CompletedTask;
    }

    public async Task<IndexingResult> IndexAsync(string documentType, IList<IndexDocument> indexDocuments)
    {
        var engineName = GetEngineName(documentType);
        var engineExists = await GetEngineExistsAsync(engineName);
        if (!engineExists)
        {
            await CreateEngineAsync(engineName);
        }

        var documents = new List<Document>();
        var documentSchemas = new List<Schema>();
        foreach (var indexDocument in indexDocuments)
        {
            var (document, documentSchema) = _documentConverter.ToProviderDocument(indexDocument);
            documents.Add(document);
            documentSchemas.Add(documentSchema);
        }

        var schema = new Schema();
        schema.Merge(documentSchemas);

        var indexingResultItems = new List<IndexingResultItem>();

        // Elastic App Search doesn't allow to create or update more than 100 documents at once and this restriction isn't configurable
        for (var currentRangeIndex = 0; currentRangeIndex < documents.Count; currentRangeIndex += 100)
        {
            var currentRangeSize = Math.Min(documents.Count - currentRangeIndex, 100);
            var createOrUpdateDocumentsResult = await CreateOrUpdateDocumentsAsync(engineName, documents.GetRange(currentRangeIndex, currentRangeSize).ToArray());
            indexingResultItems.AddRange(ConvertCreateOrUpdateDocumentResults(createOrUpdateDocumentsResult));
        }

        await UpdateSchema(engineName, schema);

        var indexingResult = new IndexingResult { Items = indexingResultItems };

        return indexingResult;
    }

    public async Task<IndexingResult> RemoveAsync(string documentType, IList<IndexDocument> indexDocuments)
    {
        var engineName = GetEngineName(documentType);
        var deleteDocumentsResult = await DeleteDocumentsAsync(engineName, indexDocuments.Select(indexDocument => indexDocument.Id).ToArray());
        var indexingItems = ConvertDeleteDocumentResults(deleteDocumentsResult);
        var indexingResult = new IndexingResult { Items = indexingItems };
        return indexingResult;
    }

    public async Task<SearchResponse> SearchAsync(string documentType, SearchRequest request)
    {
        var engineName = GetEngineName(documentType);
        var searchQuery = _searchQueryBuilder.ToSearchQuery(request);
        var searchResult = await _elasticAppSearch.SearchAsync(engineName, searchQuery);
        var searchResponse = _searchResponseBuilder.ToSearchResponse(searchResult);
        return searchResponse;
    }

    #endregion

    #region Engines

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
        await _elasticAppSearch.CreateEngineAsync(name, ModuleConstants.Api.Languages.Universal);
    }

    #endregion

    #region Documents

    #region Create or update

    protected virtual async Task<CreateOrUpdateDocumentResult[]> CreateOrUpdateDocumentsAsync(string engineName, Document[] documents)
    {
        return await _elasticAppSearch.CreateOrUpdateDocumentsAsync(engineName, documents);
    }

    protected virtual IndexingResultItem[] ConvertCreateOrUpdateDocumentResults(CreateOrUpdateDocumentResult[] createOrUpdateDocumentResults)
    {
        return createOrUpdateDocumentResults.SelectMany(documentResult =>
        {
            var succeeded = !documentResult.Errors.Any();
            if (succeeded)
            {
                return new[]
                {
                    new IndexingResultItem
                    {
                        Id = documentResult.Id,
                        Succeeded = true
                    }
                };
            }

            return documentResult.Errors.Select(error => new IndexingResultItem
            {
                Id = documentResult.Id,
                Succeeded = false,
                ErrorMessage = error
            });
        }).ToArray();
    }

    #endregion

    #region Delete

    protected virtual async Task<DeleteDocumentResult[]> DeleteDocumentsAsync(string engineName, string[] documentIds)
    {
        return await _elasticAppSearch.DeleteDocumentsAsync(engineName, documentIds);
    }

    protected virtual IndexingResultItem[] ConvertDeleteDocumentResults(DeleteDocumentResult[] deleteDocumentResults)
    {
        return deleteDocumentResults.Select(deleteDocumentResult => new IndexingResultItem
        {
            Id = deleteDocumentResult.Id,
            Succeeded = deleteDocumentResult.Deleted
        }).ToArray();
    }

    #endregion

    #endregion

    #region Schema

    protected virtual async Task UpdateSchema(string engineName, Schema schema)
    {
        await _elasticAppSearch.UpdateSchemaAsync(engineName, schema);
    }

    #endregion
}
