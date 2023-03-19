using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using VirtoCommerce.ElasticAppSearch.Core;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Documents;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Engines;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Schema;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Result;
using VirtoCommerce.ElasticAppSearch.Core.Services;
using VirtoCommerce.ElasticAppSearch.Core.Services.Builders;
using VirtoCommerce.ElasticAppSearch.Core.Services.Converters;
using VirtoCommerce.ElasticAppSearch.Data.Caching;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.SearchModule.Core.Exceptions;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Core.Services;
using Document = VirtoCommerce.ElasticAppSearch.Core.Models.Api.Documents.Document;

namespace VirtoCommerce.ElasticAppSearch.Data.Services;

public class ElasticAppSearchProvider : ISearchProvider
{
    private readonly SearchOptions _searchOptions;
    private readonly IElasticAppSearchApiClient _elasticAppSearch;
    private readonly IDocumentConverter _documentConverter;
    private readonly ISearchQueryBuilder _searchQueryBuilder;
    private readonly ISearchResponseBuilder _searchResponseBuilder;
    private readonly IPlatformMemoryCache _memoryCache;

    private const int _maxIndexingDocuments = 100;

    public ElasticAppSearchProvider(
        IOptions<SearchOptions> searchOptions,
        IElasticAppSearchApiClient elasticAppSearch,
        IDocumentConverter documentConverter,
        ISearchQueryBuilder searchQueryBuilder,
        ISearchResponseBuilder searchResponseBuilder,
        IPlatformMemoryCache memoryCache)
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

