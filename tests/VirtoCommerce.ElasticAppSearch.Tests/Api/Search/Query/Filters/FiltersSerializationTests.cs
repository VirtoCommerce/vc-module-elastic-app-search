using System.Collections.Generic;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query;
using Xunit;

namespace VirtoCommerce.ElasticAppSearch.Tests.Api.Search.Query.Filters;

public class FiltersSerializationTests: SerializationTestsBase
{

    public static IEnumerable<object[]> SerializationData => new[]
    {
        new object[] { new SearchQuery
        {
            Query = "test",
            Filters = null
        }, @"..\Default.json" },
    };

    [Theory]
    [MemberData(nameof(SerializationData))]
    public override void Serialize_Entity_CorrectlySerializes<T>(T actual, string expectedJsonFileName)
    {
        base.Serialize_Entity_CorrectlySerializes(actual, expectedJsonFileName);
    }

    protected override string GetJsonPath()
    {
        return $@"{base.GetJsonPath()}\Search\Query\Filters";
    }
}
