using System.Collections.Generic;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Filters;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Sort;
using Xunit;

namespace VirtoCommerce.ElasticAppSearch.Tests.Api.Search.Query;

public class FiltersSerializationTests: SerializationTestsBase
{

    public static IEnumerable<object[]> SerializationData => new[]
    {
        new object[] { new SearchQuery
        {
            Query = "test",
            Filters = new Filters()
        }, @"..\Default.json" },
        new object[] { new SearchQuery
        {
            Query = "test",
            Filters = new ValueFilter<string>
            {
                FieldName = "field",
                Value = new []{ "test" }
            }
        }, @"Single\Value\String.json" },
        new object[] { new SearchQuery
        {
            Query = "test",
            Filters = new ValueFilter<double>
            {
                FieldName = "field",
                Value = new []{ 1.01 }
            }
        }, @"Single\Value\Number.json" },
        new object[] { new SearchQuery
        {
            Query = "test",
            Filters = new ValueFilter<bool>
            {
                FieldName = "field",
                Value = new []{ true }
            }
        }, @"Single\Value\Bool.json" },
        new object[] { new SearchQuery
        {
            Query = "test",
            Filters = new ValueFilter<string>
            {
                FieldName = "field",
                Value = new []{ "test1", "test2" }
            }
        }, @"Single\Value\Array.json" },
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
