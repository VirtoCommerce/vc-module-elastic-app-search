using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Schema;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Filters;
using IFilter = VirtoCommerce.SearchModule.Core.Model.IFilter;

namespace VirtoCommerce.ElasticAppSearch.Core.Services.Builders;

public interface ISearchFiltersBuilder
{
    IFilters ToFilters(IFilter filter, Schema schema);
}
