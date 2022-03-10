using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using VirtoCommerce.ElasticAppSearch.Core;
using VirtoCommerce.ElasticAppSearch.Core.Extensions;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Filters;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Filters.CombiningFilters;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Filters.GeoFilter;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Filters.RangeFilters;
using VirtoCommerce.ElasticAppSearch.Core.Services.Builders;
using VirtoCommerce.ElasticAppSearch.Core.Services.Converters;
using VirtoCommerce.SearchModule.Core.Model;
using ISearchFilter = VirtoCommerce.SearchModule.Core.Model.IFilter;
using IApiFilter = VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Filters.IFilter;
using ApiGeoPoint = VirtoCommerce.ElasticAppSearch.Core.Models.Api.GeoPoint;

namespace VirtoCommerce.ElasticAppSearch.Data.Services.Builders;

public class SearchFiltersBuilder: ISearchFiltersBuilder
{
    private readonly IFieldNameConverter _fieldNameConverter;

    public SearchFiltersBuilder(IFieldNameConverter fieldNameConverter)
    {
        _fieldNameConverter = fieldNameConverter;
    }

    public virtual IFilters ToFilters(ISearchFilter filter)
    {
        return ToFilter(filter);
    }

    protected virtual IApiFilter ToFilter(ISearchFilter searchFilter)
    {
        var result = searchFilter switch
        {
            IdsFilter idsFilter => ToValueFilter(idsFilter),
            TermFilter termFilter => ToValueFilter(termFilter),
            WildCardTermFilter => throw new NotSupportedException("Elastic App Search doesn't support wildcard queries"),
            RangeFilter rangeFilter => ToRangeFilter(rangeFilter),
            GeoDistanceFilter geoDistanceFilter => ToGeoFilter(geoDistanceFilter),
            AndFilter andFilter => ToAllFilter(andFilter),
            OrFilter orFilter => ToAnyFilter(orFilter),
            NotFilter notFilter => ToNoneFilter(notFilter),
            null => null,
            _ => throw new NotSupportedException("Unknown filter")
        };
        return result;
    }

    protected virtual ValueFilter<string> ToValueFilter(IdsFilter idsFilter)
    {
        var result = new ValueFilter<string>
        {
            FieldName = ModuleConstants.Api.FieldNames.Id,
            Value = idsFilter.Values.ToArray()
        };
        return result;
    }

    protected virtual ValueFilter<string> ToValueFilter(TermFilter termFilter)
    {
        var result = new ValueFilter<string>
        {
            FieldName = _fieldNameConverter.ToProviderFieldName(termFilter.FieldName),
            Value = termFilter.Values.ToArray()
        };
        return result;
    }

    protected virtual IApiFilter ToRangeFilter(RangeFilter rangeFilter)
    {
        var result = new AllFilter
        {
            Value = rangeFilter.Values
                .Select(rangeFilterValue => ParseRangeFilter(_fieldNameConverter.ToProviderFieldName(rangeFilter.FieldName), rangeFilterValue))
                .ToArray()
        };

        return result;
    }

    protected virtual IApiFilter ParseRangeFilter(string fieldName, RangeFilterValue rangeFilterValue)
    {
        var isDecimalRangeFilter = RangeFilterExtensions.TryParse(fieldName,
            rangeFilterValue.IncludeLower, rangeFilterValue.Lower,
            rangeFilterValue.IncludeUpper, rangeFilterValue.Upper,
            out DecimalRangeFilter decimalRangeFilter);

        if (isDecimalRangeFilter)
        {
            return decimalRangeFilter;
        }

        var isDoubleRangeFilter = RangeFilterExtensions.TryParse(fieldName,
            rangeFilterValue.IncludeLower, rangeFilterValue.Lower,
            rangeFilterValue.IncludeUpper, rangeFilterValue.Upper,
            out DoubleRangeFilter doubleRangeFilter);

        if (isDoubleRangeFilter)
        {
            return doubleRangeFilter;
        }

        var isDateTimeRangeFilter = RangeFilterExtensions.TryParse(fieldName,
            rangeFilterValue.IncludeLower, rangeFilterValue.Lower,
            rangeFilterValue.IncludeUpper, rangeFilterValue.Upper,
            out DateTimeRangeFilter dateTimeRangeFilter);

        if (isDateTimeRangeFilter)
        {
            return dateTimeRangeFilter;
        }

        throw new NotSupportedException("Elastic App Search supports number and date ranges only.");
    }

    protected virtual GeoFilter ToGeoFilter(GeoDistanceFilter geoDistanceFilter)
    {
        var result = new GeoFilter
        {
            FieldName = _fieldNameConverter.ToProviderFieldName(geoDistanceFilter.FieldName),
            Value = new GeoFilterValue
            {
                Center = new ApiGeoPoint(geoDistanceFilter.Location),
                Distance = geoDistanceFilter.Distance,
                Unit = MeasurementUnit.Km
            }
        };
        return result;
    }

    protected virtual AllFilter ToAllFilter(AndFilter andFilter)
    {
        var result = new AllFilter
        {
            Value = andFilter.ChildFilters.Select(ToFilter).ToArray()
        };
        return result;
    }

    protected virtual AnyFilter ToAnyFilter(OrFilter orFilter)
    {
        var result = new AnyFilter
        {
            Value = orFilter.ChildFilters.Select(ToFilter).ToArray()
        };
        return result;
    }

    protected virtual NoneFilter ToNoneFilter(NotFilter notFilter)
    {
        var result = new NoneFilter
        {
            Value = new []{ ToFilter(notFilter.ChildFilter) }
        };
        return result;
    }
}
