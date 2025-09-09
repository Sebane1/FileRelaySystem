using FileSystemRelay;
using RelayServerProtocol.Database;
using RelayServerProtocol.TemporaryData;
using System.Diagnostics;
using System.Net;
using static RelayUploadProtocol.Enums;

namespace RelayServerProtocol.Managers
{
    public class ReceiveFromClientManager : StreamUtilities
    {
        private HttpListenerContext client;
        private TemporaryFileManager fileManager;
        private ServerAccessManager serverAccessManager;

        public ReceiveFromClientManager(HttpListenerContext client, TemporaryFileManager fileManager, ServerAccessManager serverAccessManager)
        {
            this.client = client;
            this.fileManager = fileManager;
            this.serverAccessManager = serverAccessManager;
        }

        public void ReceiveFromClient()
        {
            using (var reader = new BinaryReader(client.Request.InputStream))
            {
                using (var writer = new BinaryWriter(client.Response.OutputStream))
                {
                    try
                    {
                        var sessionId = reader.ReadString();
                        var authenticationToken = reader.ReadString();
                        var requestType = reader.ReadInt32();
                        var authenticationData = serverAccessManager.Authenticate(sessionId, authenticationToken);
                        if (authenticationData.Key)
                        {
                            HandleRequestsRequiringAuthentication(sessionId, requestType, authenticationData, reader, writer);
                        }
                        else
                        {
                            HandleRequestsNotRequiringAuthentication(sessionId, requestType, authenticationData, reader, writer);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        client.Response.StatusCode = 400;
                        client.Response.Close();
                    }
                }
            }
        }

        private void HandleRequestsNotRequiringAuthentication(string sessionId, int requestType, KeyValuePair<bool, ServerRole> authenticationData, BinaryReader reader, BinaryWriter writer)
        {
            // This data doesn't require authentication to access.
            // This allows the client user to view what the server is all about before joining.
            switch ((RequestType)requestType)
            {
                case RequestType.GetServerAlias:
                    writer.Write(serverAccessManager.GetServerAlias());
                    break;
                case RequestType.GetServerRules:
                    writer.Write(serverAccessManager.GetServerRules());
                    break;
                case RequestType.GetServerDescription:
                    writer.Write(serverAccessManager.GetServerDescription());
                    break;
                case RequestType.GetAgeGroup:
                    writer.Write(serverAccessManager.GetAgeGroup());
                    break;
                case RequestType.GetContentRating:
                    writer.Write(serverAccessManager.GetContentRating());
                    break;
                case RequestType.GetServerContentType:
                    writer.Write(serverAccessManager.GetServerContentType());
                    break;
                case RequestType.GetPublicServerInfo:
                    writer.Write(serverAccessManager.GetPublicServerInfo());
                    break;
                case RequestType.GetUploadAllowance:
                    writer.Write((int)serverAccessManager.GetUploadAllowance());
                    break;
                case RequestType.GetSynchronizationContext:
                    writer.Write(serverAccessManager.GetSynchronizationContext());
                    break;
                case RequestType.GetMaxFileSizeInMb:
                    writer.Write(serverAccessManager.GetMaxFileSizeInMb());
                    break;
                case RequestType.GetGeneralUserLifespan:
                    writer.Write(serverAccessManager.GetGeneralUserLifespan());
                    break;
                default:
                    client.Response.StatusCode = 401;
                    client.Response.Close();
                    break;
            }
        }

        private void HandleRequestsRequiringAuthentication(string sessionId, int requestType, KeyValuePair<bool, ServerRole> authenticationData, BinaryReader reader, BinaryWriter writer)
        {
            switch ((RequestType)requestType)
            {
                case RequestType.AddTemporaryFile:
                    var file = reader.ReadString();
                    AddTemporaryFile(sessionId, file, reader, writer);
                    Console.WriteLine(sessionId + " received " + file);
                    break;
                case RequestType.GetTemporaryFile:
                case RequestType.ClearState:
                    file = reader.ReadString();
                    GetTemporaryFile(file, writer, requestType);
                    break;
                case RequestType.AddPersistedFile:
                    file = reader.ReadString();
                    serverAccessManager.AddPersistedFile(sessionId, file, reader, writer);
                    break;
                case RequestType.GetPersistedFile:
                    var targetSessionId = reader.ReadString();
                    file = reader.ReadString();
                    GetPersistedFile(sessionId, targetSessionId, file, writer);
                    break;
                case RequestType.CheckLastTimePersistedFileChanged:
                    targetSessionId = reader.ReadString();
                    file = reader.ReadString();
                    writer.Write(serverAccessManager.CheckLastTimePersistedFileChanged(targetSessionId, file, reader, writer));
                    break;
                case RequestType.CheckIfFileExists:
                    targetSessionId = reader.ReadString();
                    file = reader.ReadString();
                    writer.Write(serverAccessManager.CheckIfFileExists(targetSessionId, file, reader, writer));
                    break;
                case RequestType.BanUser:
                    targetSessionId = reader.ReadString();
                    if (serverAccessManager.BanSessionId(sessionId, targetSessionId))
                    {
                        Console.WriteLine(sessionId + " banned " + targetSessionId);
                        writer.Write("Successfully banned " + targetSessionId);
                    }
                    else
                    {
                        Console.WriteLine(sessionId + " has insufficient permissions to ban " + targetSessionId);
                        writer.Write("Insufficient Permissions");
                    }
                    break;
                case RequestType.IssueAccessToken:
                    // User has a role above being a normal user.
                    if (authenticationData.Value > 0)
                    {
                        writer.Write(serverAccessManager.CreateNewUnclaimedAccessToken());
                    }
                    break;
                case RequestType.SetServerAlias:
                    serverAccessManager.SetServerAlias(reader.ReadString());
                    break;
                case RequestType.SetServerRules:
                    serverAccessManager.SetServerRules(reader.ReadString());
                    break;
                case RequestType.SetServerDescription:
                    serverAccessManager.SetServerDescription(reader.ReadString());
                    break;
                case RequestType.SetContentRating:
                    serverAccessManager.SetContentRating(reader.ReadInt32());
                    break;
                case RequestType.SetServerContentType:
                    serverAccessManager.SetServerContentType(reader.ReadInt32());
                    break;
                case RequestType.SetAgeGroup:
                    serverAccessManager.SetAgeGroup(reader.ReadInt32());
                    break;
                case RequestType.SetUploadAllowance:
                    serverAccessManager.SetUploadAllowance((ServerUploadAllowance)reader.ReadInt32());
                    break;
                case RequestType.SetSynchronizationContext:
                    serverAccessManager.SetSyncronizationContext(reader.ReadString());
                    break;
                case RequestType.SetMaxFileSizeInMb:
                    serverAccessManager.SetMaxFileSizeInMb(reader.ReadInt32());
                    break;
                case RequestType.SetGeneralUserLifespan:
                    serverAccessManager.SetGeneralUserLifespan(reader.ReadInt32());
                    break;

            }
        }

        private void AddTemporaryFile(string sessionId, string targetValue, BinaryReader reader, BinaryWriter writer)
        {
            var length = reader.ReadInt64();
            var memoryStream = new MemoryStream();
            CopyStream(reader.BaseStream, memoryStream, (int)length);
            Console.WriteLine(sessionId + " uploading " + targetValue);
            var destructionTime = reader.ReadInt32();
            lock (fileManager)
            {
                var fileIdentifier = new FileIdentifier(targetValue, memoryStream, Math.Clamp(destructionTime, 0, 60000));
                fileIdentifier.OnDisposed += delegate
                {
                    fileManager.Remove(targetValue);
                };
                fileManager.AddTemporaryFile(fileIdentifier);
            }
        }

        private void GetPersistedFile(string sessionId, string targetSessionId, string file, BinaryWriter writer)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            Console.WriteLine(sessionId + " requesting " + file);
            try
            {
                var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cdn", targetSessionId + file + ".hex");
                if (File.Exists(filePath))
                {
                    using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        CopyStream(fileStream, writer.BaseStream, (int)fileStream.Length);
                        writer.Flush();
                    }
                    Console.WriteLine("Sent " + file + " to " + sessionId);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Connection interrupted for " + file);
                Console.WriteLine(e);
            }
        }
        private void GetTemporaryFile(string hash, BinaryWriter writer, int requestType)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            Console.WriteLine("Client requesting " + hash);
            FileIdentifier identifier = null;
            // Wait for the file data to exist, probably not uploaded or still uploading.
            while (identifier == null && stopwatch.ElapsedMilliseconds < 30000)
            {
                lock (fileManager)
                {
                    var file = fileManager.GetFile(hash);
                    if (file != null)
                    {
                        identifier = file;
                        Console.WriteLine("Found a match for " + identifier.Identifier);
                        break;
                    }
                }
                Thread.Sleep(1000);
            }

            // Does the file exist now?
            if (identifier != null)
            {
                // Is the data in use by another downloader? Wait if so.
                while (identifier.InUse)
                {
                    Thread.Sleep(100);
                }
                // Send the data.
                try
                {
                    identifier.InUse = true;
                    writer.Write((byte)1);
                    if (requestType != 2)
                    {
                        MemoryStream dataStream = null;
                        lock (identifier.MemoryStream)
                        {
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
                }
                catch (Exception e)
                {
                    identifier.InUse = false;
                    Console.WriteLine("Connection interrupted for " + hash);
                    Console.WriteLine(e);
                }
            }
            else
            {
                writer.Write((byte)0);
                Console.WriteLine("Could not find " + hash);
            }
        }
    }
}
