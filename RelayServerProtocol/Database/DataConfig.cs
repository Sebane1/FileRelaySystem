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
        private int _majorMySql;
        private int _minorMySql;
        private int _patchMySql;
        public DataStorageType DataStorageType { get => _dataStorageType; set => _dataStorageType = value; }
        public DatabaseProviderType DatabaseProviderType { get => _databaseProviderType; set => _databaseProviderType = value; }
        public string ConnectionString { get => _connectionString; set => _connectionString = value; }
        public int MajorMySql { get => _majorMySql; set => _majorMySql = value; }
        public int MinorMySql { get => _minorMySql; set => _minorMySql = value; }
        public int PatchMySql { get => _patchMySql; set => _patchMySql = value; }
    }
}
