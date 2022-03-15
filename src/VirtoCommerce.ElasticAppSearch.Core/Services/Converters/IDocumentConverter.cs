using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Documents;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Schema;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Result;
using VirtoCommerce.SearchModule.Core.Model;

namespace VirtoCommerce.ElasticAppSearch.Core.Services.Converters;

public interface IDocumentConverter
{
    (Document, Schema) ToProviderDocument(IndexDocument indexDocument);

    SearchDocument ToSearchDocument(SearchResultDocument searchResultDocument);
}
