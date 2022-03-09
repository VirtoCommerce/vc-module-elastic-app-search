using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Json;
using Xunit;

namespace VirtoCommerce.ElasticAppSearch.Tests.Api;

public class SerializationTests: SerializationTestsBase
{
    public static readonly IEnumerable<object[]> WriteJsonData = new List<object[]>
    {
        new object[] { new NoChanges { Property = null }, "PropertyNull.json" },
        new object[] { new NoChanges { Property = Array.Empty<string>() }, "PropertyEmpty.json" },
        new object[] { new IgnoreNullValue { Property = null }, "Empty.json" },
        new object[] { new IgnoreEmptyValue { Property = Array.Empty<string>() }, "Empty.json" },
        new object[] { new IgnoreEmptyValue { Property = new[] { string.Empty } }, "PropertyEmpty.json" },
        new object[] { new SingleValueAsObject { Property = new[] { "test" } }, "PropertyValue.json" },
        new object[] { new SingleValueAsObject { Property = new[] { "test1", "test2" } }, "PropertyArray.json" }
    };

    [Theory]
    [MemberData(nameof(WriteJsonData))]
    public override void Serialize_Entity_CorrectlySerializes<T>(T actual, string expectedJsonFileName)
    {
        base.Serialize_Entity_CorrectlySerializes(actual, expectedJsonFileName);
    }

    private class NoChanges
    {
        [JsonConverter(typeof(ArrayConverter))]
        public string[] Property { get; set; }
    }

    private class IgnoreNullValue
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(ArrayConverter))]
        public string[] Property { get; set; }
    }

    private class IgnoreEmptyValue
    {
        [CustomJsonProperty(EmptyValueHandling = EmptyValueHandling.Ignore)]
        public string[] Property { get; set; }
    }

    private class SingleValueAsObject
    {
        [JsonConverter(typeof(ArrayConverter), SingleValueHandling.AsObject)]
        public string[] Property { get; set; }
    }
}
