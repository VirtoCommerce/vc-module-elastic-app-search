using System;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api;

public record Result
{
    public string[] Errors { get; set; }

    public override string ToString()
    {
        return Errors == null ? nameof(Result) : string.Join(Environment.NewLine, Errors);
    }
}
