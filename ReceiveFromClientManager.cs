using System.Diagnostics;
using System.Net.Sockets;
using System.Numerics;

namespace FileSystemRelay {
    public class ReceiveFromClientManager : StreamUtilities {
        private TcpClient client;
        private FileManager fileManager;

        public ReceiveFromClientManager(TcpClient client, FileManager fileManager) {
            this.client = client;
            this.fileManager = fileManager;
        }
        public void Close() {
            if (client != null) {
                if (client.Client != null) {
                    try {
                        client.Client?.Shutdown(SocketShutdown.Both);
                        client.Client?.Disconnect(true);
                        client?.Close();
                        client?.Dispose();
                    } catch {

                    }
                }
            }
        }
        public void ReceiveFromClient() {
            try {
                using (BinaryReader reader = new BinaryReader(client.GetStream())) {
                    using (BinaryWriter writer = new BinaryWriter(client.GetStream())) {
                        string hash = reader.ReadString();
                        int requestType = reader.ReadInt32();
                        switch (requestType) {
                            case 0:
                                Vector3 position = new Vector3(
                                reader.ReadSingle(),
                                reader.ReadSingle(),
                                reader.ReadSingle());
                                long length = reader.ReadInt64();
                                MemoryStream memoryStream = new MemoryStream();
                                CopyStream(reader.BaseStream, memoryStream, (int)length);
                                Console.WriteLine("Incoming " + hash + " at position " +
                                position.X + ", " + position.Y + ", " + position.Z + ".");
                                lock (fileManager) {
                                    var fileIdentifier = new FileIdentifier(hash, memoryStream) { Position = position };
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
                            case 4:
                                length = reader.ReadInt64();
                                memoryStream = new MemoryStream();
                                CopyStream(reader.BaseStream, memoryStream, (int)length);
                                lock (fileManager) {
                                    var fileIdentifier = new FileIdentifier(hash, memoryStream, reader.ReadInt32());
                                    fileIdentifier.OnDisposed += delegate {
                                        fileManager.Remove(hash);
                                    };
                                    fileManager.AddFile(fileIdentifier);
                                }
                                Console.WriteLine("Received " + hash);
                                break;
                        }
                        Close();
                    }
                }
            } catch {
                Close();
            }
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
                    writer.Write(identifier.Position.X);
                    writer.Write(identifier.Position.Y);
                    writer.Write(identifier.Position.Z);
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
