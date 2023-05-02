namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Sorting
{
    public record GeoDistanceSort : Field<GeoDistanceSortValue>, ISort
    {
    }
}
