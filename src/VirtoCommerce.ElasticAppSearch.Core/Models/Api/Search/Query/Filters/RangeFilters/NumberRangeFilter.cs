using VirtoCommerce.ElasticAppSearch.Core.Extensions;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Filters.RangeFilters;

public record NumberRangeFilter : RangeFilter<double>
{
    public NumberRangeFilter()
    {
    }

    public NumberRangeFilter(string fieldName, RangeBound<double> from, RangeBound<double> to) :
        base(fieldName, from, fromValue => fromValue?.GetNearestHigher(), to, toValue => toValue?.GetNearestHigher())
    {
    }
}
