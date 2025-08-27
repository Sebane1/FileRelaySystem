using System;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using static RelayUploadProtocol.Structs;

namespace RelayServerProtocol.Database {
    public class ServerData {
        ConcurrentDictionary<string, PersistedSessionData> _persistedSessionData = new ConcurrentDictionary<string, PersistedSessionData>();
        string _masterKeyHash = "";
        string _masterKeySalt = "";
        List<string> _unclaimedKeyHashes = new List<string>();

        string _serverRules = "";
        string _serverDescription = "";
        AgeGroup _ageGroup;
        ServerContentRating _serverContent;
        ServerContentType _serverContentType;
        int _synchronizationContext = -1;
        
        [Key]
        public int Id { get; set; } = 1; // always one row

        public ConcurrentDictionary<string, PersistedSessionData> PersistedSessionData { get => _persistedSessionData; set => _persistedSessionData = value; }
        public string MasterKeyHash { get => _masterKeyHash; set => _masterKeyHash = value; }
        public string MasterKeySalt { get => _masterKeySalt; set => _masterKeySalt = value; }
        public List<string> UnclaimedKeyHashes { get => _unclaimedKeyHashes; set => _unclaimedKeyHashes = value; }
        public int SynchronizationContext { get => _synchronizationContext; set => _synchronizationContext = value; }
        public string ServerRules { get => _serverRules; set => _serverRules = value; }
        public string ServerDescription { get => _serverDescription; set => _serverDescription = value; }
        public AgeGroup AgeGroup { get => _ageGroup; set => _ageGroup = value; }
        public ServerContentRating ServerContent { get => _serverContent; set => _serverContent = value; }
        public ServerContentType ServerContentType { get => _serverContentType; set => _serverContentType = value; }

    }
}
