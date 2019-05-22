using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Models;

namespace Server.Models
{
    public class Search
    {
        public Search(string[] suggestions, SearchResults<Email> emails)
        {
            Suggestions = suggestions;
            Emails = emails;
        }

        public string[] Suggestions { get; }
        public SearchResults<Email> Emails { get; }

    }
}
