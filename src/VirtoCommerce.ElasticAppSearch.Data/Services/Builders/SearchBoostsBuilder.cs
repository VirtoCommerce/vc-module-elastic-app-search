using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using VirtoCommerce.ElasticAppSearch.Core.Models;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Schema;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Boosts;
using VirtoCommerce.ElasticAppSearch.Core.Services.Builders;
using VirtoCommerce.ElasticAppSearch.Core.Services.Converters;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.SearchModule.Core.Model;
using static VirtoCommerce.ElasticAppSearch.Core.ModuleConstants.Api;

namespace VirtoCommerce.ElasticAppSearch.Data.Services.Builders
{
    public class SearchBoostsBuilder : ISearchBoostsBuilder
    {
        private readonly List<BoostPreset> _boostPresets;
        private readonly IFieldNameConverter _fieldNameConverter;

        public SearchBoostsBuilder(
            IOptions<ElasticAppSearchOptions> options,
            IFieldNameConverter fieldNameConverter)
        {
            _boostPresets = options.Value?.BoostPresets ?? new List<BoostPreset>();

            _fieldNameConverter = fieldNameConverter;
        }

        public Dictionary<string, Boost[]> ToBoosts(IList<SearchBoost> boosts, Schema schema)
        {
            if (boosts.IsNullOrEmpty() || _boostPresets.IsNullOrEmpty())
            {
                return new Dictionary<string, Boost[]>();
            }

            var result = boosts
                .GroupBy(x => _fieldNameConverter.ToProviderFieldName(x.FieldName))
                .Where(x => schema.Fields.ContainsKey(x.Key))
                .ToDictionary(x => x.Key, x =>
                {
                    var apiBoosts = new List<Boost>();

                    foreach (var searchBoost in x)
                    {
                        var apiBoost = ToApiBoost(searchBoost);
                        if (apiBoost != null)
                        {
                            apiBoosts.Add(apiBoost);
                        }
                    }

                    return apiBoosts.Any() ? apiBoosts.ToArray() : null;
                });

            return result;
        }

        private Boost ToApiBoost(SearchBoost searchBoost)
        {
            var preset = _boostPresets.FirstOrDefault(x => x.Name.EqualsInvariant(searchBoost.Preset))
                ?? _boostPresets.FirstOrDefault(x => x.IsDefault);

            if (preset == null)
            {
                return null;
            }

            var boost = default(Boost);

            switch (preset.Type)
            {
                case BoostTypes.Value:
                    boost = new ValueBoost
                    {
                        Value = searchBoost.Value,
                        Operation = preset.Operation,
                        Factor = preset.Factor,
                    };

                    break;
            }

            return boost;
        }
    }
}
