namespace VirtoCommerce.ElasticAppSearch.Core.Models
{
    public class BoostPreset
    {
        /// <summary>
        /// Preset name: high, medium, normal, low
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Elastic App Search API boost type: value, functional, proximity or recency
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// type of boost operation if boost type is Value or Functional: multiply or add.
        /// </summary>
        public string Operation { get; set; }

        /// <summary>
        /// Boost factor. Must be between 0 and 10. Defaults to 1.0;
        /// </summary>
        public double Factor { get; set; } = 1.0;

        /// <summary>
        /// Indicates default preset. Default will be applied if no suitable boost preset found.
        /// </summary>
        public bool IsDefault { get; set; }
    }
}
