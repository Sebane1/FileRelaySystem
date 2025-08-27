using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RelayCommonData;
using RelayServerProtocol.Database;

namespace RelayServerProtocol.Managers
{
    public class SecurityManager
    {
        private static SecurityManager _instance;

        public static SecurityManager Instance { get => _instance; set => _instance = value; }
        IDataManager _dataManager;

        public SecurityManager()
        {
            _dataManager = ConfigureData();
            _instance = this;
        }

        DataConfig LoadDataConfig()
        {
            var dataConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dataConfig.json");
            if (File.Exists(dataConfigPath))
            {
                return JsonConvert.DeserializeObject<DataConfig>(File.ReadAllText(dataConfigPath));
            }

            return new DataConfig();
        }

        public IDataManager ConfigureData()
        {
            var dataStorageType = LoadDataConfig();

            switch (dataStorageType.DataStorageType)
            {
                case DataStorageType.Json:
                    return new JsonDataManager();

                case DataStorageType.EntityFramework:
                    var optionsBuilder = new DbContextOptionsBuilder<ServerDatabasContext>();

                    // Pick based on config
                    var provider = dataStorageType.DatabaseProviderType;
                    var connectionString = dataStorageType.ConnectionString;


                    switch (provider)
                    {
                        case DatabaseProviderType.SqLite:
                            connectionString = $"Data Source={Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mainConfig.db")}";
                            optionsBuilder.UseSqlite(connectionString);
                            break;
                        case DatabaseProviderType.SqlServer:
                            optionsBuilder.UseSqlServer(connectionString);
                            break;
                        case DatabaseProviderType.Postgres:
                            optionsBuilder.UseNpgsql(connectionString);
                            break;
                        case DatabaseProviderType.MySql:
                            optionsBuilder.UseMySql(connectionString, new MySqlServerVersion(
                            new Version(dataStorageType.MajorMySql, dataStorageType.MinorMySql, dataStorageType.PatchMySql)));
                            break;

                    }

                    var db = new ServerDatabasContext(optionsBuilder.Options);
                    return new EntityFrameworkDataManager(db);
            }

            return null;
        }


        public KeyValuePair<bool, ServerRole> Authenticate(string sessionId, string authenticationToken)
        {
            var authenticationHash = Hashing.SHA512Hash(authenticationToken);
            var masterKeyAuthenticationHash = Hashing.SHA512Hash(authenticationToken + _dataManager.GetMasterKeySalt);
            var authenticationSuccess = false;
            if (masterKeyAuthenticationHash == _dataManager.GetMasterKeyHash())
            {
                _dataManager.CheckOrCreateSessionUser(sessionId);
                authenticationSuccess = true;
            }
            else if (_dataManager.IsValidUnclaimedAuthenticationKeyHash(authenticationHash))
            {
                _dataManager.CheckOrCreateSessionUser(sessionId);
                _dataManager.RemoveUnclaimedAuthenticationKeyHash(authenticationHash);
            }

            var persistedData = _dataManager.GetPersistedData(sessionId);
            if (!authenticationSuccess)
            {
                if (_dataManager.IsValidUnclaimedAuthenticationKeyHash(sessionId))
                {
                    authenticationSuccess = persistedData.HashedAccessKey == authenticationHash;
                }
                else
                {
                    authenticationSuccess = false;
                }
            }
            return new KeyValuePair<bool, ServerRole>(authenticationSuccess, persistedData.ServerRole);
        }


        public string GenerateUnclaimedAccessToken()
        {
            return _dataManager.CreateNewUnclaimedAccessToken();
        }

        public bool BanSessionId(string sessionId, string sessionIdToBan)
        {
            var userIssuingBan = _dataManager.GetPersistedData(sessionId);
            if (userIssuingBan != null)
            {
                // Must have a server role.
                if (userIssuingBan.ServerRole != ServerRole.None)
                {
                    var userToBan = _dataManager.GetPersistedData(sessionIdToBan);
                    if (userToBan != null)
                    {
                        // Must be a higher rank than the user being banned.
                        if (userIssuingBan.ServerRole > userIssuingBan.ServerRole)
                        {
                            _dataManager.BanUser(userToBan.SessionId);
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}
