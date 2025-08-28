using RelayUploadProtocol;
using static RelayUploadProtocol.Structs;

namespace RelayServerProtocol.Database
{
    public interface IDataManager
    {
        public bool CheckIfPasswordExists();
        public bool SetMasterPassword(string password);
        public PersistedSessionData GetPersistedData(string sessionId);
        public string GetMasterKeyHash();
        public string GetMasterKeySalt();
        public bool IsValidUnclaimedAuthenticationKeyHash(string keyHash);
        public void RemoveUnclaimedAuthenticationKeyHash(string keyHash);
        public string CreateNewUnclaimedAccessToken();
        public int GetSynchronizationContext();
        public void SetHashedAccessKey(string sessionId, string keyHash);
        public void CheckOrCreateSessionUser(string sessionId);
        public string GetServerAlias();
        public void SetServerAlias(string alias);
        public string GetServerRules();
        public void SetServerRules(string rules);
        public string GetServerDescription();
        public void SetServerDescription(string description);
        public void BanUser(string sessionId);
        public AgeGroup GetAgeGroup();
        public void SetAgeGroup(Structs.AgeGroup group);
        public ServerContentRating GetServerContentRating();
        public void SetServerContentRating(Structs.ServerContentRating rating);
        public ServerContentType GetServerContentType();
        public void SetServerContentType(Structs.ServerContentType type);
    }
}
