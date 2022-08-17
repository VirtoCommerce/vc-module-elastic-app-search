using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Sorting
{
    public record GeoDistanceSortValue
    {
        [JsonRequired]
        public GeoPoint Center { get; init; }

        [JsonRequired]
        public SortOrder Order { get;set; }
    }
}
