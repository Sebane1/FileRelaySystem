namespace FileRelaySystem {
    public class PersistedSessionData {
        string _sessionUserId;
        string _hashedAccessKey;
        bool _banned;
        ServerRole _serverRole;

        /// <summary>
        /// Session ID is decided by the client. Acts as the servers way to identify someone unless they change their identifier.
        /// </summary>
        public string SessionId { get => _sessionUserId; set => _sessionUserId = value; }
        /// <summary>
        /// Whether the session id associated with this object is banned from access.
        /// </summary>
        public bool Banned { get => _banned; set => _banned = value; }
        /// <summary>
        /// A hash of the access key required to use this server
        /// </summary>
        public string HashedAccessKey { get => _hashedAccessKey; set => _hashedAccessKey = value; }
        /// <summary>
        /// Defines what server actions the session user can access
        /// </summary>
        public ServerRole ServerRole { get => _serverRole; set => _serverRole = value; }
    }
    public enum ServerRole {
        None,
        Moderator,
        Admin,
        Creator
    }
}