        _memoryCache = memoryCache;
    }

    #region ISearchProvider implementation

    public async Task DeleteIndexAsync(string documentType)
    {
        var sourceEngineName = GetSourceEngineName(ref documentType);
        var engineName = sourceEngineName ?? GetEngineName(documentType);
        var result = await _elasticAppSearch.DeleteEngineAsync(engineName);

        if (result.Deleted)
        {
            SearchCacheRegion.ExpireTokenForKey(engineName);
        }
    }

    public async Task<IndexingResult> IndexAsync(string documentType, IList<IndexDocument> indexDocuments)
    {
        var sourceEngineName = GetSourceEngineName(ref documentType);
        var metaEngineName = GetEngineName(documentType);
        var engineName = sourceEngineName ?? metaEngineName;

        var engineExists = await GetEngineExistsAsync(engineName);
        if (!engineExists)
        {
            await CreateEngineAsync(engineName);
        }

        if (sourceEngineName != null)
        {
            var metaEngine = await GetEngineAsync(metaEngineName);
            if (metaEngine == null)
            {
                metaEngine = await CreateEngineAsync(metaEngineName, engineName);
            }
            else if (metaEngine.Type != EngineType.Meta)
            {
                throw new SearchException($"{ModuleConstants.ModuleName}: Found engine {metaEngineName} with default type, but the meta engine is expected.");
            }

            if (metaEngine.SourceEngines?.Contains(engineName) != true)
            {
                await AddSourceEngineAsync(metaEngineName, engineName);
            }
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
        for (var currentRangeIndex = 0; currentRangeIndex < documents.Count; currentRangeIndex += _maxIndexingDocuments)
        {
            var currentRangeSize = Math.Min(documents.Count - currentRangeIndex, _maxIndexingDocuments);
            var createOrUpdateDocumentsResult = await CreateOrUpdateDocumentsAsync(engineName, documents.GetRange(currentRangeIndex, currentRangeSize).ToArray());
            indexingResultItems.AddRange(ConvertCreateOrUpdateDocumentResults(createOrUpdateDocumentsResult));
        }

        // Check if schema was changed
        var oldSchema = await GetSchemaAsync(engineName);
        var schemaChanged = SchemaChanged(oldSchema, newSchema: schema);

        // Update and refresh cache if schema changed
        if (schemaChanged)
        {
            await UpdateSchemaAsync(engineName, schema);
            SearchCacheRegion.ExpireTokenForKey(engineName);
        }

        var indexingResult = new IndexingResult { Items = indexingResultItems };

        return indexingResult;
    }

    public async Task<IndexingResult> RemoveAsync(string documentType, IList<IndexDocument> indexDocuments)
    {
        var sourceEngineName = GetSourceEngineName(ref documentType);
        var engineName = sourceEngineName ?? GetEngineName(documentType);

        var deleteDocumentsResult = await DeleteDocumentsAsync(engineName, indexDocuments.Select(indexDocument => indexDocument.Id).ToArray());
        var indexingItems = ConvertDeleteDocumentResults(deleteDocumentsResult);
        var indexingResult = new IndexingResult { Items = indexingItems };

        return indexingResult;
    }

    public async Task<SearchResponse> SearchAsync(string documentType, SearchRequest request)
    {
        var engineName = GetEngineName(documentType);

        var schema = await GetSchemaAsync(engineName);

        SearchResponse response;

        if (string.IsNullOrEmpty(request.RawQuery))
        {
            var searchQueries = _searchQueryBuilder.ToSearchQueries(request, schema);
            var searchTasks = searchQueries?.Select(searchQuery => _elasticAppSearch.SearchAsync(engineName, searchQuery.SearchQuery))
                ?? new List<Task<SearchResult>>();
            var searchResponses = await Task.WhenAll(searchTasks);

            var searchResults = searchResponses.Select((searchResult, i) => new SearchResultAggregationWrapper
            {
                AggregationId = searchQueries[i].AggregationId,
                SearchResult = searchResult,
            }).ToList();

            response = _searchResponseBuilder.ToSearchResponse(searchResults, request.Aggregations);
        }
        else
        {
            var searchResult = await _elasticAppSearch.SearchAsync(engineName, request.RawQuery);
            response = _searchResponseBuilder.ToSearchResponse(searchResult);
        }

        return response;
    }

    #endregion

    #region Engines

    protected virtual string GetEngineName(string documentType)
    {
        return string.Join("-", _searchOptions.GetScope(documentType), documentType).ToLowerInvariant();
    }

    protected virtual string GetSourceEngineName(ref string documentType)
    {
        if (documentType?.Contains('|') != true)
        {
            return null;
        }

        var parts = documentType.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        documentType = parts[0];
        var sourceEngine = parts.Length > 1 ? parts[1] : null;

        return !string.IsNullOrEmpty(sourceEngine)
            ? string.Join("-", _searchOptions.GetScope(documentType), documentType, sourceEngine).ToLowerInvariant()
            : null;
    }

    protected virtual async Task<bool> GetEngineExistsAsync(string name)
    {
        return await _elasticAppSearch.GetEngineExistsAsync(name);
    }

    protected virtual async Task<Engine> GetEngineAsync(string name)
    {
        return await _elasticAppSearch.GetEngineAsync(name);
    }

    protected virtual async Task<Engine> CreateEngineAsync(string name, string sourceEngine = null)
    {
        return await _elasticAppSearch.CreateEngineAsync(name, ModuleConstants.Api.Languages.Universal,
            !string.IsNullOrEmpty(sourceEngine) ? new []{ sourceEngine } : null);
    }

    protected virtual async Task AddSourceEngineAsync(string name, string sourceEngine)
    {
        await _elasticAppSearch.AddSourceEnginesAsync(name, new []{ sourceEngine });
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

    protected virtual async Task<Schema> GetSchemaAsync(string engineName)
    {
        var cacheKey = CacheKey.With(GetType(), "GetSchemaAsync", engineName);

        return await _memoryCache.GetOrCreateExclusiveAsync(cacheKey, async cacheEntry =>
        {
            cacheEntry.AddExpirationToken(SearchCacheRegion.CreateChangeTokenForKey(engineName));

            return await _elasticAppSearch.GetSchemaAsync(engineName);
        });
    }

    protected virtual async Task UpdateSchemaAsync(string engineName, Schema schema)
    {
        await _elasticAppSearch.UpdateSchemaAsync(engineName, schema);
    }

    private static bool SchemaChanged(Schema oldSchema, Schema newSchema)
    {
        // added fields
        if (newSchema.Fields.Count > oldSchema.Fields.Count)
        {
            return true;
        }

        foreach (var newField in newSchema.Fields)
        {
            // new field present in the schema
            if (!oldSchema.Fields.TryGetValue(newField.Key, out var oldField))
            {
                return true;
            }

            // old field changed type
            if (newField.Value != oldField)
            {
                return true;
            }
        }

        return false;
    }

    #endregion
}
