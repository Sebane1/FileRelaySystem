using System.Text;
using static RelayUploadProtocol.Structs;

namespace RelayUploadProtocol {
    public class ClientManager {
        string ipAddress = "localhost";
        public ClientManager() {

        }
        public async Task<string> GetTemporaryFile(string sessionId, string authenticationToken, string fileId, string outputFolder) {
            string serverUrl = ipAddress + ":5105";
            int requestType = (int)RequestType.GetTemporaryFile;

            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms, Encoding.UTF8, leaveOpen: true)) {
                // Write request header
                writer.Write(sessionId);
                writer.Write(authenticationToken);
                writer.Write(fileId);
                writer.Write(requestType);
                writer.Flush();
                ms.Position = 0;

                using (var client = new HttpClient()) {
                    var content = new StreamContent(ms);
                    content.Headers.Add("Content-Type", "application/octet-stream");

                    HttpResponseMessage response = await client.PostAsync(serverUrl, content);
                    response.EnsureSuccessStatusCode();

                    using (var responseStream = await response.Content.ReadAsStreamAsync())
                    using (var reader = new BinaryReader(responseStream)) {
                        byte successFlag = reader.ReadByte();
                        if (successFlag == 0) {
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

        public async Task PutTemporaryFile(string sessionId, string authenticationToken, string fileId, string filePath, int destructionTime) {
            string serverUrl = ipAddress + ":5105";
            int requestType = (int)RequestType.AddTemporaryFile;

            // Clamp destruction time to match server rules
            destructionTime = Math.Clamp(destructionTime, 0, 60000);

            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms, Encoding.UTF8, leaveOpen: true)) {
                // Write protocol header
                writer.Write(sessionId);
                writer.Write(authenticationToken);
                writer.Write(fileId);
                writer.Write(requestType);

                // Write file
                byte[] fileBytes = File.ReadAllBytes(filePath);
                writer.Write((long)fileBytes.Length);
                writer.Write(fileBytes);

                // Write destruction time
                writer.Write(destructionTime);

                writer.Flush();
                ms.Position = 0;

                // Send raw stream
                using (var client = new HttpClient()) {
                    var content = new StreamContent(ms);
                    content.Headers.Add("Content-Type", "application/octet-stream");

                    HttpResponseMessage response = await client.PostAsync(serverUrl, content);

                    Console.WriteLine($"Response: {response.StatusCode}");
                    string respBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(respBody);
                }
            }
        }


        public async Task<string> GetPersistedFile(string sessionId, string authenticationToken, string fileId, string outputFolder) {
            string serverUrl = ipAddress + ":5105";
            int requestType = (int)RequestType.GetPersistedFile;

            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms, Encoding.UTF8, leaveOpen: true)) {
                // Write protocol header (no file data in this case)
                writer.Write(sessionId);
                writer.Write(authenticationToken);
                writer.Write(fileId); // targetValue = file hash/ID
                writer.Write(requestType);
                writer.Flush();
                ms.Position = 0;

                using (var client = new HttpClient()) {
                    var content = new StreamContent(ms);
                    content.Headers.Add("Content-Type", "application/octet-stream");

                    HttpResponseMessage response = await client.PostAsync(serverUrl, content);
                    response.EnsureSuccessStatusCode();

                    // Save the response stream as a file
                    string outputPath = Path.Combine(outputFolder, fileId + ".hex");
                    using (var responseStream = await response.Content.ReadAsStreamAsync())
                    using (var fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write)) {
                        await responseStream.CopyToAsync(fileStream);
                    }

                    Console.WriteLine($"Downloaded {fileId} to {outputPath}");
                    return outputPath;
                }
            }
        }


        public async Task PutPersistedFile(string sessionId, string authenticationToken, string fileId, string filePath) {
            string serverUrl = ipAddress + ":5105";
            int requestType = (int)RequestType.AddPersistedFile;

            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms, Encoding.UTF8, leaveOpen: true)) {
                // Write protocol header
                writer.Write(sessionId);
                writer.Write(authenticationToken);
                writer.Write(fileId);
                writer.Write(requestType);

                // Write file data
                byte[] fileBytes = File.ReadAllBytes(filePath);
                writer.Write((long)fileBytes.Length); // 64-bit length
                writer.Write(fileBytes);

                writer.Flush();
                ms.Position = 0;

                // Send raw stream
                using (var client = new HttpClient()) {
                    var content = new StreamContent(ms);
                    content.Headers.Add("Content-Type", "application/octet-stream");

                    HttpResponseMessage response = await client.PostAsync(serverUrl, content);

                    Console.WriteLine($"Response: {response.StatusCode}");
                    string respBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(respBody);
                }
            }
        }
    }
}