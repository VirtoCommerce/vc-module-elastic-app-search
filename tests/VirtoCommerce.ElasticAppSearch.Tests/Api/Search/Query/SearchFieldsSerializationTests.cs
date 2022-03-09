using System.Collections.Generic;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.SearchFields;
using Xunit;

namespace VirtoCommerce.ElasticAppSearch.Tests.Api.Search.Query;

public class SearchFieldsSerializationTests: SerializationTestsBase
{
    public static IEnumerable<object[]> SerializationData => new[]
    {
        new object[]
        {
            new SearchQuery
            {
                Query = "test",
                SearchFields = new SearchFields()
            },
            @"..\Default.json"
        },
        new object[]
        {
            new SearchQuery
            {
                Query = "test",
                SearchFields = new SearchFields(new Dictionary<string, SearchFieldValue>
                {
                    {
                        "test",
                        new SearchFieldValue()
                    }
                })
            },
            "WithoutWeight.json"
        },
        new object[]
        {
            new SearchQuery
            {
                Query = "test",
                SearchFields = new SearchFields(new Dictionary<string, SearchFieldValue>
                {
                    {
                        "test",
                        new SearchFieldValue { Weight = 1 }
                    }
                })
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
    

    [Fact(Skip = "Need to find the way to prevent pass null into the SearchFields")]
    public void Serialize_NullSearchFieldValue_ThrowsException()
    {
        Serialize_InvalidData_ThrowsException(new SearchQuery
        {
            Query = "test",
            SearchFields = new SearchFields(new Dictionary<string, SearchFieldValue>
            {
                { "test", null }
            })
        });
    }

    protected override string GetJsonPath()
    {
        return $@"{base.GetJsonPath()}\Search\Query\SearchFields";
    }
}
