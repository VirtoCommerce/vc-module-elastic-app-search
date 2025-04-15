using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Curations;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Documents;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Engines;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Schema;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Result;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Suggestions;
using Document = VirtoCommerce.ElasticAppSearch.Core.Models.Api.Documents.Document;

namespace VirtoCommerce.ElasticAppSearch.Core.Services;

public interface IElasticAppSearchApiClient
{
    Task<bool> GetEngineExistsAsync(string name, CancellationToken cancellationToken = default);

    Task<Engine> GetEngineAsync(string name, CancellationToken cancellationToken = default);

    Task<Engine> CreateEngineAsync(string name, string language, string[] sourceEngines = null, CancellationToken cancellationToken = default);

    Task<DeleteEngineResult> DeleteEngineAsync(string engineName, CancellationToken cancellationToken = default);

    Task<Engine> AddSourceEnginesAsync(string engineName, string[] sourceEngines, CancellationToken cancellationToken = default);

    Task<Engine> DeleteSourceEnginesAsync(string engineName, string[] sourceEngines, CancellationToken cancellationToken = default);

    Task<CreateOrUpdateDocumentResult[]> CreateOrUpdateDocumentsAsync(string engineName, Document[] documents, CancellationToken cancellationToken = default);

    Task<DeleteDocumentResult[]> DeleteDocumentsAsync(string engineName, string[] ids, CancellationToken cancellationToken = default);

    Task<Schema> GetSchemaAsync(string engineName, CancellationToken cancellationToken = default);

    Task<Schema> UpdateSchemaAsync(string engineName, Schema schema, CancellationToken cancellationToken = default);

    Task<SearchResult> SearchAsync(string engineName, SearchQuery query, CancellationToken cancellationToken = default);

    Task<SearchResult> SearchAsync(string engineName, string rawQuery, CancellationToken cancellationToken = default);

    Task<SearchExplainResult> SearchExplainAsync(string engineName, SearchQuery query, CancellationToken cancellationToken = default);

    Task<SearchExplainResult> SearchExplainAsync(string engineName, string rawQuery, CancellationToken cancellationToken = default);

    Task<SearchSettings> GetSearchSettingsAsync(string engineName, CancellationToken cancellationToken = default);

    Task<SuggestionApiResponse> GetSuggestionsAsync(string engineName, SuggestionApiQuery query, CancellationToken cancellationToken = default);

    Task<CurationSearchResult> GetCurationsAsync(string engineName, int skip, int take, CancellationToken cancellationToken = default);

    Task<Curation> GetCurationAsync(string engineName, string curationId, bool skipAnalytics = true, CancellationToken cancellationToken = default);
}
