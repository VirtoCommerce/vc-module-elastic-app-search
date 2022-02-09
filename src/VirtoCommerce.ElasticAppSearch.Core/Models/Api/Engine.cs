namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api;

public record Engine
{
    public string Name { get; set; }
    
    public EngineType Type { get; set; }
    
    public string Language { get; set; }
}
