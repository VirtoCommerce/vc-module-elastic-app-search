using System;
using System.Collections.Generic;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Facets;
using Xunit;

namespace VirtoCommerce.ElasticAppSearch.Tests.Api.Search.Query;

public class FacetsSerializationTests : SerializationTestsBase
{
    public static IEnumerable<object[]> SerializationData => new[]
    {
        // simple value facet
        new object[]
        {
            //query
            new SearchQuery
            {
                Query = "park",
                Facets = new Facets
                {
                    { "states", new ValueFacet
                                {
                                    Name = "top-five-states",
                                    Sort = new FacetSort(FacetSortField.Count, SortOrder.Desc),
                                    Size = 5,
                                }
                    },
                    { "world_heritage_site", new ValueFacet { } },
                },
            },
            //json
            "Value.json"
        },
        
        // number range facet
        new object[]
        {
            //query
            new SearchQuery
            {
                Query = "park",
                Facets = new Facets
                {
                    { "acres", new NumberRangeFacet
                                {
                                    Ranges = new[]
                                    {
                                        new FacetRangeValue<double> { From = 1.0d, To = 10000.0d, },
                                        new FacetRangeValue<double> { From = 10000.0d, }
                                    },
                                    Name = "min-and-max-range",
                                }
                    },
                },
            },
            //json
            "RangeNumber.json"
        },
        
        // date range facet
        new object[]
        {
            //query
            new SearchQuery
            {
                Query = "park",
                Facets = new Facets
                {
                    { "date_established", new DateTimeRangeFacet
                                {
                                    Ranges = new[]
                                    {
                                        new FacetRangeValue<DateTime>
                                        {
                                            From = new DateTime(1900, 1, 1, 12, 0, 0, DateTimeKind.Utc),
                                            To = new DateTime(1950, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                                        }
                                    },
                                    Name = "half-century",
                                }
                    },
                },
            },
            //json
            "RangeDate.json"
        },
        
        // geopoint range facet
        new object[]
        {
            //query
            new SearchQuery
            {
                Query = "park",
                Facets = new Facets
                {
                    { "location", new GeoLocationRangeFacet
                                {
                                    Ranges = new[]
                                    {
                                        new FacetRangeValue<double>
                                        {
                                            From = 0,
                                            To = 100000,
                                            Name = "Nearby",
                                        },
                                        new FacetRangeValue<double>
                                        {
                                            From = 100000,
                                            To = 300000,
                                            Name = "Drive",
                                        },
                                        new FacetRangeValue<double>
                                        {
                                            From = 300000,
                                            Name = "Fly",
                                        }
                                    },
                                    Name = "geo-range-from-san-francisco",
                                    Center = new GeoPoint { Latitude = 37.386483, Longitude = -122.083842 },
                                    Unit = MeasurementUnit.M,
                                }
                    },
                },
            },
            //json
            "RangeGeopoint.json"
        },
    };

    [Theory]
    [MemberData(nameof(SerializationData))]
    public override void Serialize_Entity_CorrectlySerializes<T>(T actual, string expectedJsonFileName)
    {
        base.Serialize_Entity_CorrectlySerializes(actual, expectedJsonFileName);
    }

    protected override string GetJsonPath()
    {
        return $@"{base.GetJsonPath()}\Search\Query\Facets";
    }
}
