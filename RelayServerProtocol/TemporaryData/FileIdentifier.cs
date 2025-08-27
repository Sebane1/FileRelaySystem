using System.Diagnostics;
using System.Numerics;

namespace RelayServerProtocol.TemporaryData {
    public class FileIdentifier : IDisposable {
        string identifier;
        MemoryStream memoryStream;
        bool inUse;
        int expirationTime;
        Vector3 position;
        private Stopwatch stopwatch;
        public event EventHandler OnDisposed;

        public FileIdentifier(string identifier, MemoryStream memoryStream, int expirationTime = 30000) {
            this.identifier = identifier;
            this.memoryStream = memoryStream;
            this.expirationTime = expirationTime;
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
            while (stopwatch.ElapsedMilliseconds < expirationTime || inUse) {
                Thread.Sleep(expirationTime);
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
