using System.Collections.Generic;
using System.Linq;
using LinqToDB.Configuration;

namespace Common.Data
{
    public class LinqToDbSettings : ILinqToDBSettings
    {
        public IEnumerable<IConnectionStringSettings> ConnectionStrings
        {
            get
            {
                yield return
                    new ConnectionStringSettings
                    {
                        Name = "DB_SearchEngine",
                        ProviderName = "SqlServer",
                        ConnectionString = @"Server=tcp:dls-case-search.database.windows.net,1433;Initial Catalog=DB_SearchEngine;Persist Security Info=False;User ID=hinz3;Password=public159753!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
                    };
            }
        }

        public IEnumerable<IDataProviderSettings> DataProviders => Enumerable.Empty<IDataProviderSettings>();

        public string DefaultConfiguration => "SqlServer";
        public string DefaultDataProvider => "SqlServer";
    }

    public class ConnectionStringSettings : IConnectionStringSettings
    {
        public string ConnectionString { get; set; }
        public bool IsGlobal => false;
        public string Name { get; set; }
        public string ProviderName { get; set; }
    }
}