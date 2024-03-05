using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using VirtoCommerce.ElasticAppSearch.Core.Models;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Schema;
using VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search;
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

        public Dictionary<string, Boost[]> ToBoosts(IList<SearchBoost> boosts, Schema schema, SearchSettings settings)
        {
            var presetsBoost = ResolveDynamicBoostingFromPresets(boosts, schema);

            if (presetsBoost.Count == 0)
            {
                return [];
            }

            // Join Dynamic and Static boosts
            var result = new Dictionary<string, Boost[]>();
            return result
                .Concat(settings.Boosts ?? new Dictionary<string, Boost[]>())
                .Concat(presetsBoost)
                .GroupBy(kvp => kvp.Key)
                .ToDictionary(
                    group => group.Key,
                    group => group.SelectMany(kvp => kvp.Value).ToArray()
                );
        }


        protected virtual Dictionary<string, Boost[]> ResolveDynamicBoostingFromPresets(IList<SearchBoost> boosts, Schema schema)
        {
            if (boosts.IsNullOrEmpty() || _boostPresets.IsNullOrEmpty())
            {
                return [];
            }

            return boosts
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
        }

        private Boost ToApiBoost(SearchBoost searchBoost)
        {
            var preset = _boostPresets.FirstOrDefault(x => x.Name.EqualsInvariant(searchBoost.Preset))
                ?? _boostPresets.FirstOrDefault(x => x.IsDefault);

            if (preset == null)
            {
                return null;
            }

            return preset.Type switch
            {
                BoostTypes.Value => new ValueBoost
                {
                    Value = [searchBoost.Value],
                    Operation = preset.Operation,
                    Factor = preset.Factor,
                },
                _ => null
            };
        }
    }
}
