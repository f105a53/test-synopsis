using System.Collections.Generic;
using System.Linq;
using Common.Data;

namespace Server.Services
{
    public class SearchService
    {
        private readonly DbContext db;

        public SearchService(DbContext db)
        {
            this.db = db;
        }

        public List<TermDoc> GetResults(string term)
        {
            lock (db)
            {
                if (db.Term.Contains(new Term {Value = term}))
                    return db.TermDoc.Where(td => td.TermId == term).ToList();
                return db.TermDoc.Where(td => td.TermId.Contains(term)).ToList();
            }
        }
    }
}