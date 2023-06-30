using System.Net.Sockets;

namespace FileSystemRelay {
    internal class Program {
        static List<TcpListener> fileReceiverListeners = new List<TcpListener>();
        static void Main(string[] args) {
            for (int i = 0; i < 10; i = 0) {
                TcpListener fileReceiverListener = new TcpListener(5105 + ((i + 1) * 100));
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
                fileReceiverListeners.Add(fileReceiverListener);
            }
            Console.WriteLine("Server started");
            while (true) {
                Thread.Sleep(10000);
            }
        }
    }
}