using RelayUploadProtocol;
using static RelayUploadProtocol.Enums;

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
        public string GetSynchronizationContext();
        public void SetSynchronizationContext(string synchronizationContext);
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
        public void SetAgeGroup(AgeGroup group);
        public ServerContentRating GetServerContentRating();
        public void SetServerContentRating(ServerContentRating rating);
        public ServerContentType GetServerContentType();
        public void SetServerContentType(ServerContentType type);
        int GetMaxFileSizeInMb();
        void SetGeneralUserLifespan(int lifespan);
        int GetGeneralUserLifespanInMilliseconds();
        ServerUploadAllowance GetUploadAllowance();
        void SetUploadAllowance(ServerUploadAllowance uploadAllowance);
        void SetMaxFileSizeInMb(int setMaxFileSizeInMb);
    }
}
