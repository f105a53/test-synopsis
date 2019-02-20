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
                        ConnectionString = @"Server=91.100.1.142;Database=DB_SearchEngine;Enlist=False;User ID=mikkel;Password=eerrddff11,,;"
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