using System;

namespace VirtoCommerce.ElasticAppSearch.Data.Models;

public record Result
{
    public string[] Errors { get; set; }

    public override string ToString()
    {
        return string.Join(Environment.NewLine, Errors);
    }
}
