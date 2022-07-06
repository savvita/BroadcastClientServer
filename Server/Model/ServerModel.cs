using Communication;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Server.Model
{
    public class ServerModel
    {
        private TcpListener listener;
        private readonly int port;
        private readonly List<ClientModel> clients = new List<ClientModel>();
        private readonly Chat chat = new Chat();

        private const string clientsFile = "ip.txt";

        public ServerModel(int port = 8008)
        {
            this.port = port;
            if (!File.Exists(clientsFile))
            {
                File.Create(clientsFile);
            }
        }

        /// <summary>
        /// Listen the port
        /// </summary>
        public void Listen()
        {
            try
            {
                listener = new TcpListener(IPAddress.Any, port);
                listener.Start();
                Console.WriteLine($"[{DateTime.Now}] Server started");

                do
                {
                    TcpClient tcpClient = listener.AcceptTcpClient();

                    string ips = File.ReadAllText(clientsFile);

                    if (!ips.Contains(tcpClient.Client.RemoteEndPoint.ToString()))
                    {
                        File.AppendAllText(clientsFile, tcpClient.Client.RemoteEndPoint.ToString() + Environment.NewLine);
                    }

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

        // TODO add time for ban

        /// <summary>
        /// Ban the client
        /// </summary>
        /// <param name="id">Id of client to ban</param>
        public void Ban(string id)
        {
            ClientModel client = clients.Where(x => x.Id.Equals(id)).FirstOrDefault();

            if (client != null && !client.IsBanned)
            {
                client.IsBanned = true;
                string msg = $"[{DateTime.Now}] Server: {client.Name} is banned";
                BroadcastMessage(msg, "");

                Console.WriteLine(msg);
            }
        }

        //TODO add Unbanning

        /// <summary>
        /// Add new client model to the list of active clients
        /// </summary>
        /// <param name="client">Client to add</param>
        public void AddClient(ClientModel client)
        {
            clients.Add(client);
        }

        /// <summary>
        /// Remove client from the list
        /// </summary>
        /// <param name="id">Id of client to remove</param>
        public void RemoveClient(string id)
        {
            ClientModel client = clients.Where(x => x.Id.Equals(id)).FirstOrDefault();

            if (client != null)
            {
                clients.Remove(client);
            }
        }

        /// <summary>
        /// Send message to all clients
        /// </summary>
        /// <param name="message">Message to send</param>
        /// <param name="senderId">ID of client that send the message</param>
        public void BroadcastMessage(string message, string senderId)
        {
            for (int i = 0; i < clients.Count; i++)
            {
                try
                {
                    chat.SendMessage(clients[i].Stream, message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{DateTime.Now}] {clients[i].Id} : {ex.Message}");
                }

                //if (!clients[i].Id.Equals(senderId))
                //{
                //    try
                //    {
                //        chat.SendMessage(clients[i].Stream, message);
                //    }
                //    catch (Exception ex)
                //    {
                //        Console.WriteLine($"[{DateTime.Now.ToString()}] {clients[i].Id} : {ex.Message}");
                //    }
                //}
            }
        }

        /// <summary>
        /// Close all connections
        /// </summary>
        public void Close()
        {
            for (int i = 0; i < clients.Count; i++)
            {
                clients[i].Close();
            }

            listener.Stop();

            Console.WriteLine($"[{DateTime.Now}] Server stopped");
        }
    }
}
