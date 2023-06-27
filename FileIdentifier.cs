using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSystemRelay {
    public class FileIdentifier : IDisposable {
        string identifier;
        string modName;
        MemoryStream memoryStream;
        bool inUse;
        public event EventHandler OnDisposed;
        public FileIdentifier(string identifier, MemoryStream memoryStream) {
            this.identifier = identifier;
            this.memoryStream = memoryStream;
            Task.Run(() => DestroyOverTime());
        }

        public MemoryStream MemoryStream { get => memoryStream; set => memoryStream = value; }
        public string ModName { get => modName; set => modName = value; }
        public string Identifier { get => identifier; set => identifier = value; }
        public bool InUse { get => inUse; set => inUse = value; }

        public void Dispose() {
            memoryStream.Dispose();
            modName = null;
            identifier = string.Empty;
        }
        async void DestroyOverTime() {
            Stopwatch stopwatch = Stopwatch.StartNew();
            while (stopwatch.ElapsedMilliseconds < 30000) {
                Thread.Sleep(60000);
            }
            Dispose();
            OnDisposed?.Invoke(this, EventArgs.Empty);
        }
    }
}
