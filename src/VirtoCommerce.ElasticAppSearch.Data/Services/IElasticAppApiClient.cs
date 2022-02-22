using System.Threading.Tasks;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Documents;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Engines;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Schema;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search;

namespace VirtoCommerce.ElasticAppSearch.Data.Services;

public interface IElasticAppApiClient
{
    Task<bool> GetEngineExistsAsync(string name);
    Task<Engine> CreateEngineAsync(string name, string language);
    Task<CreateOrUpdateDocumentResult[]> CreateOrUpdateDocumentsAsync<T>(string engineName, T[] documents);
    Task<DeleteDocumentResult[]> DeleteDocumentsAsync(string engineName, string[] ids);
    Task<Schema> UpdateSchemaAsync(string engineName, Schema schema);
    Task<SearchResult> SearchAsync(string engineName, SearchQuery query);
    Task<SearchResult> SearchAsync(string engineName, string rawQuery);
}
