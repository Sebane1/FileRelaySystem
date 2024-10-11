using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Numerics;

namespace FileSystemRelay {
    public class ReceiveFromClientManager : StreamUtilities {
        private HttpListenerContext client;
        private FileManager fileManager;

        public ReceiveFromClientManager(HttpListenerContext client, FileManager fileManager) {
            this.client = client;
            this.fileManager = fileManager;
        }
        public void Close() {
            //if (reader != null) {
            //    reader?.Close();
            //    reader?.Dispose();
            //    client.
            //}
        }
        public void ReceiveFromClient() {
            try {
                using (BinaryReader reader = new BinaryReader(client.Request.InputStream)) {
                    using (BinaryWriter writer = new BinaryWriter(client.Response.OutputStream)) {
                        string hash = reader.ReadString();
                        int requestType = reader.ReadInt32();
                        switch (requestType) {
                            case 0:
                                long length = reader.ReadInt64();
                                MemoryStream memoryStream = new MemoryStream();
                                CopyStream(reader.BaseStream, memoryStream, (int)length);
                                Console.WriteLine("Incoming " + hash);
                                int destructionTime = reader.ReadInt32();
                                lock (fileManager) {
                                    var fileIdentifier = new FileIdentifier(hash, memoryStream, Math.Clamp(destructionTime, 0, 60000));
                                    fileIdentifier.OnDisposed += delegate {
                                        fileManager.Remove(hash);
                                    };
                                    fileManager.AddFile(fileIdentifier);
                                }
                                Console.WriteLine("Received " + hash);
                                break;
                            case 1:
                            case 2:
                                SendFile(hash, writer, requestType);
                                break;
                        }
                    }
                }
            } catch (Exception e) {
                Console.WriteLine(e.Message);
            }
            Close();
        }

        private void SendFile(string hash, BinaryWriter writer, int requestType) {
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
