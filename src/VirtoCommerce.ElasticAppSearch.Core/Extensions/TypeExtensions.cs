using System;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.ElasticAppSearch.Core.Extensions;

public static class OverridenType<T> where T : new()
{
    //TODO: Move to AbstractTypeFactory<T> when it will be implemented
    public static T New() => AbstractTypeFactory<T>.HasOverrides ? AbstractTypeFactory<T>.TryCreateInstance() : new T();
}

public static class TypeExtensions
{
    public static Type GetEnumerableElementType(this Type type)
    {
        if (type.HasElementType)
        {
            return type.GetElementType();
        }

        if (type.IsGenericType)
        {
            return type.GenericTypeArguments[0];
        }

        return typeof(object);
    }
}
