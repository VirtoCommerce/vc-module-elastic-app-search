using System.Threading.Tasks;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Documents;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Engines;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Schema;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Result;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Suggestions;

namespace VirtoCommerce.ElasticAppSearch.Core.Services;

public interface IElasticAppSearchApiClient
{
    Task<bool> GetEngineExistsAsync(string name);

    Task<Engine> GetEngineAsync(string name);

    Task<Engine> CreateEngineAsync(string name, string language, string[] sourceEngines = null);

    Task<DeleteEngineResult> DeleteEngineAsync(string engineName);

    Task<Engine> AddSourceEnginesAsync(string engineName, string[] sourceEngines);

    Task<Engine> DeleteSourceEnginesAsync(string engineName, string[] sourceEngines);

    Task<CreateOrUpdateDocumentResult[]> CreateOrUpdateDocumentsAsync(string engineName, Document[] documents);

    Task<DeleteDocumentResult[]> DeleteDocumentsAsync(string engineName, string[] ids);

    Task<Schema> GetSchemaAsync(string engineName);

    Task<Schema> UpdateSchemaAsync(string engineName, Schema schema);

    Task<SearchResult> SearchAsync(string engineName, SearchQuery query);

    Task<SearchResult> SearchAsync(string engineName, string rawQuery);

    Task<SearchExplainResult> SearchExplainAsync(string engineName, SearchQuery query);

    Task<SearchExplainResult> SearchExplainAsync(string engineName, string rawQuery);

    Task<SearchSettings> GetSearchSettingsAsync(string engineName);

    Task<SuggestionApiResponse> GetSuggestionsAsync(string engineName, SuggestionApiQuery query);
}
