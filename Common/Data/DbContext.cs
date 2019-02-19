using LinqToDB;
using LinqToDB.Data;

namespace Common.Data
{
    public class DbContext : DataConnection

    {
        public DbContext() : base("SqlServer")
        {
        }

        public ITable<Document> Document => GetTable<Document>();
        public ITable<Term> Term => GetTable<Term>();
        public ITable<TermDoc> TermDoc => GetTable<TermDoc>();
    }
}