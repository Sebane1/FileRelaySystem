using RelayClientProtocol;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using static RelayUploadProtocol.Enums;

namespace RelayUploadProtocol
{
    public static class ClientManager
    {

        public static async Task<string> GetTemporaryFile(string ipAddress, string sessionId, string authenticationToken, string fileId, string outputFolder)
        {
            string serverUrl = "http://" + ipAddress + ":5105";
            int requestType = (int)RequestType.GetTemporaryFile;

            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms, Encoding.UTF8, leaveOpen: true))
            {
                // Write request header
                writer.Write(sessionId);
                writer.Write(authenticationToken);
                writer.Write(requestType);
                writer.Write(fileId);

                writer.Flush();
                ms.Position = 0;

                using (var client = new HttpClient())
                {
                    var content = new StreamContent(ms);
                    content.Headers.Add("Content-Type", "application/octet-stream");

                    HttpResponseMessage response = await client.PostAsync(serverUrl, content);
                    response.EnsureSuccessStatusCode();

                    using (var responseStream = await response.Content.ReadAsStreamAsync())
                    using (var reader = new BinaryReader(responseStream))
                    {
                        byte successFlag = reader.ReadByte();
                        if (successFlag == 0)
                        {
                            Console.WriteLine($"File {fileId} not found.");
                            return null;
                        }

                        long length = reader.ReadInt64();
                        byte[] fileBytes = reader.ReadBytes((int)length);

                        string outputPath = Path.Combine(outputFolder, fileId + ".tmp");
                        await File.WriteAllBytesAsync(outputPath, fileBytes);

                        Console.WriteLine($"Downloaded temporary file {fileId} to {outputPath}");
                        return outputPath;
                    }
                }
            }
        }

        public static async Task PutTemporaryFile(string ipAddress, string sessionId, string authenticationToken, string fileId, string filePath, int destructionTime)
        {
            string serverUrl = "http://" + ipAddress + ":5105";
            int requestType = (int)RequestType.AddTemporaryFile;

            // Clamp destruction time to match server rules
            destructionTime = Math.Clamp(destructionTime, 0, 60000);

            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms, Encoding.UTF8, leaveOpen: true))
            {
                // Write protocol header
                writer.Write(sessionId);
                writer.Write(authenticationToken);
                writer.Write(requestType);
                writer.Write(fileId);

                // Write file
                byte[] fileBytes = File.ReadAllBytes(filePath);
                writer.Write((long)fileBytes.Length);
                writer.Write(fileBytes);

                // Write destruction time
                writer.Write(destructionTime);

                writer.Flush();
                ms.Position = 0;

                // Send raw stream
                using (var client = new HttpClient())
                {
                    var content = new StreamContent(ms);
                    content.Headers.Add("Content-Type", "application/octet-stream");

                    HttpResponseMessage response = await client.PostAsync(serverUrl, content);

                    Console.WriteLine($"Response: {response.StatusCode}");
                    string respBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(respBody);
                }
            }
        }

        public static async Task<string> GetPersistedFile(
            string ipAddress,
            string sessionId,
            string authenticationToken,
            string targetSessionId,
            string fileId,
            string outputFolder,
            string password)
        {
            string serverUrl = "http://" + ipAddress + ":5105";
            int requestType = (int)RequestType.GetPersistedFile;

            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms, Encoding.UTF8, leaveOpen: true);

            writer.Write(sessionId);
            writer.Write(authenticationToken);
            writer.Write(requestType);
            writer.Write(targetSessionId);
            writer.Write(fileId);
            writer.Flush();
            ms.Position = 0;

            using var client = new HttpClient();
            var content = new StreamContent(ms);
            content.Headers.Add("Content-Type", "application/octet-stream");

            HttpResponseMessage response = await client.PostAsync(serverUrl, content);
            response.EnsureSuccessStatusCode();

            using var responseStream = await response.Content.ReadAsStreamAsync();

            // Read salt + IV
            byte[] salt = new byte[16];
            byte[] iv = new byte[16];
            await responseStream.ReadExactlyAsync(salt, 0, salt.Length);
            await responseStream.ReadExactlyAsync(iv, 0, iv.Length);

            // Derive key
            byte[] key = CryptoUtils.DeriveKeyFromPassword(password, salt);

            // Output file path
            string outputPath = Path.Combine(outputFolder, targetSessionId + fileId + ".bin");

            // Decrypt while streaming to disk
            using (var aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;
                using var crypto = new CryptoStream(responseStream, aes.CreateDecryptor(), CryptoStreamMode.Read);
                using var fsOut = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
                await crypto.CopyToAsync(fsOut);
            }

            Console.WriteLine($"Downloaded {fileId} -> {outputPath}");
            return outputPath;
        }

        public static async Task PutPersistedFile(
            string ipAddress,
            string sessionId,
            string authenticationToken,
            string fileId,
            string filePath,
            string password)
        {
            string serverUrl = "http://" + ipAddress + ":5105";
            int requestType = (int)RequestType.AddPersistedFile;

            byte[] salt = RandomNumberGenerator.GetBytes(16);
            byte[] key = CryptoUtils.DeriveKeyFromPassword(password, salt);
            using var aes = Aes.Create();
            aes.Key = key;
            aes.GenerateIV();

            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms, Encoding.UTF8, leaveOpen: true);

            // Write protocol header
            writer.Write(sessionId);
            writer.Write(authenticationToken);
            writer.Write(requestType);
            writer.Write(fileId);

            // Reserve space for length (will fill later)
            long lengthPosition = ms.Position;
            writer.Write((long)0);

            // Write salt + IV first
            writer.Write(salt);
            writer.Write(aes.IV);
            writer.Flush();

            // Encrypt file while streaming into the request body
            using (var crypto = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write, leaveOpen: true))
            using (var fsIn = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                await fsIn.CopyToAsync(crypto);
                crypto.FlushFinalBlock();
            }

            // Update payload length
            long endPosition = ms.Position;
            long payloadLength = endPosition - (lengthPosition + sizeof(long));
            ms.Position = lengthPosition;
            writer.Write(payloadLength);

            // Reset position to start for sending
            ms.Position = 0;

            using var client = new HttpClient();
            var content = new StreamContent(ms);
            content.Headers.Add("Content-Type", "application/octet-stream");

            HttpResponseMessage response = await client.PostAsync(serverUrl, content);
            Console.WriteLine($"Response: {response.StatusCode}");
            Console.WriteLine(await response.Content.ReadAsStringAsync());
        }

        public static async Task<string> GetServerAlias(string ipAddress, string sessionId)
        {
            int requestType = (int)RequestType.GetServerAlias;
            return await GetServerInfo(ipAddress, sessionId, requestType);
        }
        public static async Task<string> GetServerRules(string ipAddress, string sessionId)
        {
            int requestType = (int)RequestType.GetServerRules;
            return await GetServerInfo(ipAddress, sessionId, requestType);
        }
        public static async Task<string> GetServerDescription(string ipAddress, string sessionId)
        {
            int requestType = (int)RequestType.GetServerDescription;
            return await GetServerInfo(ipAddress, sessionId, requestType);
        }
        public static async Task<bool> SetServerAlias(string ipAddress, string sessionId, string authenticationToken, string alias)
        {
            int requestType = (int)RequestType.SetServerAlias;
            return await SetServerInfo(ipAddress, sessionId, authenticationToken, requestType, alias);
        }
        public static async Task<bool> SetServerRules(string ipAddress, string sessionId, string authenticationToken, string rules)
        {
            int requestType = (int)RequestType.SetServerRules;
            return await SetServerInfo(ipAddress, sessionId, authenticationToken, requestType, rules);
        }
        public static async Task<bool> SetServerDescription(string ipAddress, string sessionId, string authenticationToken, string description)
        {
            int requestType = (int)RequestType.SetServerDescription;
            return await SetServerInfo(ipAddress, sessionId, authenticationToken, requestType, description);
        }


        private static async Task<string> GetServerInfo(string ipAddress, string sessionId, int requestType)
        {
            string serverUrl = "http://" + ipAddress + ":5105";
            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms, Encoding.UTF8, leaveOpen: true))
            {
                // Write protocol header
                writer.Write(sessionId);
                writer.Write("");
                writer.Write(requestType);

                writer.Flush();
                ms.Position = 0;

                // Send raw stream
                using (var client = new HttpClient())
                {
                    var content = new StreamContent(ms);
                    content.Headers.Add("Content-Type", "application/octet-stream");

                    HttpResponseMessage response = await client.PostAsync(serverUrl, content);

                    Console.WriteLine($"Response: {response.StatusCode}");
                    return await response.Content.ReadAsStringAsync();
                }
            }
        }
        private static async Task<bool> SetServerInfo(string ipAddress, string sessionId, string authenticationToken, int requestType, string value)
        {
            string serverUrl = "http://" + ipAddress + ":5105";
            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms, Encoding.UTF8, leaveOpen: true))
            {
                // Write protocol header
                writer.Write(sessionId);
                writer.Write(authenticationToken);
                writer.Write(requestType);
                writer.Write(value);

                writer.Flush();
                ms.Position = 0;

                // Send raw stream
                using (var client = new HttpClient())
                {
                    var content = new StreamContent(ms);
                    content.Headers.Add("Content-Type", "application/octet-stream");

                    HttpResponseMessage response = await client.PostAsync(serverUrl, content);

                    Console.WriteLine($"Response: {response.StatusCode}");
                    return (await response.Content.ReadAsStringAsync()) == "success";
                }
            }
        }


        private static async Task<bool> SetServerEnum(string ipAddress, string sessionId, string authenticationToken, int requestType, int selectedIndex)
        {
            string serverUrl = "http://" + ipAddress + ":5105";
            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms, Encoding.UTF8, leaveOpen: true))
            {
                // Write protocol header
                writer.Write(sessionId);
                writer.Write(authenticationToken);
                writer.Write(requestType);
                writer.Write(selectedIndex);

                writer.Flush();
                ms.Position = 0;

                // Send raw stream
                using (var client = new HttpClient())
                {
                    var content = new StreamContent(ms);
                    content.Headers.Add("Content-Type", "application/octet-stream");

                    HttpResponseMessage response = await client.PostAsync(serverUrl, content);

                    Console.WriteLine($"Response: {response.StatusCode}");
                    return (await response.Content.ReadAsStringAsync()) == "success";
                }
            }
        }
        private static async Task<int> GetServerEnum(string ipAddress, string sessionId, string authenticationToken, int requestType)
        {
            string serverUrl = "http://" + ipAddress + ":5105";
            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms, Encoding.UTF8, leaveOpen: true))
            {
                // Write protocol header
                writer.Write(sessionId);
                writer.Write(authenticationToken);
                writer.Write(requestType);

                writer.Flush();
                ms.Position = 0;

                // Send raw stream
                using (var client = new HttpClient())
                {
                    var content = new StreamContent(ms);
                    content.Headers.Add("Content-Type", "application/octet-stream");

                    HttpResponseMessage response = await client.PostAsync(serverUrl, content);

                    Console.WriteLine($"Response: {response.StatusCode}");
                    using (BinaryReader reader = new BinaryReader(response.Content.ReadAsStream()))
                    {
                        return reader.ReadInt32();
                    }
                }
            }
        }
        private static async Task<bool> GetServerBool(string ipAddress, string sessionId, string authenticationToken, string fileId, int requestType)
        {
            string serverUrl = "http://" + ipAddress + ":5105";
            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms, Encoding.UTF8, leaveOpen: true))
            {
                // Write protocol header
                writer.Write(sessionId);
                writer.Write(authenticationToken);
                writer.Write(requestType);
                writer.Write(fileId);

                writer.Flush();
                ms.Position = 0;

                // Send raw stream
                using (var client = new HttpClient())
                {
                    var content = new StreamContent(ms);
                    content.Headers.Add("Content-Type", "application/octet-stream");

                    HttpResponseMessage response = await client.PostAsync(serverUrl, content);

                    Console.WriteLine($"Response: {response.StatusCode}");
                    using (BinaryReader reader = new BinaryReader(response.Content.ReadAsStream()))
                    {
                        return reader.ReadBoolean();
                    }
                }
            }
        }
        private static async Task<long> GetServerLong(string ipAddress, string sessionId, string authenticationToken, string targetSessionId, string fileId, int requestType)
        {
            string serverUrl = "http://" + ipAddress + ":5105";
            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms, Encoding.UTF8, leaveOpen: true))
            {
                // Write protocol header
                writer.Write(sessionId);
                writer.Write(authenticationToken);
                writer.Write(requestType);
                writer.Write(targetSessionId);
                writer.Write(fileId);

                writer.Flush();
                ms.Position = 0;

                // Send raw stream
                using (var client = new HttpClient())
                {
                    var content = new StreamContent(ms);
                    content.Headers.Add("Content-Type", "application/octet-stream");

                    HttpResponseMessage response = await client.PostAsync(serverUrl, content);

                    Console.WriteLine($"Response: {response.StatusCode}");
                    using (BinaryReader reader = new BinaryReader(response.Content.ReadAsStream()))
                    {
                        return reader.ReadInt64();
                    }
                }
            }
        }

        public static async Task SetServerAgeGroup(string ipAddress, string sessionId, string authenticationToken, int selectedIndex)
        {
            await SetServerEnum(ipAddress, sessionId, authenticationToken, (int)RequestType.SetAgeGroup, selectedIndex);
        }

        public static async Task SetServerContentRating(string ipAddress, string sessionId, string authenticationToken, int selectedIndex)
        {
            await SetServerEnum(ipAddress, sessionId, authenticationToken, (int)RequestType.SetContentRating, selectedIndex);
        }

        public static async Task SetServerContentType(string ipAddress, string sessionId, string authenticationToken, int selectedIndex)
        {
            await SetServerEnum(ipAddress, sessionId, authenticationToken, (int)RequestType.SetServerContentType, selectedIndex);
        }

        public static async Task<int> GetServerAgeGroup(string ipAddress, string sessionId)
        {
            return await GetServerEnum(ipAddress, sessionId, "", (int)RequestType.GetAgeGroup);
        }

        public static async Task<int> GetServerContentRating(string ipAddress, string sessionId)
        {
            return await GetServerEnum(ipAddress, sessionId, "", (int)RequestType.GetContentRating);
        }

        public static async Task<int> GetServerContentType(string ipAddress, string sessionId)
        {
            return await GetServerEnum(ipAddress, sessionId, "", (int)RequestType.SetServerContentType);
        }

        public static async Task<long> CheckLastTimePersistedFileChanged(string ipAddress, string currentCharacterId, string authenticationKey, string targetCharacterId, string appearanceFile)
        {
            return await GetServerLong(ipAddress, currentCharacterId, authenticationKey, targetCharacterId, appearanceFile, (int)RequestType.CheckLastTimePersistedFileChanged);
        }
    }
}
