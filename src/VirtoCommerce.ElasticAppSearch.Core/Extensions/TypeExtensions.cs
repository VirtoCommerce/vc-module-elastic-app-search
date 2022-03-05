using System;
using System.Linq;

namespace VirtoCommerce.ElasticAppSearch.Core.Extensions;

public static class TypeExtensions
{
    public static Type GetEnumerableElementType(this Type type)
    {
        return type.HasElementType
            ? type.GetElementType()
            : type.IsGenericType
                ? type.GenericTypeArguments.First()
                : typeof(object);
    }
}
