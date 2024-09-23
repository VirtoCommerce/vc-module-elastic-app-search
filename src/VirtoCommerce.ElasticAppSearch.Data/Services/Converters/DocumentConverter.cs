using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using VirtoCommerce.ElasticAppSearch.Core;
using VirtoCommerce.ElasticAppSearch.Core.Extensions;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Documents;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Schema;
using VirtoCommerce.ElasticAppSearch.Core.Services.Converters;
using VirtoCommerce.SearchModule.Core.Extensions;
using VirtoCommerce.SearchModule.Core.Model;
using SearchGeoPoint = VirtoCommerce.SearchModule.Core.Model.GeoPoint;

namespace VirtoCommerce.ElasticAppSearch.Data.Services.Converters;

public class DocumentConverter : IDocumentConverter
{
    private readonly ILogger<DocumentConverter> _logger;
    private readonly IFieldNameConverter _fieldNameConverter;

    public DocumentConverter(ILogger<DocumentConverter> logger, IFieldNameConverter fieldNameConverter)
    {
        _logger = logger;
        _fieldNameConverter = fieldNameConverter;
    }

    public virtual (Document, Schema) ToProviderDocument(IndexDocument indexDocument)
    {
        var document = new Document { Id = indexDocument.Id };
        var schema = new Schema();

        var fieldsByNames = indexDocument.Fields
            .Select(field => new
            {
                FieldName = _fieldNameConverter.ToProviderFieldName(field.Name),
                Field = field
            })
            .OrderBy(x => x.FieldName)
            .ToArray();

        foreach (var fieldByName in fieldsByNames)
        {
            var fieldName = fieldByName.FieldName;
            var field = fieldByName.Field;

            if (fieldName.Length > ModuleConstants.Api.FieldNames.MaximumLength)
            {
                _logger.LogCritical("Elastic App Search supports up to 64 symbols in document field name. {FieldName} field name has {FieldNameLength}", fieldName, fieldName.Length);
            }
            else
            {
                document.Fields.Add(fieldName, field.IsCollection ? field.Values : field.Value);
                schema.Fields.Add(fieldName, ToProviderFieldType(field));
            }

            // move inside json converters
            if (field.Name == ModuleConstants.Api.FieldNames.ObjectFieldName && document.Fields.ContainsKey(fieldName))
            {
                document.Fields[fieldName] = field.Value.SerializeJson();
            }
        }

        return (document, schema);
    }

    protected virtual FieldType ToProviderFieldType(IndexDocumentField indexDocumentField)
    {
        var indexDocumentFieldValueType = indexDocumentField.ValueType;
        return indexDocumentFieldValueType switch
        {
            IndexDocumentFieldValueType.Byte or IndexDocumentFieldValueType.Short or IndexDocumentFieldValueType.Integer or IndexDocumentFieldValueType.Long or IndexDocumentFieldValueType.Float or IndexDocumentFieldValueType.Double or IndexDocumentFieldValueType.Decimal => FieldType.Number,
            IndexDocumentFieldValueType.DateTime => FieldType.Date,
            IndexDocumentFieldValueType.GeoPoint => FieldType.Geolocation,
#pragma warning disable CS0618 // Type or member is obsolete
            IndexDocumentFieldValueType.Undefined => ToProviderFieldType(indexDocumentField.Name, indexDocumentField.Value),
#pragma warning restore CS0618 // Type or member is obsolete
            _ => FieldType.Text,
        };
    }

    [Obsolete("Left for backward compatibility")]
    protected virtual FieldType ToProviderFieldType(string fieldName, object fieldValue)
    {
        var fieldType = fieldValue switch
        {
            sbyte or byte or ushort or short or uint or int or ulong or long or float or double or decimal or TimeSpan => FieldType.Number,
            DateTime or DateTimeOffset => FieldType.Date,
            SearchGeoPoint => FieldType.Geolocation,
            _ => FieldType.Text
        };

        _logger.LogInformation("The {FieldName} field has undefined value type. {FieldType} type was detected automatically based on field value object type", fieldName, fieldType);

        return fieldType;
    }

    public SearchDocument ToSearchDocument(Core.Models.Api.Search.Result.SearchResultDocument searchResultDocument)
    {
        var searchDocument = new SearchDocument { Id = searchResultDocument.Id.Raw as string };
        foreach (var (providerFieldName, value) in searchResultDocument.Fields)
        {
            var indexFieldName = _fieldNameConverter.ToIndexFieldName(providerFieldName);

            var indexFieldValue = value.Raw;

            // move inside json converters
            if (indexFieldValue is JArray jArray)
            {
                indexFieldValue = jArray.ToObject<object[]>();
            }

            searchDocument.Add(indexFieldName, indexFieldValue);
        }

        searchDocument.SetRelevanceScore(searchResultDocument.Meta?.Score);

        return searchDocument;
    }
}
