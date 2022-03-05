using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Documents;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Schema;
using VirtoCommerce.SearchModule.Core.Model;

namespace VirtoCommerce.ElasticAppSearch.Core.Services.Converters;

public interface IDocumentConverter
{
    (Document, Schema) ToProviderDocument(IndexDocument indexDocument);

    SearchDocument ToSearchDocument(Models.Api.Search.Result.Document searchResultDocument);
}
