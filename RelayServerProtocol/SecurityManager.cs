using Newtonsoft.Json;
using RelayCommonData;
using RelayServerProtocol;

namespace FileRelaySystem
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

        DataStorageType LoadDataConfig()
        {
            string dataConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dataConfig.json");
            if (File.Exists(dataConfigPath))
            {
                return JsonConvert.DeserializeObject<DataConfig>(File.ReadAllText(dataConfigPath)).DataStorageType;
            }

            return DataStorageType.Json;
        }

        public IDataManager ConfigureData()
        {
            var dataStorageType = LoadDataConfig();
            switch (dataStorageType)
            {
                case DataStorageType.Json:
                    return new JsonDataManager();
                case DataStorageType.SqLite:
                    return new SqLiteDataManager();
                case DataStorageType.PostgressSql:
                    return new PostgressSqlDataManager();
                case DataStorageType.MySql:
                    return new MySqlDataManager();
                case DataStorageType.MsSql:
                    return new MsSqlDataManager();
            }
            return null;
        }

        public KeyValuePair<bool, ServerRole> Authenticate(string sessionId, string authenticationToken)
        {
            string authenticationHash = Hashing.SHA512Hash(authenticationToken);
            string masterKeyAuthenticationHash = Hashing.SHA512Hash(authenticationToken + _dataManager.GetMasterKeySalt);
            bool authenticationSuccess = false;
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
