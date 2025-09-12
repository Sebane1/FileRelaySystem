using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;

public class SseEvent
{
    public string TargetSessionId { get; set; }
    public string File { get; set; }
    public long LastChanged { get; set; }
}

public class SseClient
{
    public static event EventHandler<Tuple<string, string, long>> OnSubscribedFileChanged;
    public static async Task ListenForFileChanges(
        string ipAddress,
        string sessionId)
    {
        try
        {
            string serverUrl = "http://" + ipAddress + ":5105";
            using var client = new HttpClient();

            // Binary handshake
            using var ms = new MemoryStream();
            using (var writer = new BinaryWriter(ms, Encoding.UTF8, leaveOpen: true))
            {
                writer.Write(sessionId);
                writer.Flush();
                ms.Position = 0;

                var content = new StreamContent(ms);
                content.Headers.Add("Content-Type", "application/octet-stream");

                using var request = new HttpRequestMessage(HttpMethod.Post, $"{serverUrl}/sse")
                {
                    Content = content
                };
                using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                using var stream = await response.Content.ReadAsStreamAsync();
                using var reader = new StreamReader(stream, Encoding.UTF8);

                while (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync();
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var parts = line.Split('|');
                    if (parts.Length < 3) continue;

                    string targetSessionId = parts[0];
                    string file = parts[1];
                    long lastChanged = long.Parse(parts[2]);

                    Console.WriteLine($"File change: {file} for {targetSessionId} at {lastChanged}");
                    OnSubscribedFileChanged.Invoke(new object(), new Tuple<string, string, long>(targetSessionId, file, lastChanged));
                }
            }
        }
        catch (Exception e)
        {

        }
    }

    public static async Task<bool> SubscribeAsync(string ipAddress, string sessionId, string authenticationToken, string targetSessionId, string file)
    {
        try
        {
            string serverUrl = "http://" + ipAddress + ":5105";
            using var ms = new MemoryStream();
            using (var writer = new BinaryWriter(ms, Encoding.UTF8, leaveOpen: true))
            {
                writer.Write(sessionId);
                writer.Write(authenticationToken);
                writer.Write(targetSessionId);
                writer.Write(file);
            }
            ms.Position = 0;

            using var client = new HttpClient();
            using var content = new StreamContent(ms);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

            var response = await client.PostAsync($"{serverUrl}/sse/subscribe", content);
            response.EnsureSuccessStatusCode();

            string respBody = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Subscribe response: {respBody}");
            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }

    public static async Task UnsubscribeAsync(string ipAddress, string sessionId, string authenticationToken, string targetSessionId, string file)
    {
        try
        {
            string serverUrl = "http://" + ipAddress + ":5105";
            using var ms = new MemoryStream();
            using (var writer = new BinaryWriter(ms, Encoding.UTF8, leaveOpen: true))
            {
                writer.Write(sessionId);
                writer.Write(authenticationToken);
                writer.Write(targetSessionId);
                writer.Write(file);
            }
            ms.Position = 0;

            using var client = new HttpClient();
            using var content = new StreamContent(ms);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

            var response = await client.PostAsync($"{serverUrl}/sse/unsubscribe", content);
            response.EnsureSuccessStatusCode();

            string respBody = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Unsubscribe response: {respBody}");
        }
        catch (Exception e)
        {

        }
    }
}
