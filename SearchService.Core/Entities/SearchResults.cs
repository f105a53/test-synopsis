using System;
using System.Collections.Generic;
using System.Text;

namespace SearchService.Core.Entities
{
    public class SearchResults<T>
    {
        public List<(float Score, T Result)> Results { get; set; }
    }
}
