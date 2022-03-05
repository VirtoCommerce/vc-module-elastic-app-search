using VirtoCommerce.ElasticAppSearch.Core.Extensions;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Filters.RangeFilters;

public record DecimalRangeFilter : RangeFilter<decimal>
{
    public DecimalRangeFilter(string fieldName, RangeFilterBound<decimal> from, RangeFilterBound<decimal> to):
        base(fieldName, from, fromValue => fromValue.NearestLower(), to, toValue => toValue.NearestHigher())
    {
    }
}
