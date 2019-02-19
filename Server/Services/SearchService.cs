using Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LinqToDB;
using LinqToDB.Data;

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
            if (db.Term.Contains(new Term { Value = term }))
            {
                return db.TermDoc.Where(td => td.TermId == term).ToList();
            }
            else
            {
                return db.TermDoc.Where(td => td.TermId.Contains(term)).ToList();
            }
        }
    }
}
