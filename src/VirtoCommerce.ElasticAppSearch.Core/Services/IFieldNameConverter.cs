namespace VirtoCommerce.ElasticAppSearch.Core.Services;

public interface IFieldNameConverter
{
    string ToProviderFieldName(string indexFieldName);

    string ToIndexFieldName(string providerFieldName);
}
