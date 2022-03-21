using System.Collections.Generic;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Facets
{
    public class Facets : Dictionary<string, Facet>
    {
        public Facets()
        {
        }

        public Facets(IDictionary<string, Facet> dictionary) : base(dictionary)
        {
        }
    }
}
