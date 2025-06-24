using System;

namespace VirtoCommerce.ElasticAppSearch.Core.Extensions;

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
