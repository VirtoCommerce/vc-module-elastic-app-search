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

public class ElasticAppSearchProvider : ISearchProvider, ISupportIndexSwap, ISupportSuggestions
{
    private readonly SearchOptions _searchOptions;
    private readonly IElasticAppSearchApiClient _elasticAppSearch;
    private readonly IDocumentConverter _documentConverter;
    private readonly ISearchQueryBuilder _searchQueryBuilder;
    private readonly ISearchResponseBuilder _searchResponseBuilder;
    private readonly IPlatformMemoryCache _memoryCache;

    protected const int MaxIndexingDocuments = 100;
    protected const string EngineAlias1 = "one";
    protected const string EngineAlias2 = "two";

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
        var sourceEngineName = GetSourceEngineName(ref documentType);
        // Pseudo support index swap
        if (sourceEngineName == null)
        {
            return;
        }

        try
        {
            // Do not delete engine by default
            string deleteEngineName = null;

            var metaEngineName = GetEngineName(documentType);
            var metaEngine = await GetMetaEngineAsync(metaEngineName, required: true);
            var (activeEngineName, stagingEngineName) = GetAliases(sourceEngineName, metaEngine);

            var searchRequest = new SearchRequest
            {
                UseBackupIndex = true,
                Sorting = new[] { new SortingField { FieldName = KnownDocumentFields.IndexationDate, IsDescending = true } },
                Take = 1,
            };

            var searchResponse = await SearchInternalAsync(stagingEngineName, searchRequest);
            if (searchResponse.DocumentsCount == 0)
            {
                ThrowException($"Failed to swap engines. Engine {stagingEngineName} is empty");
            }

            if (activeEngineName != null && activeEngineName != stagingEngineName)
            {
                await DeleteSourceEngineAsync(metaEngineName, activeEngineName);
                deleteEngineName = activeEngineName;
            }

            if (activeEngineName == null || activeEngineName != stagingEngineName)
            {
                await AddSourceEngineAsync(metaEngineName, stagingEngineName);
            }

            if (deleteEngineName != null)
            {
                await DeleteEngineAsync(deleteEngineName);
            }
        }
        catch (SearchException)
        {
            throw;
        }
        catch (Exception ex)
        {
            ThrowException("Failed to swap engines", ex);
        }
    }

    public virtual Task<IndexingResult> IndexWithBackupAsync(string documentType, IList<IndexDocument> documents)
    {
        return IndexInternalAsync(documentType, documents, staging: true);
    }

    #endregion

    #region ISearchProvider implementation

    /// <summary>
    /// Delete staging index
    /// </summary>
    public virtual async Task DeleteIndexAsync(string documentType)
    {
        var engineName = await GetEngineNameAsync(documentType, staging: true);
        if (engineName == null)
        {
            return;
        }

        await DeleteEngineAsync(engineName);
    }

    public virtual Task<IndexingResult> IndexAsync(string documentType, IList<IndexDocument> documents)
    {
        return IndexInternalAsync(documentType, documents, staging: false);
    }

    public virtual async Task<IndexingResult> RemoveAsync(string documentType, IList<IndexDocument> documents)
    {
        var engineName = await GetEngineNameAsync(documentType, staging: false);
        var deleteDocumentsResult = await DeleteDocumentsAsync(engineName, documents.Select(document => document.Id).ToArray());
        var indexingItems = ConvertDeleteDocumentResults(deleteDocumentsResult);
        var indexingResult = new IndexingResult { Items = indexingItems };

        return indexingResult;
    }

    public virtual async Task<SearchResponse> SearchAsync(string documentType, SearchRequest request)
    {
        var engineName = await GetEngineNameAsync(documentType, request.UseBackupIndex);
        if (engineName == null)
        {
            return new SearchResponse();
        }

        return await SearchInternalAsync(engineName, request);
    }

    #endregion

    public virtual async Task<SuggestionResponse> GetSuggestionsAsync(string documentType, SuggestionRequest request)
    {
        var engineName = await GetEngineNameAsync(documentType, request.UseBackupIndex);
        if (engineName == null)
        {
            return new SuggestionResponse();
        }

        var apiQuery = _searchQueryBuilder.ToSuggestionQuery(request);
        var apiResponse = await _elasticAppSearch.GetSuggestionsAsync(engineName, apiQuery);
        var response = _searchResponseBuilder.ToSuggestionResponse(apiResponse);

        return response;
    }


    #region Documents

    #region Create or update

    protected virtual async Task<IndexingResult> IndexInternalAsync(string documentType, IList<IndexDocument> indexDocuments, bool staging)
    {
        var sourceEngineName = GetSourceEngineName(ref documentType);
        var engineName = GetEngineName(documentType);

        if (sourceEngineName != null)
        {
            var metaEngineName = engineName;
            var metaEngine = await GetMetaEngineAsync(metaEngineName, required: false);

            engineName = GetAliasName(sourceEngineName, metaEngine, staging);
            var engineExists = await GetEngineExistsAsync(engineName);
            if (!engineExists)
            {
                await CreateEngineAsync(engineName);
            }

            metaEngine ??= await CreateEngineAsync(metaEngineName, engineName);
            if (!staging && metaEngine.SourceEngines?.Contains(engineName) != true)
            {
                await AddSourceEngineAsync(metaEngineName, engineName);
            }
        }
        else
        {
            var engineExists = await GetEngineExistsAsync(engineName);
            if (!engineExists)
            {
                await CreateEngineAsync(engineName);
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
        }

        var indexingResult = new IndexingResult { Items = indexingResultItems };

        return indexingResult;
    }

    protected virtual Task<CreateOrUpdateDocumentResult[]> CreateOrUpdateDocumentsAsync(string engineName, Document[] documents)
    {
        return _elasticAppSearch.CreateOrUpdateDocumentsAsync(engineName, documents);
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

    #region Search

    protected virtual async Task<SearchResponse> SearchInternalAsync(string engineName, SearchRequest request)
    {
        var schema = await GetSchemaAsync(engineName);

        SearchResponse response;

        if (string.IsNullOrEmpty(request.RawQuery))
        {
            var searchQueries = _searchQueryBuilder.ToSearchQueries(request, schema);
            var searchTasks =
                searchQueries?.Select(searchQuery => _elasticAppSearch.SearchAsync(engineName, searchQuery.SearchQuery))
                ?? new List<Task<SearchResult>>();
            var searchResponses = await Task.WhenAll(searchTasks);

            var searchResults = searchResponses.Select((searchResult, i) => new SearchResultAggregationWrapper
            {
                AggregationId = searchQueries?[i].AggregationId,
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

    #endregion

    #region Schema

    protected virtual Task<Schema> GetSchemaAsync(string engineName)
    {
        var cacheKey = CacheKey.With(GetType(), "GetSchemaAsync", engineName);

        return _memoryCache.GetOrCreateExclusiveAsync(cacheKey, async cacheEntry =>
        {
            cacheEntry.AddExpirationToken(SearchCacheRegion.CreateChangeTokenForKey(engineName));

            return await _elasticAppSearch.GetSchemaAsync(engineName);
        });
    }

    protected virtual async Task UpdateSchemaAsync(string engineName, Schema schema)
    {
        try
        {
            await _elasticAppSearch.UpdateSchemaAsync(engineName, schema);
        }
        finally
        {
            SearchCacheRegion.ExpireTokenForKey(engineName);
        }
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

    #region Engines

    protected virtual async Task<string> GetEngineNameAsync(string documentType, bool staging)
    {
        var sourceEngineName = GetSourceEngineName(ref documentType);

        // Pseudo support index swap
        if (staging && sourceEngineName == null)
        {
            return null;
        }

        var engineName = GetEngineName(documentType);

        if (sourceEngineName != null)
        {
            engineName = await GetAliasName(sourceEngineName, engineName, staging);
        }

        return engineName;
    }

    protected virtual string GetSourceEngineName(ref string documentType)
    {
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

        // Part of the source engine name without alias
        return $"{GetEngineName(documentType)}-{enginePart.ToLowerInvariant()}";
    }

    protected virtual async Task<string> GetAliasName(string sourceEngineName, string metaEngineName, bool staging)
    {
        var metaEngine = await GetMetaEngineAsync(metaEngineName, required: false);

        return GetAliasName(sourceEngineName, metaEngine, staging);
    }

    protected virtual string GetAliasName(string sourceEngineName, Engine metaEngine, bool staging)
    {
        var (activeName, stagingName) = GetAliases(sourceEngineName, metaEngine);

        return staging ? stagingName : activeName;
    }

    protected virtual (string ActiveName, string StagingName) GetAliases(string sourceEngineName, Engine metaEngine)
    {
        var alias1 = GetEngineNameWithAlias(sourceEngineName, EngineAlias1);
        var alias2 = GetEngineNameWithAlias(sourceEngineName, EngineAlias2);

        var activeEngineName = metaEngine?.SourceEngines?.FirstOrDefault(x => x.StartsWith(sourceEngineName, StringComparison.OrdinalIgnoreCase));
        var stagingEngineName = activeEngineName?.EndsWith(EngineAlias1) == true ? alias2 : alias1;

        return (activeEngineName, stagingEngineName);
    }

    protected virtual string GetEngineName(string documentType)
    {
        return string.Join("-", _searchOptions.GetScope(documentType), documentType).ToLowerInvariant();
    }

    protected virtual string GetEngineNameWithAlias(string sourceEngineName, string alias)
    {
        return $"{sourceEngineName}-{alias.ToLowerInvariant()}";
    }

    protected virtual Task<bool> GetEngineExistsAsync(string name)
    {
        return _elasticAppSearch.GetEngineExistsAsync(name);
    }

    protected virtual async Task<Engine> GetMetaEngineAsync(string name, bool required)
    {
        var metaEngine = await GetEngineAsync(name);

        if (metaEngine == null && required)
        {
            ThrowException($"Engine {name} not found");
        }

        if (metaEngine != null && metaEngine.Type != EngineType.Meta)
        {
            ThrowException($"Found engine {name} with default type, but expected meta engine");
        }

        return metaEngine;
    }

    protected virtual Task<Engine> GetEngineAsync(string name)
    {
        return _elasticAppSearch.GetEngineAsync(name);
    }

    protected virtual Task<Engine> CreateEngineAsync(string name, string sourceEngine = null)
    {
        SearchCacheRegion.ExpireTokenForKey(name);

        return _elasticAppSearch.CreateEngineAsync(name, ModuleConstants.Api.Languages.Universal,
            !string.IsNullOrEmpty(sourceEngine) ? new[] { sourceEngine } : null);
    }

    protected virtual async Task DeleteEngineAsync(string name)
    {
        var result = await _elasticAppSearch.DeleteEngineAsync(name);

        if (result.Deleted)
        {
            SearchCacheRegion.ExpireTokenForKey(name);
        }
    }

    protected virtual Task AddSourceEngineAsync(string name, string sourceEngine)
    {
        return _elasticAppSearch.AddSourceEnginesAsync(name, new[] { sourceEngine });
    }

    protected virtual Task DeleteSourceEngineAsync(string name, string sourceEngine)
    {
        return _elasticAppSearch.DeleteSourceEnginesAsync(name, new[] { sourceEngine });
    }

    #endregion

    [DoesNotReturn]
    protected virtual void ThrowException(string message, Exception innerException = null)
    {
        throw new SearchException($"{ModuleConstants.ModuleName}: {message}.", innerException);
    }
}
