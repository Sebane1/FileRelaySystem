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

        public DataStorageType DataStorageType { get => _dataStorageType; set => _dataStorageType = value; }
        public DatabaseProviderType DatabaseProviderType { get => _databaseProviderType; set => _databaseProviderType = value; }
        public string ConnectionString { get => _connectionString; set => _connectionString = value; }
    }
}
