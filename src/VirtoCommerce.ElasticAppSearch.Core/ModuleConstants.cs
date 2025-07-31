using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Json;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.ElasticAppSearch.Core;

public static class ModuleConstants
{
    public const string ModuleName = "ElasticAppSearch";

    /// <summary>
    /// By default, Elastic App Search returns 10 facet values for each facet. The maximum number of facet values is 250.
    /// You can change the number of facet values returned by app_search.engine.total_facet_values_returned.limit.
    /// </summary>
    public const int MaxFacetValues = 250;

    public static class Api
    {
        public static readonly JsonSerializerSettings JsonSerializerSettings = new()
        {
            // Elastic App Search API use camelCase in JSON
            ContractResolver = new CustomContractResolver { NamingStrategy = new SnakeCaseNamingStrategy() },
            Converters = new List<JsonConverter>
            {
                new StringEnumConverter(new CamelCaseNamingStrategy()),
                new SearchModuleCoreGeoPointConverter(),
            },

            // Elastic App Search API doesn't support fraction in seconds (probably bug in their ISO 8160 / RFC3399 specification support)
            DateFormatString = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffzzz",
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
        };

        public static class Languages
        {
            public const string BrazilianPortuguese = "pt-br";
            public const string Chinese = "zh";
            public const string Danish = "da";
            public const string Dutch = "nl";
            public const string English = "en";
            public const string French = "fr";
            public const string German = "de";
            public const string Italian = "it";
            public const string Japanese = "ja";
            public const string Korean = "ko";
            public const string Portuguese = "pt";
            public const string Russian = "ru";
            public const string Spanish = "es";
            public const string Thai = "th";
            public const string Universal = null;
        }

        public static class FieldNames
        {
            public const string Id = "id";

            public const string ObjectFieldName = "__object";

            public const string ScoreFieldName = "_score";

            public static readonly string[] Reserved = { "external_id", "engine_id", "highlight", "or", "and", "not", "any", "all", "none" };

            public const string ReservedFieldNamesPrefix = "field_";

            public const string PrivateFieldPrefix = "privatefield_";

            public static readonly ReadOnlyDictionary<string, string> Replacements = new(new Dictionary<string, string>
            {
                { "-", "_hyphen_" }
            });

            public static readonly string[] IgnoredForSearch = { "__content" };

            public const int MaximumLength = 64;
        }

        public static class Search
        {
            public const int MaxDepth = 5;
        }

        public static class BoostTypes
        {
            public const string Value = "value";
            public const string Functional = "functional";
            public const string Proximity = "proximity";
            public const string Recency = "proximity";
        }

        public static class BoostOperations
        {
            public const string Multiply = "multiply";
            public const string Add = "add";
        }

        public static class BoostFunctions
        {
            public const string Linear = "linear";
            public const string Exponential = "exponential";
            public const string Logarithmic = "logarithmic";
            public const string Gaussian = "gaussian";
        }
    }

    public static class Security
    {
        public static class Permissions
        {
            public const string Access = "elasticappsearch:access";
            public const string Diagnostic = "elasticappsearch:diagnostic";

            public static string[] AllPermissions { get; } = { Access, Diagnostic };

        }
    }

    public static class Settings
    {
        public static class General
        {
            public static SettingDescriptor MaxFacetValues { get; } = new SettingDescriptor
            {
                Name = "ElasticAppSearch.MaxFacetValues",
                GroupName = "ElasticAppSearch|General",
                ValueType = SettingValueType.PositiveInteger,
                DefaultValue = MaxFacetValues,
            };
        }

        public static IEnumerable<SettingDescriptor> AllSettings => [General.MaxFacetValues];
    }
}
