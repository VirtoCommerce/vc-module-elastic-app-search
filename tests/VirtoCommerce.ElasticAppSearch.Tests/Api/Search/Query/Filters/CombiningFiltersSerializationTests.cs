using System.Collections.Generic;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Filters;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Filters.CombiningFilters;
using Xunit;

namespace VirtoCommerce.ElasticAppSearch.Tests.Api.Search.Query.Filters;

public class CombiningFiltersSerializationTests: SerializationTestsBase
{
    private static IFilter[] Filters => new IFilter[]
    {
        new ValueFilter<string>
        {
            FieldName = "field1",
            Value = new[] { "test1" }
        },
        new ValueFilter<string>
        {
            FieldName = "field2",
            Value = new[] { "test2" }
        }
    };

    public static IEnumerable<object[]> SerializationData => new[]
    {
        new object[]
        {
            new SearchQuery
            {
                Query = "test",
                Filters = new AllFilter { Value = Filters }
            },
            @"Single\Combining\All.json"
        },
        new object[]
        {
            new SearchQuery
            {
                Query = "test",
                Filters = new AnyFilter { Value = Filters }
            },
            @"Single\Combining\Any.json"
        },
        new object[]
        {
            new SearchQuery
            {
                Query = "test",
                Filters = new NoneFilter { Value = Filters }
            },
            @"Single\Combining\None.json"
        },
        new object[]
        {
            new SearchQuery
            {
                Query = "test",
                Filters = new Core.Models.Api.Search.Query.Filters.Filters
                {
                    All = Filters,
                    Any = Filters,
                    None = Filters
                }
            },
            @"Multiple\Combining\Every.json"
        },
        new object[]
        {
            new SearchQuery
            {
                Query = "test",
                Filters = new AnyFilter
                {
                    Value = new IFilter[]
                    {
                        new Core.Models.Api.Search.Query.Filters.Filters
                        {
                            All = Filters,
                            Any = Filters,
                            None = Filters
                        }
                    }
                }
            },
            @"Multiple\Combining\Nested.json"
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
        return $@"{base.GetJsonPath()}\Search\Query\Filters";
    }
}
