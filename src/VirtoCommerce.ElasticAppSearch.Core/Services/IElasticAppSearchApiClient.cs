using System.Threading.Tasks;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Documents;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Engines;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Schema;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Result;

namespace VirtoCommerce.ElasticAppSearch.Core.Services;

public interface IElasticAppSearchApiClient
{
    Task<bool> GetEngineExistsAsync(string name);

    Task<Engine> CreateEngineAsync(string name, string language);

    Task<CreateOrUpdateDocumentResult[]> CreateOrUpdateDocumentsAsync(string engineName, Documents documents);

    Task<DeleteDocumentResult[]> DeleteDocumentsAsync(string engineName, string[] ids);

    Task<Schema> UpdateSchemaAsync(string engineName, Schema schema);

    Task<SearchResult> SearchAsync(string engineName, SearchQuery query);

    Task<SearchResult> SearchAsync(string engineName, string rawQuery);
}
