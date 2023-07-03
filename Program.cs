using System.Net.Sockets;

namespace FileSystemRelay {
    internal class Program {
        static List<TcpListener> fileReceiverListeners = new List<TcpListener>();
        static void Main(string[] args) {
            Console.WriteLine("Starting server");
            FileManager fileManager = new FileManager();
            for (int i = 0; i < 10; i++) {
                TcpListener fileReceiverListener = new TcpListener(5105 + ((i + 1) * 100));
                fileReceiverListener.Start();
                Thread thread = new Thread(new ThreadStart(delegate {
                    while (true) {
                        TcpClient client = fileReceiverListener.AcceptTcpClient();
                        client.LingerState = new LingerOption(false, 5);
                        lock (fileManager) {
                            ReceiveFromClientManager receiveFromClientManager = new ReceiveFromClientManager(client, fileManager);
                            Thread thread = new Thread(new ThreadStart(receiveFromClientManager.ReceiveFromClient));
                            thread.Start();
                        }
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