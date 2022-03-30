using System.Collections.Generic;
using System.Linq;

namespace VirtoCommerce.ElasticAppSearch.Core.Extensions
{
    public static class CollectionExtensions
    {
        private static IEnumerable<string> WhereNonEmpty(this IList<string> values)
        {
            return values.Where(x => !string.IsNullOrEmpty(x));
        }
    }
}
