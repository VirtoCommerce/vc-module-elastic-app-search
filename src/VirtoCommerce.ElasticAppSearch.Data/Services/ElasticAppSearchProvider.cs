using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Core.Services;

namespace VirtoCommerce.ElasticAppSearch.Data.Services
{
    public class ElasticAppSearchProvider: ISearchProvider
    {
        public async Task DeleteIndexAsync(string documentType)
        {
            await Task.CompletedTask;
        }

        public async Task<IndexingResult> IndexAsync(string documentType, IList<IndexDocument> documents)
        {
            return new IndexingResult();
        }

        public async Task<IndexingResult> RemoveAsync(string documentType, IList<IndexDocument> documents)
        {
            return new IndexingResult();
        }

        public async Task<SearchResponse> SearchAsync(string documentType, SearchRequest request)
        {
            return new SearchResponse();
        }
    }
}
