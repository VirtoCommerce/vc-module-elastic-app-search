using System.Collections.Generic;
using System.IO;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query;
using Xunit;

namespace VirtoCommerce.ElasticAppSearch.Tests.Api.Search.Query;

public class SearchQuerySerializationTests : SerializationTestsBase
{
    public static IEnumerable<object[]> SerializationData => new[]
    {
        new object[] { new SearchQuery { Query = "test" }, @"Default.json" },
        new object[] { new SearchQuery
        {
            Query = "test",
            Page = new Page
            {
                Size = 20,
                Current = 1
            }
        }, "Pagination.json" },
    };

    [Theory]
    [MemberData(nameof(SerializationData))]
    public override void Serialize_Entity_CorrectlySerializes<T>(T actual, string expectedJsonFileName)
    {
        base.Serialize_Entity_CorrectlySerializes(actual, expectedJsonFileName);
    }

    [Fact]
    public void Serialize_WithoutRequiredFields_ThrowsException()
    {
        Serialize_InvalidData_ThrowsException(new SearchQuery());
    }

    protected override string GetJsonPath()
    {
        return Path.Combine(base.GetJsonPath(), "Search", "Query");
    }
}
