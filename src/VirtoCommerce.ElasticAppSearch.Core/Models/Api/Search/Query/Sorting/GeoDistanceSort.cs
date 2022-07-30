using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace VirtoCommerce.ElasticAppSearch.Core.Models.Api.Search.Query.Sorting
{
    public record GeoDistanceSort : Field<GeoDistanceSortValue>, ISort
    {
    }
}
