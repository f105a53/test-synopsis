using System.Collections.Generic;
using System.Linq;
using Lucene.Net.Search;

namespace Server.Services
{
    public class SearchService
    {
        private readonly Common.Index index;

        public SearchService(Common.Index index)
        {
            this.index = index;
        }

        public TopDocs  GetResults(string term)
        {
            return index.Search(term);    
        }
    }
}