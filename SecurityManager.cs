using Newtonsoft.Json;
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
                    File.ReadAllText(configPath);
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
                _securityManagerData.MasterKeyHash = SHA512Hash(password + _securityManagerData.MasterKeySalt);
                PersistData();
                return true;
            }
            return false;
        }

        public KeyValuePair<bool, ServerRole> Authenticate(string sessionId, string authenticationToken) {
            string authenticationHash = SHA512Hash(authenticationToken);
            string masterKeyAuthenticationHash = SHA512Hash(authenticationToken + _securityManagerData.MasterKeyHash);
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

        public string GenerateUnclaimedAccessKey() {
            string code = Guid.NewGuid().ToString();
            _securityManagerData.UnclaimedKeyHashes.Add(SHA512Hash(code));
            PersistData();
            return code;
        }
        public void BanSessionId(string sessionIdToBan) {
            if (_securityManagerData.PersistedSessionData.ContainsKey(sessionIdToBan)) {
                // Cant ban the creator. Check that the user isn't one.
                if (_securityManagerData.PersistedSessionData[sessionIdToBan].ServerRole != ServerRole.Creator) {
                    _securityManagerData.PersistedSessionData[sessionIdToBan].Banned = true;
                    _securityManagerData.PersistedSessionData[sessionIdToBan].HashedAccessKey = "";
                }
            }
        }

        public static string SHA512Hash(string value) {
            using (var alg = SHA512.Create()) {
                var message = Encoding.UTF8.GetBytes(value);
                var hashValue = alg.ComputeHash(message);
                string hex = "";
                foreach (byte x in hashValue) {
                    hex += string.Format("{0:x2}", x);
                }
                return hex;
            }
        }
    }
}
