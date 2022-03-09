using System.Collections.Generic;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Documents;
using Xunit;

namespace VirtoCommerce.ElasticAppSearch.Tests.Api;

public class DocumentSerializationTests: SerializationTestsBase
{
    public static IEnumerable<object[]> SerializationData => new[]
    {
        new object[] { new Documents(), "Empty.json" },
        new object[] { new Documents { new() { Id = "test" } }, "Id.json" },
        new object[]
        {
            new Documents
            {
                new()
                {
                    Id = "test",
                    Fields = new Dictionary<string, object>
                    {
                        { "text", "sample" },
                        { "number", 1 },
                        { "bool", true }
                    }
                }
            }, "Fields.json"
        },
    };

    [Theory]
    [MemberData(nameof(SerializationData))]
    public override void Entity_Serialize_Correct<T>(T actual, string expectedJsonFileName)
    {
        base.Entity_Serialize_Correct(actual, expectedJsonFileName);
    }

    //[Theory]
    //[MemberData(nameof(SerializationData))]
    //public override void Json_Deserialize_Correct<T>(T expected, string actualJsonFileName)
    //{
    //    base.Json_Deserialize_Correct(expected, actualJsonFileName);
    //}

    protected override string GetJsonPath()
    {
        return $"{base.GetJsonPath()}\\Documents";
    }
}
