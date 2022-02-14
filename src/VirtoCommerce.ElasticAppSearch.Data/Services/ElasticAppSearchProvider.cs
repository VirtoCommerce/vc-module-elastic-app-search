using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using VirtoCommerce.ElasticAppSearch.Core;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Core.Services;

namespace VirtoCommerce.ElasticAppSearch.Data.Services;

public class ElasticAppSearchProvider: ISearchProvider
{
    private readonly SearchOptions _searchOptions;
    private readonly ApiClient _elasticAppSearch;

    protected virtual string ReservedFieldNamesPrefix => "field_";

    protected virtual string PrivateFieldPrefix => "privatefield_";

    protected virtual string WhitespaceReplacement => "_";

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

        var schema = new Schema(documentSchemas);

        var documentResults = new List<DocumentResult>();

        // Elastic App Search doesn't allow to create or update more than 100 documents at once and this restriction isn't configurable
        for (var currentRangeIndex = 0; currentRangeIndex < documents.Count; currentRangeIndex += 100)
        {
            var currentRangeSize = Math.Min(documents.Count - currentRangeIndex, 100);
            documentResults.AddRange(await CreateOrUpdateDocumentsAsync(engineName, documents.GetRange(currentRangeIndex, currentRangeSize).ToArray()));
        }

        await UpdateSchema(engineName, schema);

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

    protected virtual async Task<DocumentResult[]> CreateOrUpdateDocumentsAsync(string engineName, Document[] documents)
    {
        return await _elasticAppSearch.CreateOrUpdateDocuments(engineName, documents);
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
                document.Content.Add(fieldName, field.IsCollection ? field.Values : field.Value);
                schema.Add(fieldName, ConvertFieldType(field.ValueType));
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

    #endregion

    #region Schema

    protected virtual async Task UpdateSchema(string engineName, Schema schema)
    {
        await _elasticAppSearch.UpdateSchema(engineName, schema);
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
