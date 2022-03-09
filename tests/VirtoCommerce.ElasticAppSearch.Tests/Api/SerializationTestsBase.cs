using Newtonsoft.Json;
using VirtoCommerce.ElasticAppSearch.Core;
using VirtoCommerce.ElasticAppSearch.Tests.Extensions;
using Xunit;

namespace VirtoCommerce.ElasticAppSearch.Tests.Api;

public class SerializationTestsBase
{
    public virtual void Entity_Serialize_Correct<T>(T actual, string expectedJsonFileName)
    {
        // Arrange
        var expectedJson = JsonHelper.LoadFrom(GetJsonPath(expectedJsonFileName)).ReplaceLineEndings(string.Empty).ReplaceWhitespaces(string.Empty);

        // Act
        var actualJson = JsonConvert.SerializeObject(actual, ModuleConstants.Api.JsonSerializerSettings);

        // Assert
        Assert.Equal(expectedJson, actualJson, ignoreLineEndingDifferences: true, ignoreWhiteSpaceDifferences: true);
    }
    
    public virtual void Json_Deserialize_Correct<T>(T expected, string actualJsonFileName)
    {
        // Arrange
        var actualJson = JsonHelper.LoadFrom(GetJsonPath(actualJsonFileName));

        // Act
        var actual = JsonConvert.DeserializeObject<T>(actualJson, ModuleConstants.Api.JsonSerializerSettings);

        // Assert
        Assert.Equal(expected, actual);
    }

    protected virtual string GetJsonPath()
    {
        return @"Api\Json";
    }

    protected virtual string GetJsonPath(string fileName)
    {
        return $"{GetJsonPath()}\\{fileName}";
    }
}
