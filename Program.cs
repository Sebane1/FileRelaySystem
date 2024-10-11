using System.Net;
using System.Net.Sockets;

namespace FileSystemRelay {
    internal class Program {
        static List<TcpListener> fileReceiverListeners = new List<TcpListener>();
        static void Main(string[] args) {
            Console.WriteLine("Starting server");
            FileManager fileManager = new FileManager();
            HttpListener fileReceiverListener = new HttpListener();
            fileReceiverListener.Prefixes.Add("http://10.0.0.21:5105/");
            fileReceiverListener.Prefixes.Add("http://localhost:5105/");
            fileReceiverListener.Prefixes.Add("http://127.0.0.1:5105/");
            fileReceiverListener.Start();
            Thread thread = new Thread(new ThreadStart(delegate {
                while (true) {
                    HttpListenerContext client = fileReceiverListener.GetContext();
                    lock (fileManager) {
                        ReceiveFromClientManager receiveFromClientManager =
                        new ReceiveFromClientManager(client, fileManager);
                        Thread thread = new Thread(new ThreadStart(receiveFromClientManager.ReceiveFromClient));
                        thread.Start();
                    }
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