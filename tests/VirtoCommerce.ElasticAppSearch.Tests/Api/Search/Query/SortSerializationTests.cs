using System;
using System.Collections.Generic;
using System.IO;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Sorting;
using Xunit;

namespace VirtoCommerce.ElasticAppSearch.Tests.Api.Search.Query;

public class SortSerializationTests: SerializationTestsBase
{
    public static IEnumerable<object[]> SerializationData => new[]
    {
        new object[]
        {
            new SearchQuery
            {
                Query = "test",
                Sort = Array.Empty<FieldSort>()
            },
            Path.Combine("..", "Default.json")
        },
        new object[]
        {
            new SearchQuery
            {
                Query = "test",
                Sort = new FieldSort[]
                {
                    new()
                    {
                        FieldName = "test",
                        Value = SortOrder.Asc
                    },
                }
            },
            "Single.json"
        },
        new object[]
        {
            new SearchQuery
            {
                Query = "test",
                Sort = new FieldSort[]
                {
                    new()
                    {
                        FieldName = "test1",
                        Value = SortOrder.Asc
                    },
                    new()
                    {
                        FieldName = "test2",
                        Value = SortOrder.Desc
                    }
                }
            },
            "Multiple.json"
        },
        new object[]
        {
            new SearchQuery
            {
                Query = "test",
                Sort = new GeoDistanceSort[]
                {
                    new()
                    {
                        FieldName = "location",
                        Value = new GeoDistanceSortValue { Center = new GeoPoint(){ Latitude = 38.89, Longitude = -77.08 }, Order = SortOrder.Asc }
                    }
                }
            },
            "Geo.json"
        }
    };

    [Theory]
    [MemberData(nameof(SerializationData))]
    public override void Serialize_Entity_CorrectlySerializes<T>(T actual, string expectedJsonFileName)
    {
        base.Serialize_Entity_CorrectlySerializes(actual, expectedJsonFileName);
    }

    protected override string GetJsonPath()
    {
        return Path.Combine(base.GetJsonPath(), "Search", "Query", "Sort");
    }
}
