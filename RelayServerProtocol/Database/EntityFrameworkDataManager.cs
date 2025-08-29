using RelayCommonData;
using RelayUploadProtocol;
using static RelayServerProtocol.Database.ServerDatabaseContext;
using static RelayUploadProtocol.Enums;

namespace RelayServerProtocol.Database
{
    public class EntityFrameworkDataManager : IDataManager
    {
        private readonly ServerDatabaseContext _database;

        // Constructor takes connection string + provider
        public EntityFrameworkDataManager(ServerDatabaseContext serverDatabasContext)
        {
            _database = serverDatabasContext;

            // Make sure DB + schema exists
            _database.Database.EnsureCreated();
        }

        // ------------------ Master password ------------------
        public bool CheckIfPasswordExists() =>
            !string.IsNullOrEmpty(_database.ServerData.Find(1)?.MasterKeyHash);

        public bool SetMasterPassword(string password)
        {
            if (string.IsNullOrEmpty(password)) return false;

            var salt = Guid.NewGuid().ToString();
            var hash = Hashing.SHA512Hash(password + salt);

            var server = _database.ServerData.Find(1);
            server.MasterKeyHash = hash;
            server.MasterKeySalt = salt;
            _database.SaveChanges();
            return true;
        }

        public string GetMasterKeyHash() => _database.ServerData.Find(1)?.MasterKeyHash;
        public string GetMasterKeySalt() => _database.ServerData.Find(1)?.MasterKeySalt;

        // ------------------ Unclaimed keys ------------------
        public string CreateNewUnclaimedAccessToken()
        {
            var code = Guid.NewGuid().ToString();
            var hash = Hashing.SHA512Hash(code);

            _database.UnclaimedKeys.Add(new UnclaimedKey { KeyHash = hash });
            _database.SaveChanges();
            return code;
        }

        public bool IsValidUnclaimedAuthenticationKeyHash(string keyHash) =>
            _database.UnclaimedKeys.Any(k => k.KeyHash == keyHash);

        public void RemoveUnclaimedAuthenticationKeyHash(string authenticationHash)
        {
            var entity = _database.UnclaimedKeys.Find(authenticationHash);
            if (entity != null)
            {
                _database.UnclaimedKeys.Remove(entity);
                _database.SaveChanges();
            }
        }

        // ------------------ Sessions ------------------
        public PersistedSessionData GetPersistedData(string sessionId) =>
            _database.Sessions.Find(sessionId);

        public void SetHashedAccessKey(string sessionId, string authenticationHash)
        {
            var session = _database.Sessions.Find(sessionId);
            if (session != null)
            {
                session.HashedAccessKey = authenticationHash;
                _database.SaveChanges();
            }
        }

        public void CheckOrCreateSessionUser(string sessionId)
        {
            if (_database.Sessions.Find(sessionId) == null)
            {
                var role = !_database.Sessions.Any() ? ServerRole.Creator : ServerRole.None;
                _database.Sessions.Add(new PersistedSessionData
                {
                    SessionId = sessionId,
                    Banned = false,
                    ServerRole = role,
                    HashedAccessKey = ""
                });
                _database.SaveChanges();
            }
        }

        public void BanUser(string sessionId)
        {
            var session = _database.Sessions.Find(sessionId);
            if (session != null)
            {
                session.Banned = true;
                session.HashedAccessKey = "";
                _database.SaveChanges();
            }
        }

        // ------------------ ServerData fields ------------------
        public AgeGroup GetAgeGroup() => _database.ServerData.Find(1)?.AgeGroup ?? default;
        public void SetAgeGroup(AgeGroup group) { _database.ServerData.Find(1).AgeGroup = group; _database.SaveChanges(); }

        public ServerContentRating GetServerContentRating() => _database.ServerData.Find(1)?.ServerContentRating ?? default;
        public void SetServerContentRating(ServerContentRating rating) { _database.ServerData.Find(1).ServerContentRating = rating; _database.SaveChanges(); }

        public ServerContentType GetServerContentType() => _database.ServerData.Find(1)?.ServerContentType ?? default;
        public void SetServerContentType(ServerContentType type) { _database.ServerData.Find(1).ServerContentType = type; _database.SaveChanges(); }

        public string GetServerDescription() => _database.ServerData.Find(1)?.ServerDescription;
        public void SetServerDescription(string desc) { _database.ServerData.Find(1).ServerDescription = desc; _database.SaveChanges(); }

        public string GetServerRules() => _database.ServerData.Find(1)?.ServerRules;
        public void SetServerRules(string rules) { _database.ServerData.Find(1).ServerRules = rules; _database.SaveChanges(); }

        public string GetSynchronizationContext() => _database.ServerData.Find(1)?.SynchronizationContext;
        public void SetSynchronizationContext(string ctx) { _database.ServerData.Find(1).SynchronizationContext = ctx; _database.SaveChanges(); }

        public string GetServerAlias() => _database.ServerData.Find(1)?.ServerAlias;

        public void SetServerAlias(string alias) { _database.ServerData.Find(1).ServerAlias = alias; _database.SaveChanges(); }

        public int GetMaxFileSizeInMb()
        {
            return _database.ServerData.Find(1).MaxFileSizeInMb;
        }

        public void SetGeneralUserLifespan(int lifespan)
        {
            _database.ServerData.Find(1).GeneralUserLifeSpanInMilliseconds = lifespan;
        }

        public int GetGeneralUserLifespanInMilliseconds()
        {
            return _database.ServerData.Find(1).GeneralUserLifeSpanInMilliseconds;
        }

        public ServerUploadAllowance GetUploadAllowance()
        {
            return _database.ServerData.Find(1).ServerUploadAllowance;
        }

        public void SetUploadAllowance(ServerUploadAllowance uploadAllowance)
        {
            _database.ServerData.Find(1).ServerUploadAllowance = uploadAllowance;
        }

        public void SetMaxFileSizeInMb(int setMaxFileSizeInMb)
        {
            _database.ServerData.Find(1).MaxFileSizeInMb = setMaxFileSizeInMb;
        }
    }
}
