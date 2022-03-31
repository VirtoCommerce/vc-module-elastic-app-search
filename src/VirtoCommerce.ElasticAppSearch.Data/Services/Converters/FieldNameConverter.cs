using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using VirtoCommerce.ElasticAppSearch.Core;
using VirtoCommerce.ElasticAppSearch.Core.Services.Converters;

namespace VirtoCommerce.ElasticAppSearch.Data.Services.Converters;

public class FieldNameConverter : IFieldNameConverter
{
    protected virtual string ReservedFieldNamesPrefix => ModuleConstants.Api.FieldNames.ReservedFieldNamesPrefix;

    protected virtual string PrivateFieldPrefix => ModuleConstants.Api.FieldNames.PrivateFieldPrefix;

    protected virtual Dictionary<string, string> Replacements => new(ModuleConstants.Api.FieldNames.Replacements);

    public virtual string ToProviderFieldName(string indexFieldName)
    {
        // Only lowercase letters allowed
        var providerFieldName = indexFieldName.ToLowerInvariant();

        // Replace whitespaces with underscores
        providerFieldName = Regex.Replace(providerFieldName, @"\W", "_");

        // Replace private field prefix (double underscore) because field name cannot have leading underscore
        providerFieldName = Regex.Replace(providerFieldName, @"^__", PrivateFieldPrefix);

        // Only letters, numbers & underscored allowed. To provide one-to-one mapping, we should replace special symbols
        foreach (var (original, replacement) in Replacements)
        {
            providerFieldName = providerFieldName.Replace(original, replacement);
        }

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

        // Revert back replacements
        foreach (var (original, replacement) in Replacements)
        {
            indexFieldName = indexFieldName.Replace(replacement, original);
        }

        // Remove prefix from fields with reserved name
        indexFieldName = Regex.Replace(indexFieldName, $"^{ReservedFieldNamesPrefix}", string.Empty);

        return indexFieldName;
    }
}
