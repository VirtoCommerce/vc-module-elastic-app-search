using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using VirtoCommerce.ElasticAppSearch.Core;
using VirtoCommerce.ElasticAppSearch.Data.Models.Documents;
using VirtoCommerce.ElasticAppSearch.Data.Models.Schema;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Core.Services;

namespace VirtoCommerce.ElasticAppSearch.Data.Services;

public class ElasticAppSearchProvider: ISearchProvider
{
    private readonly SearchOptions _searchOptions;
    private readonly ApiClient _elasticAppSearch;
    private readonly ElasticAppSearchQueryBuilder _searchQueryBuilder;
    private readonly ElasticAppSearchResponseBuilder _searchResponseBuilder;

    protected virtual string ReservedFieldNamesPrefix => "field_";

    protected virtual string PrivateFieldPrefix => "privatefield_";

    protected virtual string WhitespaceReplacement => "_";

    public ElasticAppSearchProvider(
        IOptions<SearchOptions> searchOptions,
        ApiClient elasticAppSearch,
        ElasticAppSearchQueryBuilder searchQueryBuilder,
        ElasticAppSearchResponseBuilder searchResponseBuilder)
    {
        if (searchOptions == null)
        {
            throw new ArgumentNullException(nameof(searchOptions));
        }
        
        _searchOptions = searchOptions.Value;
        
        _elasticAppSearch = elasticAppSearch;

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
            var (document, documentSchema) = ConvertIndexDocument(indexDocument);
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
            indexingResultItems.AddRange(await CreateOrUpdateDocumentsAsync(engineName, documents.GetRange(currentRangeIndex, currentRangeSize).ToArray()));
        }

        await UpdateSchema(engineName, schema);

        var indexingResult = new IndexingResult { Items = indexingResultItems };

        return indexingResult;
    }

    public async Task<IndexingResult> RemoveAsync(string documentType, IList<IndexDocument> indexDocuments)
    {
        var engineName = GetEngineName(documentType);
        var indexingItems = await DeleteDocumentsAsync(engineName, indexDocuments.Select(indexDocument => indexDocument.Id).ToArray());
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
        await _elasticAppSearch.CreateEngineAsync(name, ModuleConstants.ElasticSearchApi.Languages.Universal);
    }

    #endregion

    #region Documents

    #region Create or update

    protected virtual async Task<IndexingResultItem[]> CreateOrUpdateDocumentsAsync(string engineName, Document[] documents)
    {
        return ConvertCreateOrUpdateDocumentResults(await _elasticAppSearch.CreateOrUpdateDocumentsAsync(engineName, documents));
    }

    protected virtual (Document, Schema) ConvertIndexDocument(IndexDocument indexDocument)
    {
        var document = new Document { Id = indexDocument.Id };
        var schema = new Schema();

        var fieldsByNames = indexDocument.Fields.Select(field => new { FieldName = ConvertFieldName(field.Name), Field = field }).OrderBy(x => x.FieldName).ToArray();
        foreach (var fieldByName in fieldsByNames)
        {
            var fieldName = fieldByName.FieldName;
            var field = fieldByName.Field;
            
            if (field.Name.Length <= ModuleConstants.ElasticSearchApi.FieldNames.MaximumLength)
            {
                document.Fields.Add(fieldName, field.IsCollection ? field.Values : field.Value);
                schema.Fields.Add(fieldName, ConvertFieldType(field.ValueType));
            }
        }

        return (document, schema);
    }

    protected virtual string ConvertFieldName(string name)
    {
        // Only lowercase letters allowed
        var result = name.ToLowerInvariant();

        // Replace private field prefix (double underscore) because field name cannot have leading underscore
        result = Regex.Replace(result, @"^__", PrivateFieldPrefix);

        // Replace whitespaces because field name cannot contain whitespace
        result = Regex.Replace(result, @"\W", WhitespaceReplacement);

        // Add special prefix if field name is reserved
        if (ModuleConstants.ElasticSearchApi.FieldNames.Reserved.Contains(result))
        {
            result = $"{ReservedFieldNamesPrefix}{result}";
        }

        return result;
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

    protected virtual async Task<IndexingResultItem[]> DeleteDocumentsAsync(string engineName, string[] documentIds)
    {
        return ConvertDeleteDocumentResults(await _elasticAppSearch.DeleteDocumentsAsync(engineName, documentIds));
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

    protected virtual FieldType ConvertFieldType(IndexDocumentFieldValueType indexFieldType)
    {
        switch (indexFieldType)
        {
            case IndexDocumentFieldValueType.Byte:
            case IndexDocumentFieldValueType.Short:
            case IndexDocumentFieldValueType.Integer:
            case IndexDocumentFieldValueType.Long:
            case IndexDocumentFieldValueType.Float:
            case IndexDocumentFieldValueType.Double:
            case IndexDocumentFieldValueType.Decimal:
                return FieldType.Number;
            case IndexDocumentFieldValueType.DateTime:
                return FieldType.Date;
            case IndexDocumentFieldValueType.GeoPoint:
                return FieldType.Geolocation;
            default:
                return FieldType.Text;
        }
    }

    #endregion
}
