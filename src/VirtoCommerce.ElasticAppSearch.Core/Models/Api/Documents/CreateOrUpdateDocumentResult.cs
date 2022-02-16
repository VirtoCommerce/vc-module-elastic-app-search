using System;
using System.Linq;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Documents;

public record CreateOrUpdateDocumentResult
{
    public string Id { get; init; }

    public string[] Errors { get; init; }

    public override string ToString()
    {
        return $"The following document {Id} has errors: {Environment.NewLine}{string.Join(Environment.NewLine, Errors.Select((error, index) => $"{index}. {error}"))}";
    }
}
