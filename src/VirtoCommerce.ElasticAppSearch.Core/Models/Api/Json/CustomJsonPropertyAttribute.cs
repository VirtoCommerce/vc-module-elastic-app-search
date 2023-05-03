using System;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Json;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Parameter)]
public class CustomJsonPropertyAttribute : Attribute
{
    public EmptyValueHandling EmptyValueHandling { get; set; }
}
