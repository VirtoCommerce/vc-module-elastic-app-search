using System;
using System.Collections.Generic;
using System.IO;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Filters.RangeFilters;
using Xunit;

namespace VirtoCommerce.ElasticAppSearch.Tests.Api.Search.Query.Filters;

public class RangeFilterSerializationTests : SerializationTestsBase
{
    public static IEnumerable<object[]> SerializationData => new[]
    {
        new object[] { new SearchQuery
        {
            Query = "test",
            Filters = new NumberRangeFilter
            {
                FieldName = "field",
                Value = new RangeValue<double>
                {
                    From = 0.1d,
                    To = 10.0d
                }
            }
        }, "Double.json" },
        new object[] { new SearchQuery
        {
            Query = "test",
            Filters = new DateTimeRangeFilter
            {
                FieldName = "field",
                Value = new RangeValue<DateTime>
                {
                    From = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    To = new DateTime(1999, 12, 31, 23, 59, 59, DateTimeKind.Utc)
                }
            }
        }, "Date.json" },
    };

    [Theory]
    [MemberData(nameof(SerializationData))]
    public override void Serialize_Entity_CorrectlySerializes<T>(T actual, string expectedJsonFileName)
    {
        base.Serialize_Entity_CorrectlySerializes(actual, expectedJsonFileName);
    }

    protected override string GetJsonPath()
    {
        return Path.Combine(base.GetJsonPath(), "Search", "Query", "Filters", "Single", "Range");
    }
}
