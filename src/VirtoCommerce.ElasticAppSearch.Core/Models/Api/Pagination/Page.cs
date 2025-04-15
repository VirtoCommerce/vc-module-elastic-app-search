namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Pagination;

public class Page
{
    public int Current { get; set; }

    public int TotalPages { get; set; }

    public int TotalResults { get; set; }

    public int Size { get; set; }
}
