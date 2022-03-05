using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using VirtoCommerce.ElasticAppSearch.Core;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Json;
using Xunit;

namespace VirtoCommerce.ElasticAppSearch.Tests;

public class JsonTests
{
    public static readonly IEnumerable<object[]> WriteJsonData = new List<object[]>
    {
        new object[] { new NoChanges { Property = null }, "{\"property\":null}" },
        new object[] { new NoChanges { Property = Array.Empty<string>() }, "{\"property\":[]}" },
        new object[] { new IgnoreNullValue { Property = null }, "{}" },
        new object[] { new IgnoreEmptyValue { Property = Array.Empty<string>() }, "{}" },
        new object[] { new IgnoreEmptyValue { Property = new[] { string.Empty } }, "{\"property\":[\"\"]}" },
        new object[] { new SingleValueAsObject { Property = new[] { "test" } }, "{\"property\":\"test\"}" },
        new object[] { new SingleValueAsObject { Property = new[] { "test1", "test2" } }, "{\"property\":[\"test1\",\"test2\"]}" }
    };

    [Theory]
    [MemberData(nameof(WriteJsonData))]
    public void WriteJson_CustomValuesHandling_WriteCorrectValue(object objectToSerialize, string expectedJson)
    {
        // Act
        var actualJson = JsonConvert.SerializeObject(objectToSerialize, ModuleConstants.Api.JsonSerializerSettings);

        // Assert
        Assert.Equal(expectedJson, actualJson);
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
