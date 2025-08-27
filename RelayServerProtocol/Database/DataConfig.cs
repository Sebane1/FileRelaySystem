using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelayServerProtocol.Database
{
    public class DataConfig
    {
        private DataStorageType _dataStorageType;
        private DatabaseProviderType _databaseProviderType;
        private string _connectionString;
        private string _databaseHost;
        private string _databaseUser;
        private string _databasePassword;

        public DataStorageType DataStorageType { get => _dataStorageType; set => _dataStorageType = value; }
        public DatabaseProviderType DatabaseProviderType { get => _databaseProviderType; set => _databaseProviderType = value; }
        public string ConnectionString { get => _connectionString; set => _connectionString = value; }
        public string DatabaseHost { get => _databaseHost; set => _databaseHost = value; }
        public string DatabasePassword { get => _databasePassword; set => _databasePassword = value; }
        public string DatabaseUser { get => _databaseUser; set => _databaseUser = value; }
    }
}
