using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Models
{
    public class SearchResults
    {
        public List<(float Score, Email Email)> Results { get; set; }
    }
}
