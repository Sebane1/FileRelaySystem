using System.Diagnostics;
using System.Numerics;

namespace FileSystemRelay {
    public class FileIdentifier : IDisposable {
        string identifier;
        MemoryStream memoryStream;
        bool inUse;
        Vector3 position;
        private Stopwatch stopwatch;
        public event EventHandler<FileIdentifierArgs> OnFileDataAdded;
        public event EventHandler OnDisposed;
        public FileIdentifier(string identifier, MemoryStream memoryStream) {
            this.identifier = identifier;
            this.memoryStream = memoryStream;
            Task.Run(() => DestroyOverTime());
        }

        public MemoryStream MemoryStream {
            get {
                lock (memoryStream) {
                    return memoryStream;
                }
            }
            set {
                lock (memoryStream) {
                    memoryStream = value;
                    OnFileDataAdded?.Invoke(this, new FileIdentifierArgs() { Data = this });
                }
            }
        }

        public string Identifier { get => identifier; set => identifier = value; }
        public bool InUse { get => inUse; set => inUse = value; }
        public Vector3 Position { get => position; set => position = value; }
        public Stopwatch Stopwatch { get => stopwatch; set => stopwatch = value; }

        public void Dispose() {
            memoryStream?.Dispose();
            identifier = string.Empty;
        }
        async void DestroyOverTime() {
            stopwatch = Stopwatch.StartNew();
            while (stopwatch.ElapsedMilliseconds < 35000 || inUse) {
                Thread.Sleep(1000);
            }
            Dispose();
            OnDisposed?.Invoke(this, EventArgs.Empty);
        }
        public class FileIdentifierArgs : EventArgs {
            FileIdentifier data;
            public FileIdentifier Data { get => data; set => data = value; }
        }
    }
}
