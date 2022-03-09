using System.Collections.Generic;
using System.IO;
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
    public override void Serialize_Entity_CorrectlySerializes<T>(T actual, string expectedJsonFileName)
    {
        base.Serialize_Entity_CorrectlySerializes(actual, expectedJsonFileName);
    }

    [Fact]
    public void Serialize_WithoutRequiredFields_ThrowsException()
    {
        Serialize_InvalidData_ThrowsException(new Documents { new() });
    }

    [Theory]
    [MemberData(nameof(SerializationData))]
    public override void Deserialize_Json_CorrectlyDeserializes<T>(T expected, string actualJsonFileName)
    {
        base.Deserialize_Json_CorrectlyDeserializes(expected, actualJsonFileName);
    }

    protected override string GetJsonPath()
    {
        return Path.Combine(base.GetJsonPath(), "Documents");
    }
}
