namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api;

public record Field<TFieldValue>
{
    public virtual string FieldName { get; init; }

    public virtual TFieldValue Value { get; init; }
}
