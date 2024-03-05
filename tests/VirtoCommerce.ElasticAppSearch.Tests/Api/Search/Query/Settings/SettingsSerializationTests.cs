using System.Collections.Generic;
using System.IO;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Boosts;
using Xunit;

namespace VirtoCommerce.ElasticAppSearch.Tests.Api.Search.Query.Boosts
{
    public class SettingsSerializationTests : SerializationTestsBase
    {
        public static IEnumerable<object[]> SerializationData => new[]
        {
            new object[]
            {
                //query
                new SearchSettings
                {
                    Precision = 2,
                    PrecisionEnabled = true,
                    Boosts = new Dictionary<string, Boost[]>
                    {
                        { "brand", new Boost[]
                                    {
                                        new ValueBoost
                                        {
                                            Value = ["Apple", "Samsung"],
                                            Factor = 1,
                                        }
                                    }
                        },
                    },
                },
                //json
                "Settings.json"
            },
        };

        [Theory]
        [MemberData(nameof(SerializationData))]
        public override void Serialize_Entity_CorrectlySerializes<T>(T actual, string expectedJsonFileName)
        {
            base.Serialize_Entity_CorrectlySerializes(actual, expectedJsonFileName);
        }

        [Theory]
        [MemberData(nameof(SerializationData))]
        public override void Deserialize_Json_CorrectlyDeserializes<T>(T expected, string actualJsonFileName)
        {
            base.Deserialize_Json_CorrectlyDeserializes(expected, actualJsonFileName);
        }

        protected override string GetJsonPath()
        {
            return Path.Combine(base.GetJsonPath(), "Search", "Query", "Settings");
        }
    }
}
