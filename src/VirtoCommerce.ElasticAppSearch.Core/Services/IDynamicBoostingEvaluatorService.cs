using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Boosts;
using VirtoCommerce.SearchModule.Core.Model;

namespace VirtoCommerce.ElasticAppSearch.Core.Services;

public interface IDynamicBoostingEvaluatorService
{
    Task<Dictionary<string, Boost[]>> EvaluateAsync(SearchRequest searchRequest);
}
