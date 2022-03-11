using System;
using System.Diagnostics;
using System.Linq;
using VirtoCommerce.ElasticAppSearch.Core;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Documents;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Schema;
using VirtoCommerce.ElasticAppSearch.Core.Services.Converters;
using VirtoCommerce.SearchModule.Core.Model;
using SearchGeoPoint = VirtoCommerce.SearchModule.Core.Model.GeoPoint;

namespace VirtoCommerce.ElasticAppSearch.Data.Services.Converters;

public class DocumentConverter: IDocumentConverter
{
    private readonly IFieldNameConverter _fieldNameConverter;

    public DocumentConverter(IFieldNameConverter fieldNameConverter)
    {
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

            if (field.Name.Length <= ModuleConstants.Api.FieldNames.MaximumLength)
            {
                document.Fields.Add(fieldName, field.IsCollection ? field.Values : field.Value);
                schema.Fields.Add(fieldName, ToProviderFieldType(field));
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
            IndexDocumentFieldValueType.Undefined => ToProviderFieldType(indexDocumentField.Name, indexDocumentField.Value),
            _ => FieldType.Text,
        };
    }

    [Obsolete("Left for backward compatibility")]
    protected virtual FieldType ToProviderFieldType(string fieldName, object fieldValue)
    {
        Debug.WriteLine($"The {fieldName} field has undefined value type. It will be detected automatically based on field value object type.");

        var result = fieldValue switch
        {
            sbyte or byte or ushort or short or uint or int or ulong or long or float or double or decimal or TimeSpan => FieldType.Number,
            DateTime or DateTimeOffset => FieldType.Date,
            SearchGeoPoint => FieldType.Geolocation,
            _ => FieldType.Text
        };

        return result;
    }

    public SearchDocument ToSearchDocument(Core.Models.Api.Search.Result.Document searchResultDocument)
    {
        var searchDocument = new SearchDocument { Id = searchResultDocument.Id.Raw as string };
        foreach (var (providerFieldName, value) in searchResultDocument.Fields)
        {
            var indexFieldName = _fieldNameConverter.ToIndexFieldName(providerFieldName);
            var indexFieldValue = value.Raw;
            searchDocument.Add(indexFieldName, indexFieldValue);
        }

        return searchDocument;
    }
}
