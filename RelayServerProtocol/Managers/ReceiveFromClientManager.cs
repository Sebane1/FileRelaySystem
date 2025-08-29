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
        public ReceiveFromClientManager(HttpListenerContext client, TemporaryFileManager fileManager)
        {
            this.client = client;
            this.fileManager = fileManager;
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
                        var authenticationData = ServerAccessManager.Instance.Authenticate(sessionId, authenticationToken);
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
            // This allows the client use to view what the server is all about before joining.
            switch ((RequestType)requestType)
            {
                case RequestType.GetServerAlias:
                    writer.Write(ServerAccessManager.Instance.GetServerAlias());
                    break;
                case RequestType.GetServerRules:
                    writer.Write(ServerAccessManager.Instance.GetServerRules());
                    break;
                case RequestType.GetServerDescription:
                    writer.Write(ServerAccessManager.Instance.GetServerDescription());
                    break;
                case RequestType.GetAgeGroup:
                    writer.Write(ServerAccessManager.Instance.GetAgeGroup());
                    break;
                case RequestType.GetContentRating:
                    writer.Write(ServerAccessManager.Instance.GetContentRating());
                    break;
                case RequestType.GetServerContentType:
                    writer.Write(ServerAccessManager.Instance.GetServerContentType());
                    break;
                case RequestType.GetPublicServerInfo:
                    writer.Write(ServerAccessManager.Instance.GetPublicServerInfo());
                    break;
                case RequestType.GetUploadAllowance:
                    writer.Write((int)ServerAccessManager.Instance.GetUploadAllowance());
                    break;
                case RequestType.GetSynchronizationContext:
                    writer.Write(ServerAccessManager.Instance.GetSynchronizationContext());
                    break;
                case RequestType.GetMaxFileSizeInMb:
                    writer.Write(ServerAccessManager.Instance.GetMaxFileSizeInMb());
                    break;
                case RequestType.GetGeneralUserLifespan:
                    writer.Write(ServerAccessManager.Instance.GetGeneralUserLifespan());
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
                    var targetValue = reader.ReadString();
                    AddTemporaryFile(sessionId, targetValue, reader, writer);
                    Console.WriteLine(sessionId + " received " + targetValue);
                    break;
                case RequestType.GetTemporaryFile:
                case RequestType.ClearState:
                    targetValue = reader.ReadString();
                    GetTemporaryFile(targetValue, writer, requestType);
                    break;
                case RequestType.AddPersistedFile:
                    targetValue = reader.ReadString();
                    ServerAccessManager.Instance.AddPersistedFile(sessionId, targetValue, reader, writer);
                    break;
                case RequestType.GetPersistedFile:
                    targetValue = reader.ReadString();
                    GetPersistedFile(targetValue, writer);
                    break;
                case RequestType.CheckIfPersistedFileChanged:
                    targetValue = reader.ReadString();
                    writer.Write(ServerAccessManager.Instance.CheckIfPersistedFileChanged(sessionId, targetValue, reader, writer));
                    break;
                case RequestType.CheckIfFileExists:
                    targetValue = reader.ReadString();
                    writer.Write(ServerAccessManager.Instance.CheckIfFileExists(sessionId, targetValue, reader, writer));
                    break;
                case RequestType.BanUser:
                    targetValue = reader.ReadString();
                    if (ServerAccessManager.Instance.BanSessionId(sessionId, targetValue))
                    {
                        Console.WriteLine(sessionId + " banned " + targetValue);
                        writer.Write("Successfully banned " + targetValue);
                    }
                    else
                    {
                        Console.WriteLine(sessionId + " has insufficient permissions to ban " + targetValue);
                        writer.Write("Insufficient Permissions");
                    }
                    break;
                case RequestType.IssueAccessToken:
                    // User has a role above being a normal user.
                    if (authenticationData.Value > 0)
                    {
                        writer.Write(ServerAccessManager.Instance.CreateNewUnclaimedAccessToken());
                    }
                    break;
                case RequestType.SetServerAlias:
                    ServerAccessManager.Instance.SetServerAlias(reader.ReadString());
                    break;
                case RequestType.SetServerRules:
                    ServerAccessManager.Instance.SetServerRules(reader.ReadString());
                    break;
                case RequestType.SetServerDescription:
                    ServerAccessManager.Instance.SetServerDescription(reader.ReadString());
                    break;
                case RequestType.SetContentRating:
                    ServerAccessManager.Instance.SetContentRating(reader.ReadInt32());
                    break;
                case RequestType.SetServerContentType:
                    ServerAccessManager.Instance.SetServerContentType(reader.ReadInt32());
                    break;
                case RequestType.SetAgeGroup:
                    ServerAccessManager.Instance.SetAgeGroup(reader.ReadInt32());
                    break;
                case RequestType.SetUploadAllowance:
                    ServerAccessManager.Instance.SetUploadAllowance((ServerUploadAllowance)reader.ReadInt32());
                    break;
                case RequestType.SetSynchronizationContext:
                    ServerAccessManager.Instance.SetSyncronizationContext(reader.ReadString());
                    break;
                case RequestType.SetMaxFileSizeInMb:
                    ServerAccessManager.Instance.SetMaxFileSizeInMb(reader.ReadInt32());
                    break;
                case RequestType.SetGeneralUserLifespan:
                    ServerAccessManager.Instance.SetGeneralUserLifespan(reader.ReadInt32());
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

        private void GetPersistedFile(string hash, BinaryWriter writer)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            Console.WriteLine("Client requesting " + hash);
            try
            {
                var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cdn", hash + ".hex");
                if (File.Exists(filePath))
                {
                    using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        CopyStream(fileStream, writer.BaseStream, (int)fileStream.Length);
                        writer.Flush();
                    }
                    Console.WriteLine("Sent " + hash);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Connection interrupted for " + hash);
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
