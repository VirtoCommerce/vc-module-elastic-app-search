using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Filters;


namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Facets;

public class FacetRequest
{
    public string Id { get; set; }

    public string FieldName { get; set; }

    public string FacetFieldName { get; set; }

    public Facet Facet { get; set; }

    public IFilters Filter { get; set; }

    public string FilterName { get; set; }
}
