using FileRelaySystem;
using static RelayUploadProtocol.Structs;

namespace RelayServerProtocol
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
        public string GetServerRules();
        public string GetServerDescription();
        public void BanUser(string sessionId);
        public AgeGroup GetAgeGroup();
        public ServerContentRating GetServerContent();
        public ServerContentType GetServerContentType();
    }
}
