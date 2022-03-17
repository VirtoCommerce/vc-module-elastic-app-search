using System.Collections;
using System.Linq;

namespace VirtoCommerce.ElasticAppSearch.Core.Extensions;

public static class ObjectExtensions
{
    public static object[] AsArray(this object value)
    {
        return value as object[] ?? (value as IEnumerable)?.Cast<object>().ToArray();
    }
}
