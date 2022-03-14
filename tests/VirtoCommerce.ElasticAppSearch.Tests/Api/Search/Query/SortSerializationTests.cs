using System.Collections.Generic;
using System.IO;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Sort;
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
                Sort = new Sort()
            },
            Path.Combine("..", "Default.json")
        },
        new object[]
        {
            new SearchQuery
            {
                Query = "test",
                Sort = new Sort(new[]
                {
                    new Field<SortOrder>
                    {
                        FieldName = "test",
                        Value = SortOrder.Asc
                    },
                })
            },
            "Single.json"
        },
        new object[]
        {
            new SearchQuery
            {
                Query = "test",
                Sort = new Sort(new[]
                {
                    new Field<SortOrder>
                    {
                        FieldName = "test1",
                        Value = SortOrder.Asc
                    },
                    new Field<SortOrder>
                    {
                        FieldName = "test2",
                        Value = SortOrder.Desc
                    }
                })
            },
            "Multiple.json"
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
        return Path.Combine(base.GetJsonPath(), "Search", "Query", "Sort");
    }
}
