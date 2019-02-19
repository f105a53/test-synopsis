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
                        Name = "SqlServer",
                        ProviderName = "SqlServer",
                        ConnectionString = @"Server=.\;Database=DB_SearchEngine;Trusted_Connection=True;Enlist=False;"
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