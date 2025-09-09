using FileSystemRelay;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RelayCommonData;
using RelayServerProtocol.Database;
using static RelayUploadProtocol.Enums;

namespace RelayServerProtocol.Managers
{
    public class ServerAccessManager : StreamUtilities
    {
        public IDataManager DataManager { get => _dataManager; set => _dataManager = value; }

        IDataManager _dataManager;

        public ServerAccessManager()
        {
            _dataManager = ConfigureData();
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
                    var optionsBuilder = new DbContextOptionsBuilder<ServerDatabaseContext>();

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

                    var database = new ServerDatabaseContext(optionsBuilder.Options);
                    return new EntityFrameworkDataManager(database);
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
                _dataManager.SetSessionIdAccessTokenHash(sessionId, authenticationHash);
                _dataManager.RemoveUnclaimedAuthenticationKeyHash(authenticationHash);
                authenticationSuccess = true;
            }

            var persistedData = _dataManager.GetPersistedData(sessionId);
            if (!authenticationSuccess)
            {
                authenticationSuccess = persistedData.HashedAccessKey == authenticationHash;
            }
            return new KeyValuePair<bool, ServerRole>(authenticationSuccess, persistedData.ServerRole);
        }


        public string CreateNewUnclaimedAccessToken()
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

        internal string GetServerAlias()
        {
            return _dataManager.GetServerAlias();
        }

        internal string GetServerRules()
        {
            return _dataManager.GetServerRules();
        }

        internal string GetServerDescription()
        {
            return _dataManager.GetServerDescription();
        }

        internal int GetAgeGroup()
        {
            return (int)_dataManager.GetAgeGroup();
        }

        internal int GetContentRating()
        {
            return (int)_dataManager.GetServerContentRating();
        }

        internal int GetServerContentType()
        {
            return (int)_dataManager.GetServerContentType();
        }

        internal bool GetPublicServerInfo()
        {
            // To do
            throw new NotImplementedException();
        }
        public void SetServerAlias(string alias)
        {
            _dataManager.SetServerAlias(alias);
        }

        public void SetServerRules(string rules)
        {
            _dataManager.SetServerRules(rules);
        }
        public void SetServerDescription(string description)
        {
            _dataManager.SetServerDescription(description);
        }

        public void SetAgeGroup(int ageGroupEnum)
        {
            _dataManager.SetAgeGroup((AgeGroup)ageGroupEnum);
        }

        public void SetContentRating(int contentRatingEnum)
        {
            _dataManager.SetServerContentRating((ServerContentRating)contentRatingEnum);
        }

        public void SetServerContentType(int serverContentType)
        {
            _dataManager.SetServerContentType((ServerContentType)serverContentType);
        }

        public void AddPersistedFile(string targetSessionId, string file, BinaryReader reader, BinaryWriter writer)
        {
            var length = reader.ReadInt64();
            Console.WriteLine(targetSessionId + " uploading " + file);
            var directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cdn");
            Directory.CreateDirectory(directory);
            var filePath = Path.Combine(directory, targetSessionId + file + ".hex");
            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                CopyStream(reader.BaseStream, fileStream, (int)length);
            }
            Console.WriteLine(targetSessionId + " persisted " + file);
        }

        public long CheckLastTimePersistedFileChanged(string targetSessionId, string file, BinaryReader reader, BinaryWriter writer)
        {
            var directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cdn");
            Directory.CreateDirectory(directory);
            var filePath = Path.Combine(directory, targetSessionId + file + ".hex");
            if (File.Exists(filePath))
            {
                return File.GetLastWriteTimeUtc(filePath).Ticks;
            }
            return 0;
        }

        public bool CheckIfFileExists(string targetSessionId, string file, BinaryReader reader, BinaryWriter writer)
        {
            var directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cdn");
            Directory.CreateDirectory(directory);
            var filePath = Path.Combine(directory, targetSessionId + file + ".hex");
            return File.Exists(filePath);
        }

        public ServerUploadAllowance GetUploadAllowance()
        {
            return _dataManager.GetUploadAllowance();
        }

        public string GetSynchronizationContext()
        {
            return _dataManager.GetSynchronizationContext();
        }

        public int GetMaxFileSizeInMb()
        {
            return _dataManager.GetMaxFileSizeInMb();
        }

        public void SetGeneralUserLifespan(int lifespan)
        {
            _dataManager.SetGeneralUserLifespan(lifespan);
        }

        public int GetGeneralUserLifespan()
        {
            return _dataManager.GetGeneralUserLifespanInMilliseconds();
        }

        public void SetUploadAllowance(ServerUploadAllowance uploadAllowance)
        {
            _dataManager.SetUploadAllowance(uploadAllowance);
        }

        public void SetSyncronizationContext(string synchronizationContext)
        {
            _dataManager.SetSynchronizationContext(synchronizationContext);
        }

        public void SetMaxFileSizeInMb(int maxFileSizeInMb)
        {
            _dataManager.SetMaxFileSizeInMb(maxFileSizeInMb);
        }
    }
}
