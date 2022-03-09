using System.Collections.Generic;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Filters;
using Xunit;

namespace VirtoCommerce.ElasticAppSearch.Tests.Api.Search.Query.Filters;

public class ValueFilterSerializationTests: SerializationTestsBase
{

    public static IEnumerable<object[]> SerializationData => new[]
    {
        new object[] { new SearchQuery
        {
            Query = "test",
            Filters = new ValueFilter<string>
            {
                FieldName = "field",
                Value = new []{ "test" }
            }
        }, "String.json" },
        new object[] { new SearchQuery
        {
            Query = "test",
            Filters = new ValueFilter<double>
            {
                FieldName = "field",
                Value = new []{ 1.01 }
            }
        }, "Number.json" },
        new object[] { new SearchQuery
        {
            Query = "test",
            Filters = new ValueFilter<bool>
            {
                FieldName = "field",
                Value = new []{ true }
            }
        }, "Bool.json" },
    };

    [Theory]
    [MemberData(nameof(SerializationData))]
    public override void Serialize_Entity_CorrectlySerializes<T>(T actual, string expectedJsonFileName)
    {
        base.Serialize_Entity_CorrectlySerializes(actual, expectedJsonFileName);
    }

    protected override string GetJsonPath()
    {
        return $@"{base.GetJsonPath()}\Search\Query\Filters\Single\Value";
    }
}
