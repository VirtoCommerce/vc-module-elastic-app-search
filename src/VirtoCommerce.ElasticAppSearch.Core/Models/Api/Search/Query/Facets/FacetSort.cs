namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Facets
{
    public record FacetSort : Field<SortOrder>
    {
        public FacetSort(FacetSortField field, SortOrder sortOrder)
        {
            FieldName = field.ToString().ToLowerInvariant();
            Value = sortOrder;
        }
    }
}
