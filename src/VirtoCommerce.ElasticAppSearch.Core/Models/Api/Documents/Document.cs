using Newtonsoft.Json;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Json;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Documents;

[JsonConverter(typeof(DocumentConverter<Document, object>))]
public record Document: DocumentBase<object>
{
}
