using System.Net.Sockets;

namespace FileSystemRelay
{
    internal class Program
    {
        static List<TcpListener> fileReceiverListeners = new List<TcpListener>();
        static void Main(string[] args)
        {
            Console.WriteLine("Starting server");
            FileManager fileManager = new FileManager();
            for (int i = 0; i < 50; i++)
            {
                try
                {
                    TcpListener fileReceiverListener = new TcpListener(5105 + ((i)));
                    fileReceiverListener.Start();
                    Thread thread = new Thread(new ThreadStart(delegate
                    {
                        while (true)
                        {
                            try
                            {
                                TcpClient client = fileReceiverListener.AcceptTcpClient();
                                client.LingerState = new LingerOption(false, 0);
                                lock (fileManager)
                                {
                                    ReceiveFromClientManager receiveFromClientManager = new ReceiveFromClientManager(client, fileManager);
                                    Thread thread = new Thread(new ThreadStart(receiveFromClientManager.ReceiveFromClient));
                                    thread.Start();
                                }
                            }
                            catch
                            {
                                break;
                            }
                        }
                    }));
                    thread.Start();
                    fileReceiverListeners.Add(fileReceiverListener);
                }
                catch { }
            }
            Console.WriteLine("Server started");
            while (true)
            {
                Thread.Sleep(10000);
            }
        }
    }
}