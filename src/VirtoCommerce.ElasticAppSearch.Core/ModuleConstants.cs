using System;
using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.ElasticAppSearch.Core
{
    public static class ModuleConstants
    {
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
}
