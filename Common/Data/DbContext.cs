using System;
using System.Collections.Generic;
using System.Text;
using LinqToDB;

namespace Common.Data
{
    public class DbContext : LinqToDB.Data.DataConnection

    {
        public DbContext() : base("Database") { }

        public ITable<Document> Document => GetTable<Document>();
        public ITable<Term> Term => GetTable<Term>();
        public ITable<TermDoc> TermDoc => GetTable<TermDoc>();

    }
}
