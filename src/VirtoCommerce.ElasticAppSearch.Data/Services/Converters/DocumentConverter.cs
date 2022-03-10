using System.Linq;
using VirtoCommerce.ElasticAppSearch.Core;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Documents;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Schema;
using VirtoCommerce.ElasticAppSearch.Core.Services.Converters;
using VirtoCommerce.SearchModule.Core.Model;

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
                schema.Fields.Add(fieldName, ToProviderFieldType(field.ValueType));
            }
        }

        return (document, schema);
    }
    
    protected virtual FieldType ToProviderFieldType(IndexDocumentFieldValueType indexFieldType)
    {
        return indexFieldType switch
        {
            IndexDocumentFieldValueType.Byte or IndexDocumentFieldValueType.Short or IndexDocumentFieldValueType.Integer or IndexDocumentFieldValueType.Long or IndexDocumentFieldValueType.Float or IndexDocumentFieldValueType.Double or IndexDocumentFieldValueType.Decimal => FieldType.Number,
            IndexDocumentFieldValueType.DateTime => FieldType.Date,
            IndexDocumentFieldValueType.GeoPoint => FieldType.Geolocation,
            _ => FieldType.Text,
        };
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
