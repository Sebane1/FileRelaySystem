using Newtonsoft.Json;
using RelayCommonData;
using System.Security.Cryptography;
using System.Text;

namespace FileRelaySystem {
    public class SecurityManager {
        SecurityManagerData _securityManagerData;
        private static SecurityManager _instance;

        public static SecurityManager Instance { get => _instance; set => _instance = value; }

        public SecurityManager() {
            LoadData();
            _instance = this;
        }

        public void PersistData() {
            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mainConfig.json");
            File.WriteAllText(configPath, JsonConvert.SerializeObject(_securityManagerData, Formatting.Indented));
        }

        public void LoadData() {
            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mainConfig.json");
            if (File.Exists(configPath)) {
                try {
                    _securityManagerData = JsonConvert.DeserializeObject<SecurityManagerData>(File.ReadAllText(configPath));
                    if (_securityManagerData.PersistedSessionData == null) {
                        _securityManagerData.PersistedSessionData = new Dictionary<string, PersistedSessionData>();
                    }
                } catch {

                }
            } else {
                _securityManagerData = new SecurityManagerData();
            }
        }

        public bool CheckIfPasswordExists() {
            return !string.IsNullOrEmpty(_securityManagerData.MasterKeyHash);
        }

        public bool SetMasterPassword(string password) {
            if (!string.IsNullOrEmpty(password)) {
                _securityManagerData.MasterKeySalt = Guid.NewGuid().ToString();
                _securityManagerData.MasterKeyHash = Hashing.SHA512Hash(password + _securityManagerData.MasterKeySalt);
                PersistData();
                return true;
            }
            return false;
        }

        public KeyValuePair<bool, ServerRole> Authenticate(string sessionId, string authenticationToken) {
            string authenticationHash = Hashing.SHA512Hash(authenticationToken);
            string masterKeyAuthenticationHash = Hashing.SHA512Hash(authenticationToken + _securityManagerData.MasterKeySalt);
            bool authenticationSuccess = false;
            if (masterKeyAuthenticationHash == _securityManagerData.MasterKeyHash) {
                CheckOrCreateSessionUser(sessionId);
                authenticationSuccess = true;
            } else if (_securityManagerData.UnclaimedKeyHashes.Contains(authenticationHash)) {
                CheckOrCreateSessionUser(sessionId);
                _securityManagerData.PersistedSessionData[sessionId].HashedAccessKey = authenticationHash;
                _securityManagerData.UnclaimedKeyHashes.Remove(authenticationHash);
            }

            if (!authenticationSuccess) {
                if (_securityManagerData.PersistedSessionData.ContainsKey(sessionId)) {
                    authenticationSuccess = _securityManagerData.PersistedSessionData[sessionId].HashedAccessKey == authenticationHash;
                } else {
                    authenticationSuccess = false;
                }
            }
            return new KeyValuePair<bool, ServerRole>(authenticationSuccess, _securityManagerData.PersistedSessionData[sessionId].ServerRole);
        }

        public void CheckOrCreateSessionUser(string sessionId) {
            if (!_securityManagerData.PersistedSessionData.ContainsKey(sessionId)) {
                // Add the new client. First client to ever get registered with the server is the Creator.
                _securityManagerData.PersistedSessionData.Add(sessionId, new PersistedSessionData() {
                    ServerRole = _securityManagerData.PersistedSessionData.Count == 1 ? ServerRole.Creator : ServerRole.None
                });
                PersistData();
            }
        }

        public string GenerateUnclaimedAccessToken() {
            string code = Guid.NewGuid().ToString();
            _securityManagerData.UnclaimedKeyHashes.Add(Hashing.SHA512Hash(code));
            PersistData();
            return code;
        }

        public bool BanSessionId(string sessionId, string sessionIdToBan) {
            if (_securityManagerData.PersistedSessionData.ContainsKey(sessionIdToBan)) {
                var userIssuingBan = _securityManagerData.PersistedSessionData[sessionIdToBan];
                // Must have a server role.
                if (userIssuingBan.ServerRole != ServerRole.None) {
                    var userToBan = _securityManagerData.PersistedSessionData[sessionIdToBan];
                    // Must be a higher rank than the user being banned.
                    if (userIssuingBan.ServerRole > userIssuingBan.ServerRole) {
                        _securityManagerData.PersistedSessionData[sessionIdToBan].Banned = true;
                        _securityManagerData.PersistedSessionData[sessionIdToBan].HashedAccessKey = "";
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
