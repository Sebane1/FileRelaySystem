using FileRelaySystem;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Numerics;

namespace FileSystemRelay {
    public class ReceiveFromClientManager : StreamUtilities {
        private HttpListenerContext client;
        private FileManager fileManager;
        public enum RequestType {
            AddTemporaryFile = 0,
            GetTemporaryFile = 1,
            ClearState = 2,
            AddPersistedFile = 3,
            GetPersistedFile = 4,
            BanUser = 5,
            IssueAccessToken = 6,
        }
        public ReceiveFromClientManager(HttpListenerContext client, FileManager fileManager) {
            this.client = client;
            this.fileManager = fileManager;
        }

        public void ReceiveFromClient() {
            try {
                using (BinaryReader reader = new BinaryReader(client.Request.InputStream)) {
                    using (BinaryWriter writer = new BinaryWriter(client.Response.OutputStream)) {
                        string sessionId = reader.ReadString();
                        string authenticationToken = reader.ReadString();
                        var authenticationData = SecurityManager.Instance.Authenticate(sessionId, authenticationToken);
                        if (authenticationData.Key) {
                            string targetValue = reader.ReadString();
                            int requestType = reader.ReadInt32();
                            switch ((RequestType)requestType) {
                                case RequestType.AddTemporaryFile:
                                    long length = reader.ReadInt64();
                                    MemoryStream memoryStream = new MemoryStream();
                                    CopyStream(reader.BaseStream, memoryStream, (int)length);
                                    Console.WriteLine(sessionId + " uploading " + targetValue);
                                    int destructionTime = reader.ReadInt32();
                                    lock (fileManager) {
                                        var fileIdentifier = new FileIdentifier(targetValue, memoryStream, Math.Clamp(destructionTime, 0, 60000));
                                        fileIdentifier.OnDisposed += delegate {
                                            fileManager.Remove(targetValue);
                                        };
                                        fileManager.AddTemporaryFile(fileIdentifier);
                                    }
                                    Console.WriteLine(sessionId + " received " + targetValue);
                                    break;
                                case RequestType.GetTemporaryFile:
                                case RequestType.ClearState:
                                    GetTemporaryFile(targetValue, writer, requestType);
                                    break;
                                case RequestType.AddPersistedFile:
                                    length = reader.ReadInt64();
                                    Console.WriteLine(sessionId + " uploading " + targetValue);
                                    string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cdn", targetValue + ".hex");
                                    using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write)) {
                                        CopyStream(reader.BaseStream, fileStream, (int)length);
                                    }
                                    Console.WriteLine(sessionId + " persisted " + targetValue);
                                    break;
                                case RequestType.GetPersistedFile:
                                    GetPersistedFile(targetValue, writer);
                                    break;
                                case RequestType.BanUser:
                                    if (SecurityManager.Instance.BanSessionId(sessionId, targetValue)) {
                                        Console.WriteLine(sessionId + " banned " + targetValue);
                                    } else {
                                        Console.WriteLine(sessionId + " has insufficient permissions to ban " + targetValue);
                                        writer.Write("Ban succeeded");
                                    }
                                    break;
                                case RequestType.IssueAccessToken:
                                    if (authenticationData.Value > 0) {
                                        writer.Write(SecurityManager.Instance.GenerateUnclaimedAccessToken());
                                    }
                                    break;
                            }
                        }
                    }
                }
            } catch (Exception e) {
                Console.WriteLine(e.Message);
            }
        }
        private void GetPersistedFile(string hash, BinaryWriter writer) {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            Console.WriteLine("Client requesting " + hash);
            try {
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cdn", hash + ".hex");
                if (File.Exists(filePath)) {
                    using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                        CopyStream(fileStream, writer.BaseStream, (int)fileStream.Length);
                        writer.Flush();
                    }
                    Console.WriteLine("Sent " + hash);
                }
            } catch (Exception e) {
                Console.WriteLine("Connection interrupted for " + hash);
                Console.WriteLine(e);
            }
        }
        private void GetTemporaryFile(string hash, BinaryWriter writer, int requestType) {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            Console.WriteLine("Client requesting " + hash);
            FileIdentifier identifier = null;
            // Wait for the file data to exist, probably not uploaded or still uploading.
            while (identifier == null && stopwatch.ElapsedMilliseconds < 30000) {
                lock (fileManager) {
                    var file = fileManager.GetFile(hash);
                    if (file != null) {
                        identifier = file;
                        Console.WriteLine("Found a match for " + identifier.Identifier);
                        break;
                    }
                }
                Thread.Sleep(1000);
            }

            // Does the file exist now?
            if (identifier != null) {
                // Is the data in use by another downloader? Wait if so.
                while (identifier.InUse) {
                    Thread.Sleep(100);
                }
                // Send the data.
                try {
                    identifier.InUse = true;
                    writer.Write((byte)1);
                    if (requestType != 2) {
                        MemoryStream dataStream = null;
                        lock (identifier.MemoryStream) {
                            writer.Write(identifier.MemoryStream.Length);
                            identifier.MemoryStream.Position = 0;
                            dataStream = new MemoryStream(identifier.MemoryStream.ToArray());
                            identifier.InUse = false;
                        }
                        CopyStream(dataStream, writer.BaseStream,
                        (int)dataStream.Length);
                        writer.Flush();
                    }
                    identifier.InUse = false;
                    Console.WriteLine("Sent " + hash);
                } catch (Exception e) {
                    identifier.InUse = false;
                    Console.WriteLine("Connection interrupted for " + hash);
                    Console.WriteLine(e);
                }
            } else {
                writer.Write((byte)0);
                Console.WriteLine("Could not find " + hash);
            }
        }
    }
}
