using System;
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

    public static class Api
    {
        public static readonly JsonSerializerSettings JsonSerializerSettings = new()
        {
            // Elastic App Search API use camelCase in JSON
            ContractResolver = new CustomContractResolver { NamingStrategy = new SnakeCaseNamingStrategy() },
            Converters = new List<JsonConverter> { new StringEnumConverter(new CamelCaseNamingStrategy()) },

            // Elastic App Search API doesn't support fraction in seconds (probably bug in their ISO 8160 specification support)
            DateFormatString = "yyyy'-'MM'-'dd'T'HH':'mm':'ssK",
            DateTimeZoneHandling = DateTimeZoneHandling.Utc
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

            public static readonly string[] Reserved = { "external_id", "engine_id", "highlight", "or", "and", "not", "any", "all", "none" };

            public const string ReservedFieldNamesPrefix = "field_";

            public const string PrivateFieldPrefix = "privatefield_";

            public static readonly ReadOnlyDictionary<string, string> Replacements = new(new Dictionary<string, string>
            {
                { "-", "_hyphen_" }
            });

            public const int MaximumLength = 64;
        }

        public static class Search
        {
            public const int MaxDepth = 5;
        }
    }

    public static class Security
    {
        public static class Permissions
        {
            public static string[] AllPermissions { get; } = Array.Empty<string>();
        }
    }

    public static class Settings
    {
        public static class Indexing
        {
            private static readonly SettingDescriptor IndexTotalFieldsLimit = new()
            {
                Name = "VirtoCommerce.Search.ElasticAppSearch.IndexTotalFieldsLimit",
                GroupName = "Search|ElasticSearch",
                ValueType = SettingValueType.Integer,
                DefaultValue = 1000
            };

            public static IEnumerable<SettingDescriptor> AllSettings
            {
                get
                {
                    yield return IndexTotalFieldsLimit;
                }
            }
        }

        public static IEnumerable<SettingDescriptor> AllSettings => Indexing.AllSettings;
    }
}
