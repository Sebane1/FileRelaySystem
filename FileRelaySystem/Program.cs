using RelayServerProtocol.Managers;
using System.Net;
using System.Net.Sockets;

namespace FileSystemRelay
{
    internal class Program
    {
        static List<TcpListener> fileReceiverListeners = new List<TcpListener>();
        static void Main(string[] args)
        {
            Console.WriteLine("Starting server");
            TemporaryFileManager fileManager = new TemporaryFileManager();
            ServerAccessManager serverAccessManager = new ServerAccessManager();
            HttpListener fileReceiverListener = new HttpListener();
            if (!serverAccessManager.DataManager.CheckIfPasswordExists())
            {
                bool passwordSetSucceeded = false;
                while (!passwordSetSucceeded)
                {
                    Console.WriteLine("No master password set. Please make one for connecting from client.");
                    passwordSetSucceeded = serverAccessManager.DataManager.SetMasterPassword(Console.ReadLine());
                    if (!passwordSetSucceeded)
                    {
                        Console.WriteLine("Password cannot be empty!");
                    }
                    else
                    {
                        Console.WriteLine("Password set! Do not share it with others.");
                    }
                }
            }
            fileReceiverListener.Prefixes.Add("http://10.0.0.21:5105/");
            fileReceiverListener.Prefixes.Add("http://localhost:5105/");
            fileReceiverListener.Start();
            Thread thread = new Thread(new ThreadStart(delegate
            {
                while (true)
                {
                    HttpListenerContext client = fileReceiverListener.GetContext();
                    lock (fileManager)
                    {
                        ReceiveFromClientManager receiveFromClientManager = new ReceiveFromClientManager(client, fileManager, serverAccessManager);
                        Task.Run(receiveFromClientManager.ReceiveFromClient);
                    }
                }
            }));
            thread.Start();
            Console.WriteLine("Server started");
            while (true)
            {
                Thread.Sleep(10000);
            }
        }
    }
}
