using System.Text;
using static RelayUploadProtocol.Enums;

namespace RelayUploadProtocol
{
    public static class ClientManager
    {
        static string ipAddress = "localhost";

        public static string IpAddress { get => ipAddress; set => ipAddress = value; }

        public static async Task<string> GetTemporaryFile(string sessionId, string authenticationToken, string fileId, string outputFolder)
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

        public static async Task PutTemporaryFile(string sessionId, string authenticationToken, string fileId, string filePath, int destructionTime)
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

        public static async Task<string> GetPersistedFile(string sessionId, string authenticationToken, string fileId, string outputFolder)
        {
            string serverUrl = "http://" + ipAddress + ":5105";
            int requestType = (int)RequestType.GetPersistedFile;

            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms, Encoding.UTF8, leaveOpen: true))
            {
                // Write protocol header (no file data in this case)
                writer.Write(sessionId);
                writer.Write(authenticationToken);
                writer.Write(requestType);
                writer.Write(fileId); // targetValue = file hash/ID
                writer.Flush();
                ms.Position = 0;

                using (var client = new HttpClient())
                {
                    var content = new StreamContent(ms);
                    content.Headers.Add("Content-Type", "application/octet-stream");

                    HttpResponseMessage response = await client.PostAsync(serverUrl, content);
                    response.EnsureSuccessStatusCode();

                    // Save the response stream as a file
                    string outputPath = Path.Combine(outputFolder, fileId + ".hex");
                    using (var responseStream = await response.Content.ReadAsStreamAsync())
                    using (var fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                    {
                        await responseStream.CopyToAsync(fileStream);
                    }

                    Console.WriteLine($"Downloaded {fileId} to {outputPath}");
                    return outputPath;
                }
            }
        }


        public static async Task PutPersistedFile(string sessionId, string authenticationToken, string fileId, string filePath)
        {
            string serverUrl = "http://" + ipAddress + ":5105";
            int requestType = (int)RequestType.AddPersistedFile;

            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms, Encoding.UTF8, leaveOpen: true))
            {
                // Write protocol header
                writer.Write(sessionId);
                writer.Write(authenticationToken);
                writer.Write(requestType);
                writer.Write(fileId);

                // Write file data
                byte[] fileBytes = File.ReadAllBytes(filePath);
                writer.Write((long)fileBytes.Length); // 64-bit length
                writer.Write(fileBytes);

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
        public static async Task<string> GetServerAlias(string sessionId)
        {
            int requestType = (int)RequestType.GetServerAlias;
            return await GetServerInfo(sessionId, requestType);
        }
        public static async Task<string> GetServerRules(string sessionId)
        {
            int requestType = (int)RequestType.GetServerRules;
            return await GetServerInfo(sessionId, requestType);
        }
        public static async Task<string> GetServerDescription(string sessionId)
        {
            int requestType = (int)RequestType.GetServerDescription;
            return await GetServerInfo(sessionId, requestType);
        }
        public static async Task<bool> SetServerAlias(string sessionId, string authenticationToken, string alias)
        {
            int requestType = (int)RequestType.SetServerAlias;
            return await SetServerInfo(sessionId, authenticationToken, requestType, alias);
        }
        public static async Task<bool> SetServerRules(string sessionId, string authenticationToken, string rules)
        {
            int requestType = (int)RequestType.SetServerRules;
            return await SetServerInfo(sessionId, authenticationToken, requestType, rules);
        }
        public static async Task<bool> SetServerDescription(string sessionId, string authenticationToken, string description)
        {
            int requestType = (int)RequestType.SetServerDescription;
            return await SetServerInfo(sessionId, authenticationToken, requestType, description);
        }


        private static async Task<string> GetServerInfo(string sessionId, int requestType)
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
        private static async Task<bool> SetServerInfo(string sessionId, string authenticationToken, int requestType, string value)
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


        private static async Task<bool> SetServerEnum(string sessionId, string authenticationToken, int requestType, int selectedIndex)
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
        private static async Task<int> GetServerEnum(string sessionId, string authenticationToken, int requestType)
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
        public static async Task SetServerAgeGroup(string sessionId, string authenticationToken, int selectedIndex)
        {
            await SetServerEnum(sessionId, authenticationToken, (int)RequestType.SetAgeGroup, selectedIndex);
        }

        public static async Task SetServerContentRating(string sessionId, string authenticationToken, int selectedIndex)
        {
            await SetServerEnum(sessionId, authenticationToken, (int)RequestType.SetContentRating, selectedIndex);
        }

        public static async Task SetServerContentType(string sessionId, string authenticationToken, int selectedIndex)
        {
            await SetServerEnum(sessionId, authenticationToken, (int)RequestType.SetServerContentType, selectedIndex);
        }

        public static async Task<int> GetServerAgeGroup(string sessionId)
        {
            return await GetServerEnum(sessionId, "", (int)RequestType.GetAgeGroup);
        }

        public static async Task<int> GetServerContentRating(string sessionId)
        {
            return await GetServerEnum(sessionId, "", (int)RequestType.GetContentRating);
        }

        public static async Task<int> GetServerContentType(string sessionId)
        {
            return await GetServerEnum(sessionId, "", (int)RequestType.SetServerContentType);
        }
    }
}
