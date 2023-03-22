using System.Collections.Generic;
using System.IO;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Filters.GeoFilter;
using Xunit;

namespace VirtoCommerce.ElasticAppSearch.Tests.Api.Search.Query.Filters;

public class GeoFilterSerializationTests: SerializationTestsBase
{

    public static IEnumerable<object[]> SerializationData => new[]
    {
        new object[] { new SearchQuery
        {
            Query = "test",
            Filters = new GeoFilter
            {
                FieldName = "field",
                Value = new GeoFilterValue
                {
                    Center = new GeoPoint
                    {
                        Latitude = 11.3501959,
                        Longitude = 142.1995228
                    },
                    Unit = MeasurementUnit.Km,
                    Distance = 100
                }
            }
        }, "Distance.json" },
        new object[] { new SearchQuery
        {
            Query = "test",
            Filters = new GeoFilter
            {
                FieldName = "field",
                Value = new GeoFilterValue
                {
                    Center = new GeoPoint
                    {
                        Latitude = 11.3501959,
                        Longitude = 142.1995228
                    },
                    Unit = MeasurementUnit.Km,
                    From = 0,
                    To = 100
                }
            }
        }, "FromTo.json" },
    };

    [Theory]
    [MemberData(nameof(SerializationData))]
    public override void Serialize_Entity_CorrectlySerializes<T>(T actual, string expectedJsonFileName)
    {
        base.Serialize_Entity_CorrectlySerializes(actual, expectedJsonFileName);
    }

    protected override string GetJsonPath()
    {
        return Path.Combine(base.GetJsonPath(), "Search", "Query", "Filters", "Single", "Geo");
    }
}
