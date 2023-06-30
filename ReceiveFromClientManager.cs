using System.Diagnostics;
using System.Net.Sockets;

namespace FileSystemRelay {
    public class ReceiveFromClientManager : StreamUtilities {
        private TcpClient client;
        private FileManager fileManager;

        public ReceiveFromClientManager(TcpClient client, FileManager fileManager) {
            this.client = client;
            this.fileManager = fileManager;
        }

        public void ReceiveFromClient() {
            using (BinaryReader reader = new BinaryReader(client.GetStream())) {
                using (BinaryWriter writer = new BinaryWriter(client.GetStream())) {
                    while (true) {
                        try {
                            string hash = reader.ReadString();
                            switch (reader.ReadInt32()) {
                                case 0:
                                    long length = reader.ReadInt64();
                                    MemoryStream memoryStream = new MemoryStream();
                                    CopyStream(reader.BaseStream, memoryStream, (int)length);
                                    if (!fileManager.Files.ContainsKey(hash)) {
                                        fileManager.Files.Add(hash, new FileIdentifier(hash, memoryStream));
                                        fileManager.Files[hash].OnDisposed += delegate { fileManager.Files.Remove(hash); };
                                    }
                                    Console.WriteLine("Audio was received");
                                    break;
                                case 1:
                                    Stopwatch stopwatch = new Stopwatch();
                                    stopwatch.Start();
                                    // Wait for the file to exist, probably not uploaded or still uploading.
                                    while (!fileManager.Files.ContainsKey(hash) && stopwatch.ElapsedMilliseconds < 10000) {
                                        Thread.Sleep(1000);
                                    }

                                    // Does the file exist now?
                                    if (fileManager.Files.ContainsKey(hash)) {
                                        FileIdentifier identifier = fileManager.Files[hash];
                                        // Is the data in use by another downloader? Wait if so.
                                        while (identifier.InUse) {
                                            Thread.Sleep(100);
                                        }
                                        // Send the data.
                                        lock (identifier) {
                                            identifier.InUse = true;
                                            writer.Write((byte)1);
                                            writer.Write(identifier.MemoryStream.Length);
                                            identifier.MemoryStream.Position = 0;
                                            CopyStream(identifier.MemoryStream, writer.BaseStream, (int)identifier.MemoryStream.Length);
                                            writer.Flush();
                                            identifier.InUse = false;
                                        }
                                        Console.WriteLine("Audio was sent");
                                    } else {
                                        writer.Write((byte)0);
                                        Console.WriteLine("Audio was not found");
                                    }
                                    break;
                            }
                        } catch {
                            client.Client.Shutdown(SocketShutdown.Both);
                            client.Client.Disconnect(true);
                            client.Close();
                            client.Dispose();
                            break;
                        }
                    }
                }
            }
        }
    }
}
