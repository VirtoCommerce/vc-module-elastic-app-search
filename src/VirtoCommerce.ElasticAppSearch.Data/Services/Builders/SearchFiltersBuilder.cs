using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using VirtoCommerce.ElasticAppSearch.Core;
using VirtoCommerce.ElasticAppSearch.Core.Extensions;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Schema;
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

    public virtual IFilters ToFilters(ISearchFilter filter, Schema schema)
    {
        var result = ToFilter(filter, schema);
        return result;
    }

    protected virtual IApiFilter ToFilter(ISearchFilter searchFilter, Schema schema)
    {
        IApiFilter result;
        switch (searchFilter)
        {
            case IdsFilter idsFilter:
                result = ToValueFilter(idsFilter);
                break;
            case TermFilter termFilter:
                result = ToValueFilter(termFilter, schema);
                break;
            case WildCardTermFilter:
                Debug.WriteLine("Elastic App Search doesn't support wildcard queries.");
                result = GetNothingFilter();
                break;
            case RangeFilter rangeFilter:
                result = ToRangeFilter(rangeFilter, schema);
                break;
            case GeoDistanceFilter geoDistanceFilter:
                result = ToGeoFilter(geoDistanceFilter, schema);
                break;
            case AndFilter andFilter:
                result = ToAllFilter(andFilter, schema);
                break;
            case OrFilter orFilter:
                result = ToAnyFilter(orFilter, schema);
                break;
            case NotFilter notFilter:
                result = ToNoneFilter(notFilter, schema);
                break;
            case null:
                result = null;
                break;
            default:
                Debug.WriteLine("Unknown filter");
                result = GetNothingFilter();
                break;
        }

        return result;
    }

    protected virtual IApiFilter ToValueFilter(IdsFilter idsFilter)
    {
        var result = new ValueFilter<string>
        {
            FieldName = ModuleConstants.Api.FieldNames.Id,
            Value = idsFilter.Values.ToArray()
        };
        return result;
    }

    protected virtual IApiFilter ToValueFilter(TermFilter termFilter, Schema schema)
    {
        var fieldName = _fieldNameConverter.ToProviderFieldName(termFilter.FieldName);
        var fieldType = schema.Fields.ContainsKey(fieldName) ? (FieldType?)schema.Fields[fieldName] : null;

        IApiFilter result;
        switch (fieldType)
        {

            case null:
                result = GetNothingFilter();
                break;
            case FieldType.Text:
                result = new ValueFilter<string>
                {
                    FieldName = fieldName,
                    Value = termFilter.Values.ToArray()
                };
                break;
            case FieldType.Number:
                result = new ValueFilter<double>
                {
                    FieldName = fieldName,
                    Value = termFilter.Values.Select(value => double.Parse(value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture)).ToArray()
                };
                break;
            case FieldType.Date:
                result = new ValueFilter<DateTime>
                {
                    FieldName = fieldName,
                    Value = termFilter.Values.Select(value => DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal)).ToArray()
                };
                break;
            default:
                Debug.WriteLine("Elastic App Search supports value filter for fields with text, number and date field types only.");
                result = GetNothingFilter();
                break;
        }

        return result;
    }

    protected virtual IApiFilter ToRangeFilter(RangeFilter rangeFilter, Schema schema)
    {
        var fieldName = _fieldNameConverter.ToProviderFieldName(rangeFilter.FieldName);
        var result = new AllFilter
        {
            Value = rangeFilter.Values.Select(rangeFilterValue => ToRangeFilter(fieldName, rangeFilterValue, schema)).ToArray()
        };

        return result;
    }

    protected virtual IApiFilter ToRangeFilter(string fieldName, RangeFilterValue rangeFilterValue, Schema schema)
    {
        var fieldType = schema.Fields.ContainsKey(fieldName) ? (FieldType?)schema.Fields[fieldName] : null;

        IApiFilter result;
        switch (fieldType)
        {
            case null:
                result = GetNothingFilter();
                break;
            case FieldType.Number:
                var isNumberRangeFilter = RangeFilterExtensions.TryParse(fieldName,
                    rangeFilterValue.IncludeLower, rangeFilterValue.Lower,
                    rangeFilterValue.IncludeUpper, rangeFilterValue.Upper,
                    out NumberRangeFilter doubleRangeFilter);
                result = isNumberRangeFilter ? doubleRangeFilter : GetNothingFilter();
                break;
            case FieldType.Date:
                var isDateTimeRangeFilter = RangeFilterExtensions.TryParse(fieldName,
                    rangeFilterValue.IncludeLower, rangeFilterValue.Lower,
                    rangeFilterValue.IncludeUpper, rangeFilterValue.Upper,
                    out DateTimeRangeFilter dateTimeRangeFilter);
                result = isDateTimeRangeFilter ? dateTimeRangeFilter : GetNothingFilter();
                break;
            default:
                Debug.WriteLine("Elastic App Search supports number and date ranges only.");
                result = GetNothingFilter();
                break;
        }
        return result;
    }

    protected virtual IApiFilter ToGeoFilter(GeoDistanceFilter geoDistanceFilter, Schema schema)
    {
        var fieldName = _fieldNameConverter.ToProviderFieldName(geoDistanceFilter.FieldName);
        var fieldType = schema.Fields.ContainsKey(fieldName) ? (FieldType?)schema.Fields[fieldName] : null;

        IApiFilter result;
        switch (fieldType)
        {

            case null:
                result = GetNothingFilter();
                break;
            case FieldType.Geolocation:
                result = new GeoFilter
                {
                    FieldName = fieldName,
                    Value = new GeoFilterValue
                    {
                        Center = new ApiGeoPoint(geoDistanceFilter.Location),
                        Distance = geoDistanceFilter.Distance,
                        Unit = MeasurementUnit.Km
                    }
                };
                break;
            default:
                Debug.WriteLine("Elastic App Search supports geo filter for fields with geolocation field type only.");
                result = GetNothingFilter();
                break;
        }

        return result;
    }

    protected virtual IApiFilter ToAllFilter(AndFilter andFilter, Schema schema)
    {
        var result = new AllFilter
        {
            Value = andFilter.ChildFilters.Select(searchFilter => ToFilter(searchFilter, schema)).ToArray()
        };
        return result;
    }

    protected virtual IApiFilter ToAnyFilter(OrFilter orFilter, Schema schema)
    {
        var result = new AnyFilter
        {
            Value = orFilter.ChildFilters.Select(searchFilter => ToFilter(searchFilter, schema)).ToArray()
        };
        return result;
    }

    protected virtual IApiFilter ToNoneFilter(NotFilter notFilter, Schema schema)
    {
        var result = new NoneFilter
        {
            Value = new []{ ToFilter(notFilter.ChildFilter, schema) }
        };
        return result;
    }

    protected virtual ValueFilter<string> GetNothingFilter()
    {
        var result = new ValueFilter<string>
        {
            FieldName = ModuleConstants.Api.FieldNames.Id,
            Value = new[] { string.Empty }
        };
        return result;
    }
}
