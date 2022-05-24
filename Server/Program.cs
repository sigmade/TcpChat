using System;
using System.Threading;

namespace Server
{
    internal class Program
    {
        private static ServerHandler server;
        private static Thread listenThread;

        private static void Main(string[] args)
        {
            try
            {
                server = new ServerHandler();
                listenThread = new Thread(new ThreadStart(server.Listen));
                listenThread.Start();
            }
            catch (Exception ex)
            {
                server.Disconnect();
                Console.WriteLine(ex.Message);
            }
        }
    }
}