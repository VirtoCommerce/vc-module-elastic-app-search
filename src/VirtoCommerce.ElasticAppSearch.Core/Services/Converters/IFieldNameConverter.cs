namespace VirtoCommerce.ElasticAppSearch.Core.Services.Converters;

public interface IFieldNameConverter
{
    string ToProviderFieldName(string indexFieldName);

    string ToIndexFieldName(string providerFieldName);
}
