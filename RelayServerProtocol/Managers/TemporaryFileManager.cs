using RelayServerProtocol.TemporaryData;
using System.Collections.Concurrent;

namespace RelayServerProtocol.Managers {
    public class TemporaryFileManager {
        volatile ConcurrentDictionary<string, FileIdentifier> files = new ConcurrentDictionary<string, FileIdentifier>();

        public void AddTemporaryFile(FileIdentifier identifier) {
            lock (files) {
                if (files.ContainsKey(identifier.Identifier)) {
                    try {
                        files[identifier.Identifier]?.Dispose();
                    } catch (Exception ex) {
                        Console.WriteLine(ex.ToString());
                    }
                }
                files[identifier.Identifier] = identifier;
            }
        }

        public FileIdentifier GetFile(string hash) {
            lock (files) {
                try {
                    return files[hash];
                } catch {
                    return null;
                }
            }
        }
        public void Remove(string hash) {
            lock (files) {
                FileIdentifier value;
                files.Remove(hash, out value);
            }
        }
    }
}
