using System;
using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.ElasticAppSearch.Core;

public static class ModuleConstants
{
    public const string ModuleName = "ElasticAppSearch";

    public static class ElasticSearchApi
    {
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
