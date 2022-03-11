using VirtoCommerce.ElasticAppSearch.Core.Extensions;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Filters.RangeFilters;

public record NumberRangeFilter : RangeFilter<double>
{
    public NumberRangeFilter()
    {
    }

    public NumberRangeFilter(string fieldName, RangeFilterBound<double> from, RangeFilterBound<double> to):
        base(fieldName, from, fromValue => fromValue?.GetNearestLower(), to, toValue => toValue?.GetNearestHigher())
    {
    }
}
