using Communication;
using System;
using System.ComponentModel;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

namespace ServerWPF.Model
{
    public class ClientModel : INotifyPropertyChanged
    {
        private readonly TcpClient tcpClient;
        private readonly Chat chat = new Chat();
        private readonly ServerModel server;

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Unique identifier of the client
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Name of the client. May not be unique
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// True if client is banned otherwise false
        /// </summary>

        private bool isBanned = false;
        public bool IsBanned
        {
            get => isBanned;

            set
            {
                isBanned = value;
                OnPropertyChanged(nameof(IsBanned));
            }
        }

        /// <summary>
        /// Network stream of this client
        /// </summary>
        public NetworkStream? Stream { get; private set; }

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
                server.OnGotError($"[{DateTime.Now}] {Id} : {ex.Message}");
            }
        }

        private void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private string GetMessageToSend(string message)
        {
            return $"[{DateTime.Now}] {Name}: {message}";
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
                            server.BroadcastMessage(GetMessageToSend("leave the chat"));
                            break;
                        }

                        if (!IsBanned)
                        {

                            if (msg != string.Empty)
                            {
                                server.BroadcastMessage(GetMessageToSend(msg));
                            }
                        }
                        else
                        {
                            chat.SendMessage(Stream, $"[{DateTime.Now}] You cannot write to this chat");
                        }
                    }
                    catch (Exception ex)
                    {
                        server.OnGotError($"[{DateTime.Now}] {Id} : {ex.Message}");
                        server.BroadcastMessage(GetMessageToSend("leave the chat"));
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
                server.AddClient(this);

                server.BroadcastMessage(GetMessageToSend("joined the chat"));
            }
            catch (Exception ex)
            {
                server.OnGotError($"[{DateTime.Now}] {Id} : {ex.Message}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Close all connections
        /// </summary>
        public void Close()
        {
            tcpClient?.Close();
            Stream?.Close();
        }
    }
}
