using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace FileRelaySystem {
    public class SecurityManagerData {
        Dictionary<string, PersistedSessionData> _persistedSessionData;
        string _masterKeyHash = "";
        string _masterKeySalt = "";
        List<string> _unclaimedKeyHashes = new List<string>();

        string _serverRules = "";
        AgeGroup _ageGroup;
        ServerContentRating _serverContent;
        ServerContentType _serverContentType;
        int _synchronizationContext = -1;

        public Dictionary<string, PersistedSessionData> PersistedSessionData { get => _persistedSessionData; set => _persistedSessionData = value; }
        public string MasterKeyHash { get => _masterKeyHash; set => _masterKeyHash = value; }
        public string MasterKeySalt { get => _masterKeySalt; set => _masterKeySalt = value; }
        public List<string> UnclaimedKeyHashes { get => _unclaimedKeyHashes; set => _unclaimedKeyHashes = value; }
        public int SynchronizationContext { get => _synchronizationContext; set => _synchronizationContext = value; }
    }
    public enum AgeGroup {
        Everyone = 0,
        AdultsOnly = 1
    }
    public enum ServerContentRating {
        SafeForWorkOnly = 0,
        NotSafeForWork = 1,
    }
    public enum ServerContentType {
        General = 0,
        Outfits = 1,
        Memes = 2,
        Other = 3
    }
}
