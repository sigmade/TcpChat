using System;
using System.Net.Sockets;
using System.Text;

namespace Server
{
    internal class Client
    {
        public string Id { get; private set; }
        public string UserName { get; private set; }
        public NetworkStream Stream { get; private set; }

        private readonly TcpClient _client;
        private readonly ServerHandler _server;

        public Client(
            TcpClient tcpClient,
            ServerHandler serverHandler)
        {
            Id = Guid.NewGuid().ToString();
            _client = tcpClient;
            _server = serverHandler;

            Stream = _client.GetStream();
            UserName = GetMessage();

            _server.AddConnection(this);
        }

        public void Process()
        {
            try
            {
                string message;
                // в бесконечном цикле получаем сообщения от клиента
                while (true)
                {
                    try
                    {
                        message = GetMessage();
                        message = String.Format("{0}: {1}", UserName, message);
                        Console.WriteLine(message);
                        _server.SendAllExceptSender(message, Id);
                    }
                    catch
                    {
                        message = String.Format("{0}: покинул чат", UserName);
                        Console.WriteLine(message);
                        _server.SendAllExceptSender(message, Id);
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                // в случае выхода из цикла закрываем ресурсы
                _server.RemoveConnection(Id);
                Close();
            }
        }

        // чтение входящего сообщения и преобразование в строку
        private string GetMessage()
        {
            var data = new byte[64]; // буфер для получаемых данных
            var builder = new StringBuilder();
            int bytes = 0;
            do
            {
                bytes = Stream.Read(data, 0, data.Length);
                builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
            }
            while (Stream.DataAvailable);

            return builder.ToString();
        }

        // закрытие подключения
        protected internal void Close()
        {
            if (Stream != null)
                Stream.Close();
            if (_client != null)
                _client.Close();
        }
    }
}