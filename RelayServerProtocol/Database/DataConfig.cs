using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelayServerProtocol.Database
{
    public class DataConfig
    {
        DataStorageType _dataStorageType;

        public DataStorageType DataStorageType { get => _dataStorageType; set => _dataStorageType = value; }
    }
}
