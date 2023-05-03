using System.Collections.Generic;
using System.IO;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query;
using Xunit;

namespace VirtoCommerce.ElasticAppSearch.Tests.Api.Search.Query;

public class SearchFieldsSerializationTests : SerializationTestsBase
{
    public static IEnumerable<object[]> SerializationData => new[]
    {
        new object[]
        {
            new SearchQuery
            {
                Query = "test",
                SearchFields = new Dictionary<string, SearchFieldValue>()
            },
            Path.Combine("..", "Default.json")
        },
        new object[]
        {
            new SearchQuery
            {
                Query = "test",
                SearchFields = new Dictionary<string, SearchFieldValue>
                {
                    {
                        "test",
                        new SearchFieldValue()
                    }
                }
            },
            "WithoutWeight.json"
        },
        new object[]
        {
            new SearchQuery
            {
                Query = "test",
                SearchFields = new Dictionary<string, SearchFieldValue>
                {
                    {
                        "test",
                        new SearchFieldValue { Weight = 1 }
                    }
                }
            },
            "WithWeight.json"
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
        return Path.Combine(base.GetJsonPath(), "Search", "Query", "SearchFields");
    }
}
