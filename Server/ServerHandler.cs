using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Server
{
    internal class ServerHandler
    {
        private static TcpListener TcpListener;
        private List<Client> Clients = new();

        protected internal void AddConnection(Client clientObject)
        {
            Clients.Add(clientObject);
        }

        protected internal void RemoveConnection(string id)
        {
            var client = Clients.FirstOrDefault(c => c.Id == id);
            if (client != null)
                Clients.Remove(client);
        }

        protected internal void Listen()
        {
            try
            {
                TcpListener = new TcpListener(IPAddress.Any, 8888);
                TcpListener.Start();
                Console.WriteLine("Сервер запущен. Ожидание подключений...");

                while (true)
                {
                    var tcpClient = TcpListener.AcceptTcpClient();
                    var client = new Client(tcpClient, this);

                    if (CheckIsNewClient(client.UserName))
                    {
                        var hiMessage = $"{client.UserName} вошел в чат";
                        SendAllExceptSender(hiMessage, client.Id);
                        Console.WriteLine(hiMessage);

                        var clientThread = new Thread(new ThreadStart(client.Process));
                        clientThread.Start();
                    }
                    else
                    {
                        SendMessageByUsername("ExistLoginConnection", client.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Disconnect();
            }
        }

        private bool CheckIsNewClient(string userName)
        {
            return Clients
                .Where(c => c.UserName == userName)
                .Count() < 2;
        }

        protected internal void SendAllExceptSender(string message, string senderId)
        {
            var data = Encoding.Unicode.GetBytes(message);
            for (int i = 0; i < Clients.Count; i++)
            {
                if (Clients[i].Id != senderId)
                {
                    Clients[i].Stream.Write(data, 0, data.Length);
                }
            }
        }

        protected internal void SendMessageByUsername(string message, string userId)
        {
            var data = Encoding.Unicode.GetBytes(message);

            var client = Clients.FirstOrDefault(c => c.Id == userId);
            client.Stream.Write(data, 0, data.Length);
        }

        protected internal void Disconnect()
        {
            TcpListener.Stop();

            for (int i = 0; i < Clients.Count; i++)
            {
                Clients[i].Close();
            }
            Environment.Exit(0);
        }
    }
}