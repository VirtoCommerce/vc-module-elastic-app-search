using System.Linq;
using System.Text.RegularExpressions;
using VirtoCommerce.ElasticAppSearch.Core;
using VirtoCommerce.ElasticAppSearch.Core.Services;

namespace VirtoCommerce.ElasticAppSearch.Data.Services;

public class FieldNameConverter: IFieldNameConverter
{
    protected virtual string ReservedFieldNamesPrefix => ModuleConstants.Api.FieldNames.ReservedFieldNamesPrefix;

    protected virtual string PrivateFieldPrefix =>  ModuleConstants.Api.FieldNames.PrivateFieldPrefix;

    public virtual string ToProviderFieldName(string indexFieldName)
    {
        // Only lowercase letters allowed
        var providerFieldName = indexFieldName.ToLowerInvariant();

        // Replace private field prefix (double underscore) because field name cannot have leading underscore
        providerFieldName = Regex.Replace(providerFieldName, @"^__", PrivateFieldPrefix);

        // Add special prefix if field name is reserved
        if (ModuleConstants.Api.FieldNames.Reserved.Contains(providerFieldName))
        {
            providerFieldName = $"{ReservedFieldNamesPrefix}{providerFieldName}";
        }

        return providerFieldName;
    }

    public string ToIndexFieldName(string providerFieldName)
    {
        var indexFieldName = providerFieldName;

        // Get back private field prefix 
        indexFieldName = Regex.Replace(indexFieldName, $"^{PrivateFieldPrefix}", "__");

        indexFieldName = Regex.Replace(indexFieldName, $"^{ReservedFieldNamesPrefix}", string.Empty);

        return indexFieldName;
    }
}
