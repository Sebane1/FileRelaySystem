using System.Net.Sockets;

namespace FileSystemRelay {
    internal class Program {
        static TcpListener fileReceiverListener = null;
        static void Main(string[] args) {
            fileReceiverListener = new TcpListener(5400);
            fileReceiverListener.Start();
            FileManager fileManager = new FileManager();
            Console.WriteLine("Starting server");
            Thread thread = new Thread(new ThreadStart(delegate {
                while (true) {
                    TcpClient client = fileReceiverListener.AcceptTcpClient();
                    client.LingerState = new LingerOption(false, 5);
                    ReceiveFromClientManager receiveFromClientManager = new ReceiveFromClientManager(client, fileManager);
                    Thread thread = new Thread(new ThreadStart(receiveFromClientManager.ReceiveFromClient));
                    thread.Start();
                }
            }));
            thread.Start();

            Console.WriteLine("Server started");
            while (true) {
                Thread.Sleep(10000);
            }
        }
    }
}