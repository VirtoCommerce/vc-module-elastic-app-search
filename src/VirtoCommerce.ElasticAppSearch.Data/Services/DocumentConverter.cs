using System.Linq;
using VirtoCommerce.ElasticAppSearch.Core;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Documents;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Schema;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search;
using VirtoCommerce.ElasticAppSearch.Core.Services;
using VirtoCommerce.SearchModule.Core.Model;

namespace VirtoCommerce.ElasticAppSearch.Data.Services;

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

    public SearchDocument ToSearchDocument(SearchResultDocument searchResultDocument)
    {
        var searchDocument = new SearchDocument { Id = searchResultDocument.Id };
        foreach (var (providerFieldName, value) in searchResultDocument.Fields)
        {
            var indexFieldName = _fieldNameConverter.ToIndexFieldName(providerFieldName);
            searchDocument.Add(indexFieldName, value);
        }

        return searchDocument;
    }
}
