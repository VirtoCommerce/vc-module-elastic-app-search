using System.Collections.Generic;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Schema;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Boosts;
using VirtoCommerce.SearchModule.Core.Model;

namespace VirtoCommerce.ElasticAppSearch.Core.Services.Builders
{
    public interface ISearchBoostsBuilder
    {
        Dictionary<string, Boost[]> ToBoosts(IList<SearchBoost> boosts, Schema schema);
    }
}
