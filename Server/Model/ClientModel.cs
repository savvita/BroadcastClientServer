using Communication;
using System;
using System.Net.Sockets;

namespace Server.Model
{
    public class ClientModel
    {
        private readonly TcpClient tcpClient;
        private readonly Chat chat;
        private ServerModel server;

        /// <summary>
        /// Unique identifier of the client
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Name of the client. May not be unique
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// True if client is banned otherwise false
        /// </summary>
        public bool IsBanned { get; set; } = false;

        /// <summary>
        /// Network stream of this client
        /// </summary>
        public NetworkStream Stream { get; private set; }

        public ClientModel(TcpClient tcpClient, ServerModel server)
        {
            Id = Guid.NewGuid().ToString();

            this.tcpClient = tcpClient;
            this.server = server;

            try
            {
                Stream = tcpClient.GetStream();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now}] {Id} : {ex.Message}");
            }

            chat = new Chat();

            server.AddClient(this);
        }

        /// <summary>
        /// Chating
        /// </summary>
        public void Proceed()
        {
            string msg;

            if (EnterChat())
            {
                do
                {
                    try
                    {
                        msg = chat.ReceiveMessage(Stream);

                        if (msg.Equals(Chat.STOP_CODE))
                        {
                            SendMessage($"[{DateTime.Now}] {Name} leave the chat");
                            break;
                        }

                        if (!IsBanned)
                        {

                            if (msg != string.Empty)
                            {
                                SendMessage($"[{DateTime.Now}] {Name} : {msg}");
                            }
                        }
                        else
                        {
                            chat.SendMessage(Stream, $"[{DateTime.Now}] You cannot write to this chat");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[{DateTime.Now}] {Id} : {ex.Message}");
                        SendMessage($"[{DateTime.Now}] {Name} leave the chat");
                        break;
                    }
                } while (true);

                server.RemoveClient(Id);

                Close();
            }
        }


        /// <summary>
        /// Enter the chat, get a name
        /// </summary>
        /// <returns>True if no error during connection otherwise false</returns>
        private bool EnterChat()
        {
            try
            {
                Name = chat.ReceiveMessage(Stream);

                SendMessage($"[{DateTime.Now.ToString()}] {Name} joined the chat");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now.ToString()}] {Id} : {ex.Message}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Send message to all users and display it at the server
        /// </summary>
        /// <param name="message">Message to send</param>
        private void SendMessage(string message)
        {
            server.BroadcastMessage(message, Id);
            Console.WriteLine(message);
        }

        /// <summary>
        /// Close all connections
        /// </summary>
        public void Close()
        {
            tcpClient.Close();
            Stream.Close();
        }
    }
}
