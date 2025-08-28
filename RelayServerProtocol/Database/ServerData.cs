using System;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using static RelayUploadProtocol.Structs;

namespace RelayServerProtocol.Database
{
    public class ServerData
    {
        ConcurrentDictionary<string, PersistedSessionData> _persistedSessionData = new ConcurrentDictionary<string, PersistedSessionData>();
        string _masterKeyHash = "";
        string _masterKeySalt = "";
        List<string> _unclaimedKeyHashes = new List<string>();

        string _serverAlias = "";
        string _serverRules = "";
        string _serverDescription = "";
        string _synchronizationContext = "";

        AgeGroup _ageGroup;
        ServerContentRating _serverContent;
        ServerContentType _serverContentType;
        int _maxFileSizeInMb = 500;
        int _maxUsers = 20;
        int _generalUserLifeSpanInMilliseconds = -1;


        [Key]
        public int Id { get; set; } = 1; // always one row

        public ConcurrentDictionary<string, PersistedSessionData> PersistedSessionData { get => _persistedSessionData; set => _persistedSessionData = value; }
        public string MasterKeyHash { get => _masterKeyHash; set => _masterKeyHash = value; }
        public string MasterKeySalt { get => _masterKeySalt; set => _masterKeySalt = value; }
        public List<string> UnclaimedKeyHashes { get => _unclaimedKeyHashes; set => _unclaimedKeyHashes = value; }
        public string SynchronizationContext { get => _synchronizationContext; set => _synchronizationContext = value; }
        public string ServerRules { get => _serverRules; set => _serverRules = value; }
        public string ServerDescription { get => _serverDescription; set => _serverDescription = value; }
        public AgeGroup AgeGroup { get => _ageGroup; set => _ageGroup = value; }
        public ServerContentRating ServerContentRating { get => _serverContent; set => _serverContent = value; }
        public ServerContentType ServerContentType { get => _serverContentType; set => _serverContentType = value; }
        public string ServerAlias { get => _serverAlias; set => _serverAlias = value; }
        public int MaxFileSizeInMb { get => _maxFileSizeInMb; set => _maxFileSizeInMb = value; }
        public int MaxUsers { get => _maxUsers; set => _maxUsers = value; }
        public int GeneralUserLifeSpanInMilliseconds { get => _generalUserLifeSpanInMilliseconds; set => _generalUserLifeSpanInMilliseconds = value; }
    }
}
