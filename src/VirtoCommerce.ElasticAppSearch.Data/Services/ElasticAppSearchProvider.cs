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

        var documentResults = new List<DocumentResult>();
        var providerDocuments = documents.Select(ConvertDocument).ToArray();

        #region Temporary workaround

        for (var i = 0; i < providerDocuments.Length; i += 100)
        {
            var range = new Range(i, Math.Min(providerDocuments.Length - 1, i + 100 - 1));
            documentResults.AddRange(await CreateOrUpdateDocumentsAsync(engineName, providerDocuments[range]));
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
        await _elasticAppSearch.CreateEngineAsync(name, ModuleConstants.ElasticSearchApi.Languages.Universal);
    }

    protected virtual async Task<DocumentResult[]> CreateOrUpdateDocumentsAsync(string engineName, SearchDocument[] documents)
    {
        return await _elasticAppSearch.CreateOrUpdateDocuments(engineName, documents);
    }

    protected virtual SearchDocument ConvertDocument(IndexDocument document)
    {
        var result = new SearchDocument { Id = document.Id };

        result.Add(ModuleConstants.ElasticSearchApi.FieldNames.Id, document.Id);

        var fieldsByNames = document.Fields.Select(field => new { FieldName = ConvertFieldName(field.Name), Field = field }).OrderBy(x => x.FieldName).ToArray();
        foreach (var fieldByName in fieldsByNames)
        {
            var fieldName = fieldByName.FieldName;
            var field = fieldByName.Field;
            
            if (field.Name.Length <= ModuleConstants.ElasticSearchApi.FieldNames.MaximumLength)
            {
                result.Add(fieldName, field.IsCollection ? field.Values : field.Value);
            }
        }

        return result;
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
}
