using SearchService.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SearchService.Core.Interfaces
{
    public interface ISearchService
    {
        SearchResults<Email> GetSearchResults(SearchRequest request);
    }
}
