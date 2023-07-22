using System.Collections.Concurrent;
using static FileSystemRelay.FileIdentifier;

namespace FileSystemRelay {
    public class FileManager {
        volatile ConcurrentDictionary<string, FileIdentifier> files = new ConcurrentDictionary<string, FileIdentifier>();
        volatile Queue<EventHandler<FileIdentifierArgs>> eventQueue = new Queue<EventHandler<FileIdentifierArgs>>();

        public event EventHandler<FileIdentifierArgs> OnNewSoundFile;
        public void AddFile(FileIdentifier identifier) {
            lock (files) {
                files[identifier.Identifier] = identifier;
            }
            lock (eventQueue) {
                while (eventQueue.Count > 0) {
                    OnNewSoundFile += eventQueue.Dequeue();
                }
            }
            OnNewSoundFile?.Invoke(this, new FileIdentifierArgs() { Data = identifier });
            Console.WriteLine("Event callback for " + identifier.Identifier + " triggered");
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

        public void AddEvent(EventHandler<FileIdentifierArgs> eventHandler) {
            lock (eventQueue) {
                eventQueue.Enqueue(eventHandler);
            }
        }
    }
}
