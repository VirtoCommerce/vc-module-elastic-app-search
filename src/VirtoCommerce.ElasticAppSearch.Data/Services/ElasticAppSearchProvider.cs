using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

public class ElasticAppSearchProvider : ISearchProvider, ISupportIndexSwap
{
    private readonly SearchOptions _searchOptions;
    private readonly IElasticAppSearchApiClient _elasticAppSearch;
    private readonly IDocumentConverter _documentConverter;
    private readonly ISearchQueryBuilder _searchQueryBuilder;
    private readonly ISearchResponseBuilder _searchResponseBuilder;
    private readonly IPlatformMemoryCache _memoryCache;

    private const int MaxIndexingDocuments = 100;

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

    #region Implementation of ISupportIndexSwap

    public virtual async Task SwapIndexAsync(string documentType)
    {
        var sourceEngineName = GetSourceEngineName(ref documentType, out var sourceEnginePart);
        //Pseudo support index swap
        if (sourceEngineName == null)
        {
            return;
        }

        try
        {
            //Do not delete engine on default
            string deleteEngineName = null;

            var metaEngineName = GetEngineName(documentType);
            var metaEngine = await GetEngineAsync(metaEngineName);
            if (metaEngine == null)
            {
                ThrowException($"Engine {metaEngineName} not found");
            }
            else if (metaEngine.Type != EngineType.Meta)
            {
                ThrowException($"Found engine {metaEngineName} with default type, but expected meta engine");
            }

            var oldEngineName = metaEngine.SourceEngines?.FirstOrDefault(x => x.StartsWith(sourceEnginePart));
            if (oldEngineName != null && oldEngineName != sourceEngineName)
            {
                await DeleteSourceEngineAsync(metaEngineName, oldEngineName);
                deleteEngineName = oldEngineName;
            }

            if (oldEngineName == null || oldEngineName != sourceEngineName)
            {
                await AddSourceEngineAsync(metaEngineName, sourceEngineName);
            }

            if (deleteEngineName != null)
            {
                await DeleteEngineAsync(deleteEngineName);
            }
        }
        catch (Exception ex)
        {
            ThrowException("Failed to swap engines", ex);
        }
    }

    public virtual Task<IndexingResult> IndexWithBackupAsync(string documentType, IList<IndexDocument> documents)
    {
        return IndexInternalAsync(documentType, documents, true);
    }

    #endregion

    #region ISearchProvider implementation

    public virtual async Task DeleteIndexAsync(string documentType)
    {
        var sourceEngineName = GetSourceEngineName(ref documentType);
        var engineName = sourceEngineName ?? GetEngineName(documentType);

        await DeleteEngineAsync(engineName);
    }

    public virtual Task<IndexingResult> IndexAsync(string documentType, IList<IndexDocument> documents)
    {
        return IndexInternalAsync(documentType, documents);
    }

    public virtual async Task<IndexingResult> RemoveAsync(string documentType, IList<IndexDocument> documents)
    {
        var sourceEngineName = GetSourceEngineName(ref documentType);
        var engineName = sourceEngineName ?? GetEngineName(documentType);

        var deleteDocumentsResult = await DeleteDocumentsAsync(engineName, documents.Select(document => document.Id).ToArray());
        var indexingItems = ConvertDeleteDocumentResults(deleteDocumentsResult);
        var indexingResult = new IndexingResult { Items = indexingItems };

        return indexingResult;
    }

    public virtual async Task<SearchResponse> SearchAsync(string documentType, SearchRequest request)
    {
        var sourceEngineName = GetSourceEngineName(ref documentType);
        //Pseudo support index swap
        if (request.UseBackupIndex && sourceEngineName == null)
        {
            return new SearchResponse();
        }

        var engineName = sourceEngineName ?? GetEngineName(documentType);

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

    protected virtual string GetSourceEngineName(ref string documentType, out string sourceEnginePart)
    {
        sourceEnginePart = null;
        if (documentType?.Contains('|') != true)
        {
            return null;
        }

        var parts = documentType.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        documentType = parts[0];
        var enginePart = parts.Length > 1 ? parts[1] : null;
        if (string.IsNullOrEmpty(enginePart))
        {
            return null;
        }

        //Part of the source engine name without alias
        var sourceEngineName = sourceEnginePart = $"{GetEngineName(documentType)}-{enginePart.ToLowerInvariant()}";

        var aliasPart = parts.Length > 2 ? parts[2] : null;
        if (!string.IsNullOrEmpty(aliasPart))
        {
            sourceEngineName = $"{sourceEngineName}-{aliasPart.ToLowerInvariant()}";
        }

        return sourceEngineName;
    }

    protected string GetSourceEngineName(ref string documentType) => GetSourceEngineName(ref documentType, out _);

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

    protected virtual async Task DeleteEngineAsync(string name)
    {
        var result = await _elasticAppSearch.DeleteEngineAsync(name);

        if (result.Deleted)
        {
            SearchCacheRegion.ExpireTokenForKey(name);
        }
    }

    protected virtual async Task AddSourceEngineAsync(string name, string sourceEngine)
    {
        await _elasticAppSearch.AddSourceEnginesAsync(name, new []{ sourceEngine });
    }

    protected virtual async Task DeleteSourceEngineAsync(string name, string sourceEngine)
    {
        await _elasticAppSearch.DeleteSourceEnginesAsync(name, new []{ sourceEngine });
    }

    #endregion

    #region Documents

    #region Create or update

    protected virtual async Task<IndexingResult> IndexInternalAsync(string documentType, IList<IndexDocument> indexDocuments, bool withBackup = false)
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
                ThrowException($"Found engine {metaEngineName} with default type, but expected meta engine");
            }

            if (!withBackup && metaEngine.SourceEngines?.Contains(engineName) != true)
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
        for (var currentRangeIndex = 0; currentRangeIndex < documents.Count; currentRangeIndex += MaxIndexingDocuments)
        {
            var currentRangeSize = Math.Min(documents.Count - currentRangeIndex, MaxIndexingDocuments);
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

    [DoesNotReturn]
    protected virtual void ThrowException(string message, Exception innerException = null)
    {
        throw new SearchException($"{ModuleConstants.ModuleName}: {message}.", innerException);
    }
}
