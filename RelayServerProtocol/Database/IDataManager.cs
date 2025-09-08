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
        public int GetMaxFileSizeInMb();
        public void SetGeneralUserLifespan(int lifespan);
        public int GetGeneralUserLifespanInMilliseconds();
        public ServerUploadAllowance GetUploadAllowance();
        public void SetUploadAllowance(ServerUploadAllowance uploadAllowance);
        public void SetMaxFileSizeInMb(int setMaxFileSizeInMb);
        void SetSessionIdAccessTokenHash(string sessionId, string authenticationHash);
    }
}
