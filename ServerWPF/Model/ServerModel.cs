using Communication;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ServerWPF.Model
{
    public class ServerModel
    {
        private TcpListener? listener;
        private readonly int port;
        private readonly Chat chat = new Chat();

        private const string clientsFile = "ip.txt";

        /// <summary>
        /// List of clients connected to the server
        /// </summary>
        private readonly List<ClientModel> clients = new List<ClientModel>();
        public List<ClientModel> Clients
        {
            get => clients;
        }

        #region Events
        /// <summary>
        /// Raise when during the connection was an error
        /// </summary>
        public event Action<string>? GotError;

        /// <summary>
        /// Raise when the server is started to listening
        /// </summary>
        public Action<string>? ServerStarted;

        /// <summary>
        /// Raise when new client is connected to the server
        /// </summary>
        public Action<ClientModel>? ClientConnected;

        /// <summary>
        /// Raise when the client is disconnected
        /// </summary>
        public Action<ClientModel>? ClientDisconnected; 
        #endregion

        public ServerModel(int port = 8008)
        {
            this.port = port;

            if (!File.Exists(clientsFile))
            {
                File.Create(clientsFile);
            }
        }


        /// <summary>
        /// Listen to the port
        /// </summary>
        public void Listen()
        {
            try
            {
                listener = new TcpListener(IPAddress.Any, port);
                listener.Start();

                ServerStarted?.Invoke($"[{DateTime.Now}] Server started");

                do
                {
                    TcpClient tcpClient = listener.AcceptTcpClient();

                    AddIPToList(tcpClient);

                    ClientModel client = new ClientModel(tcpClient, this);

                    Thread thread = new Thread(client.Proceed);
                    thread.Start();

                } while (true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now}] Server: {ex.Message}");
                Close();
            }
        }

        /// <summary>
        /// Add IP of client to the list of all IPs that were connected to the server
        /// </summary>
        /// <param name="tcpClient">Client</param>
        private void AddIPToList(TcpClient tcpClient)
        {
            string ips = File.ReadAllText(clientsFile);

            string? ip = tcpClient.Client.RemoteEndPoint?.ToString();

            if (ip != null && !ips.Contains(ip))
            {
                File.AppendAllText(clientsFile, ip + Environment.NewLine);
            }
        }

 
        /// <summary>
        /// Send message from the server to the client
        /// </summary>
        /// <param name="id">Id of the client to send a message to</param>
        /// <param name="message">Message</param>
        public void SendMessage(string id, string? message)
        {
            try
            {
                ClientModel? client = GetClientById(id);

                if(client != null && message != null)
                {
                    chat.SendMessage(client.Stream, $"[{DateTime.Now}] Server: {message}");
                }
            }
            catch { }
        }

        // TODO add time for ban

        /// <summary>
        /// Ban the client
        /// </summary>
        /// <param name="id">Id of client to ban</param>
        public void Ban(string id)
        {
            ClientModel? client = GetClientById(id);

            if (client != null && !client.IsBanned)
            {
                client.IsBanned = true;
                BroadcastMessage($"[{DateTime.Now}] Server: {client.Name} is banned");
            }

        }

        /// <summary>
        /// Unban the client
        /// </summary>
        /// <param name="id">Id of client to unban</param>
        public void Unban(string id)
        {
            ClientModel? client = GetClientById(id);

            if (client != null && client.IsBanned)
            {
                client.IsBanned = false;
                BroadcastMessage($"[{DateTime.Now}] Server: {client.Name} is unbanned");
            }
        }

        /// <summary>
        /// Get a client by its Id
        /// </summary>
        /// <param name="id">Id of a client</param>
        /// <returns>Client with specified Id</returns>
        public ClientModel? GetClientById(string id)
        {
            return clients.Where(x => x.Id.Equals(id)).FirstOrDefault();
        }

        /// <summary>
        /// Add new client model to the list of active clients
        /// </summary>
        /// <param name="client">Client to add</param>
        public void AddClient(ClientModel client)
        {
            clients.Add(client);

            ClientConnected?.Invoke(client);
        }

        /// <summary>
        /// Remove client from the list
        /// </summary>
        /// <param name="id">Id of client to remove</param>
        public void RemoveClient(string id)
        {
            ClientModel? client = GetClientById(id);

            if (client != null)
            {
                clients.Remove(client);
                ClientDisconnected?.Invoke(client);
            }
        }

        /// <summary>
        /// Send message to all clients
        /// </summary>
        /// <param name="message">Message to send</param>
        public void BroadcastMessage(string message)
        {
            for (int i = 0; i < clients.Count; i++)
            {
                try
                {
                    chat.SendMessage(clients[i].Stream, message);
                }
                catch (Exception ex)
                {
                    OnGotError($"[{DateTime.Now}] {clients[i].Id} : {ex.Message}" + Environment.NewLine);
                }
            }         
        }

        /// <summary>
        /// Close all connections
        /// </summary>
        public void Close()
        {
            BroadcastMessage(Chat.STOP_CODE);
            for (int i = 0; i < clients.Count; i++)
            {
                clients[i].Close();
            }
            clients.Clear();
            listener?.Stop();
        }

        /// <summary>
        /// Raise an event GotError
        /// </summary>
        /// <param name="message">Error message</param>
        public void OnGotError(string message)
        {
            GotError?.Invoke(message);
        }
    }
}
