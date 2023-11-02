using System.Collections.Generic;
using System.IO;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Boosts;
using Xunit;

namespace VirtoCommerce.ElasticAppSearch.Tests.Api.Search.Query.Boosts
{
    public class ValueBoostsSerializationTests : SerializationTestsBase
    {
        public static IEnumerable<object[]> SerializationData => new[]
        {
            new object[]
            {
                //query
                new SearchQuery
                {
                    Query = "park",
                    Boosts = new Dictionary<string, Boost[]>
                    {
                        { "state", new Boost[]
                                    {
                                        new ValueBoost
                                        {
                                            Value = "TX",
                                            Operation = "multiply",
                                            Factor = 10.0,
                                        },
                                        new ValueBoost
                                        {
                                            Value = "MN",
                                            Operation = "add",
                                            Factor = 5.0,
                                        }
                                    }
                        },
                    },
                },
                //json
                "Value.json"
            },
        };

        [Theory]
        [MemberData(nameof(SerializationData))]
        public override void Serialize_Entity_CorrectlySerializes<T>(T actual, string expectedJsonFileName)
        {
            base.Serialize_Entity_CorrectlySerializes(actual, expectedJsonFileName);
        }

        protected override string GetJsonPath()
        {
            return Path.Combine(base.GetJsonPath(), "Search", "Query", "Boosts");
        }
    }
}
