using Newtonsoft.Json;
using RelayCommonData;
using RelayUploadProtocol;
using System.Collections.Concurrent;
namespace RelayServerProtocol.Database
{
    public class JsonDataManager : IDataManager
    {
        ServerData _serverData;
        public JsonDataManager()
        {
            LoadData();
        }
        public bool CheckIfPasswordExists()
        {
            return !string.IsNullOrEmpty(_serverData.MasterKeyHash);
        }

        public bool SetMasterPassword(string password)
        {
            if (!string.IsNullOrEmpty(password))
            {
                _serverData.MasterKeySalt = Guid.NewGuid().ToString();
                _serverData.MasterKeyHash = Hashing.SHA512Hash(password + _serverData.MasterKeySalt);
                PersistData();
                return true;
            }
            return false;
        }

        public string CreateNewUnclaimedAccessToken()
        {
            var code = Guid.NewGuid().ToString();
            _serverData.UnclaimedKeyHashes.Add(Hashing.SHA512Hash(code));
            PersistData();
            return code;
        }

        public Structs.AgeGroup GetAgeGroup()
        {
            return _serverData.AgeGroup;
        }

        public string GetMasterKeyHash()
        {
            return _serverData.MasterKeyHash;
        }

        public string GetMasterKeySalt()
        {
            return _serverData.MasterKeySalt;
        }

        public PersistedSessionData GetPersistedData(string sessionId)
        {
            if (_serverData.PersistedSessionData.ContainsKey(sessionId))
            {
                return _serverData.PersistedSessionData[sessionId];
            }
            return null;
        }

        public Structs.ServerContentRating GetServerContentRating()
        {
            return _serverData.ServerContentRating;
        }

        public Structs.ServerContentType GetServerContentType()
        {
            return _serverData.ServerContentType;
        }

        public string GetServerDescription()
        {
            return _serverData.ServerDescription;
        }

        public string GetServerRules()
        {
            return _serverData.ServerRules;
        }

        public string GetSynchronizationContext()
        {
            return _serverData.SynchronizationContext;
        }

        public bool IsValidUnclaimedAuthenticationKeyHash(string keyHash)
        {
            return _serverData.UnclaimedKeyHashes.Contains(keyHash);
        }

        public void RemoveUnclaimedAuthenticationKeyHash(string authenticationHash)
        {
            _serverData.UnclaimedKeyHashes.Remove(authenticationHash);
            PersistData();
        }
        public void LoadData()
        {
            var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mainConfig.json");
            if (File.Exists(configPath))
            {
                try
                {
                    _serverData = JsonConvert.DeserializeObject<ServerData>(File.ReadAllText(configPath));
                    if (_serverData.PersistedSessionData == null)
                    {
                        _serverData.PersistedSessionData = new ConcurrentDictionary<string, PersistedSessionData>();
                    }
                }
                catch
                {

                }
            }
            else
            {
                _serverData = new ServerData();
            }
        }
        public void PersistData()
        {
            var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mainConfig.json");
            var configPathBackup = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mainConfig_backup.json");

            if (File.Exists(configPath))
            {
                File.Copy(configPath, configPathBackup, true);
            }

            File.WriteAllText(configPath, JsonConvert.SerializeObject(_serverData, Formatting.Indented));
        }

        public void SetHashedAccessKey(string sessionId, string authenticationHash)
        {
            _serverData.PersistedSessionData[sessionId].HashedAccessKey = authenticationHash;
        }
        public void CheckOrCreateSessionUser(string sessionId)
        {
            if (!_serverData.PersistedSessionData.ContainsKey(sessionId))
            {
                // Add the new client. First client to ever get registered with the server is the Creator.
                _serverData.PersistedSessionData.TryAdd(sessionId, new PersistedSessionData()
                {
                    ServerRole = _serverData.PersistedSessionData.Count == 0 ? ServerRole.Creator : ServerRole.None
                });
                PersistData();
            }
        }

        public void BanUser(string sessionId)
        {
            var data = _serverData.PersistedSessionData[sessionId];
            data.Banned = true;
            data.HashedAccessKey = "";
            PersistData();
        }

        public void SetServerRules(string rules)
        {
            _serverData.ServerRules = rules;
            PersistData();
        }

        public void SetServerDescription(string description)
        {
            _serverData.ServerDescription = description;
            PersistData();
        }

        public void SetAgeGroup(Structs.AgeGroup group)
        {
            _serverData.AgeGroup = group;
            PersistData();
        }

        public void SetServerContentRating(Structs.ServerContentRating rating)
        {
            _serverData.ServerContentRating = rating;
            PersistData();
        }

        public void SetServerContentType(Structs.ServerContentType type)
        {
            _serverData.ServerContentType = type;
            PersistData();
        }

        public void SetSynchronizationContext(string synchronizationContext)
        {
            _serverData.SynchronizationContext = synchronizationContext;
        }

        public string GetServerAlias()
        {
            return _serverData.ServerAlias;
        }

        public void SetServerAlias(string alias)
        {
            _serverData.ServerAlias = alias;
        }
    }
}
