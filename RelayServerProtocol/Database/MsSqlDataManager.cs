using RelayUploadProtocol;

namespace RelayServerProtocol.Database
{
    internal class MsSqlDataManager : IDataManager
    {
        public void BanUser(string sessionId)
        {
            throw new NotImplementedException();
        }

        public bool CheckIfPasswordExists()
        {
            throw new NotImplementedException();
        }

        public void CheckOrCreateSessionUser(string sessionId)
        {
            throw new NotImplementedException();
        }

        public string CreateNewUnclaimedAccessToken()
        {
            throw new NotImplementedException();
        }

        public Structs.AgeGroup GetAgeGroup()
        {
            throw new NotImplementedException();
        }

        public string GetMasterKeyHash()
        {
            throw new NotImplementedException();
        }

        public string GetMasterKeySalt()
        {
            throw new NotImplementedException();
        }

        public PersistedSessionData GetPersistedData(string sessionId)
        {
            throw new NotImplementedException();
        }

        public Structs.ServerContentRating GetServerContent()
        {
            throw new NotImplementedException();
        }

        public Structs.ServerContentType GetServerContentType()
        {
            throw new NotImplementedException();
        }

        public string GetServerDescription()
        {
            throw new NotImplementedException();
        }

        public string GetServerRules()
        {
            throw new NotImplementedException();
        }

        public int GetSynchronizationContext()
        {
            throw new NotImplementedException();
        }

        public bool IsValidUnclaimedAuthenticationKeyHash(string keyHash)
        {
            throw new NotImplementedException();
        }

        public void RemoveUnclaimedAuthenticationKeyHash(string keyHash)
        {
            throw new NotImplementedException();
        }

        public void SetHashedAccessKey(string sessionId, string keyHash)
        {
            throw new NotImplementedException();
        }

        public bool SetMasterPassword(string password)
        {
            throw new NotImplementedException();
        }
    }
}
