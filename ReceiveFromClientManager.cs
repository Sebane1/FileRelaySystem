using System.Diagnostics;
using System.Net.Sockets;
using System.Numerics;
using static FileSystemRelay.FileIdentifier;

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
                                    fileManager.AddSoundFile(new FileIdentifier(hash, memoryStream) { Position = position });
                                }
                                Console.WriteLine("Received " + hash);
                                break;
                            case 1:
                            case 2:
                                Stopwatch stopwatch = new Stopwatch();
                                stopwatch.Start();
                                Console.WriteLine("Client requesting " + hash);
                                FileIdentifier identifier = null;
                                // Backup event callback to try and get up to date data
                                //EventHandler<FileIdentifierArgs> delegateValue = delegate (object v, FileIdentifierArgs e) {
                                //    lock (e.Data) {
                                //        if (e.Data.Identifier.Contains(hash)) {
                                //            identifier = e.Data;
                                //            Console.WriteLine("Event callback for " + identifier.Identifier + " found a match!");
                                //        }
                                //    }
                                //};
                                //lock (fileManager) {
                                //    fileManager.AddEvent(delegateValue);
                                //}

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
                                    lock (identifier) {
                                        identifier.InUse = true;
                                        writer.Write((byte)1);
                                        writer.Write(identifier.Position.X);
                                        writer.Write(identifier.Position.Y);
                                        writer.Write(identifier.Position.Z);
                                        if (requestType != 2) {
                                            lock (identifier.MemoryStream) {
                                                writer.Write(identifier.MemoryStream.Length);
                                                identifier.MemoryStream.Position = 0;
                                                CopyStream(identifier.MemoryStream, writer.BaseStream,
                                                (int)identifier.MemoryStream.Length);
                                                writer.Flush();
                                            }
                                        } else {
                                            Console.WriteLine("Sent position data to " + hash +
                                            $" {identifier.Position.X},{identifier.Position.Y},{identifier.Position.Z}");
                                        }
                                        identifier.InUse = false;
                                    }
                                    Console.WriteLine("Sent " + hash);
                                } else {
                                    writer.Write((byte)0);
                                    Console.WriteLine("Could not find " + hash);
                                }
                                //lock (fileManager) {
                                //    fileManager.OnNewSoundFile -= delegateValue;
                                //}
                                break;
                        }
                        Close();
                    }
                }
            } catch {
                Close();
            }
        }
    }
}
