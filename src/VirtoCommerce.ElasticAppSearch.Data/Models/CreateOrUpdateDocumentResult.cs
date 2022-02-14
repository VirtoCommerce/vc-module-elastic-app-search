using System;
using System.Linq;

namespace VirtoCommerce.ElasticAppSearch.Data.Models;

public record CreateOrUpdateDocumentResult
{
    public string Id { get; set; }

    public string[] Errors { get; set; }

    public override string ToString()
    {
        return $"The following document {Id} has errors: {Environment.NewLine}{string.Join(Environment.NewLine, Errors.Select((error, index) => $"{index}. {error}"))}";
    }
}
