using Communication;
using System;
using System.Net.Sockets;

namespace Client.Model
{
    public class ClientModel
    {
        private readonly int port;
        private readonly string host;
        private TcpClient? tcpClient;
        private NetworkStream? stream;
        private readonly Chat chat = new Chat(); 

        /// <summary>
        /// Name of the client
        /// </summary>
        public string? Name { get; set; }

        public ClientModel(string host, int port)
        {
            this.host = host;
            this.port = port;
        }

        /// <summary>
        /// Connect to the server
        /// </summary>
        /// <returns>True if connection is set otherwise false</returns>
        public bool Connect()
        {
            bool result;
            tcpClient = new TcpClient();

            try
            {
                tcpClient.Connect(host, port);
                stream = tcpClient.GetStream();
                result = SendMessage(Name);
            }
            catch
            {
                result = false;
            }

            return result;
        }

        /// <summary>
        /// Send a message
        /// </summary>
        /// <param name="message">Message to send</param>
        /// <returns>True if no error otherwise false</returns>
        public bool SendMessage(string? message)
        {
            if(message == null)
            {
                return false;
            }

            bool result;
            try
            {
                chat.SendMessage(stream, message);
                result = true;
            }
            catch
            {
                result = false;
                Disconnect();
            }

            return result;
        }

        /// <summary>
        /// Receive a message from the server
        /// </summary>
        /// <returns>Received message</returns>
        public string ReceiveMessage(out bool isSuccess)
        {
            string msg;
            try
            {
                msg = chat.ReceiveMessage(stream);

                if (msg.Equals(Chat.STOP_CODE))
                {
                    msg = $"[{DateTime.Now}] Server: Disconnected";
                    isSuccess = false;
                    Disconnect();
                }
                else
                {
                    isSuccess = true;
                }
            }
            catch 
            {
                msg = $"[{DateTime.Now}] Server: Connection lost";
                isSuccess = false;
                Close();
            }

            return msg;
        }

        /// <summary>
        /// Send a stop message and close all connections
        /// </summary>
        public void Close()
        {
            if (stream != null)
            {
                SendMessage(Chat.STOP_CODE);
            }        

            Disconnect();
        }

        /// <summary>
        /// Close all connections
        /// </summary>
        public void Disconnect()
        {
            if (stream != null)
            {
                stream.Close();
            }

            if (tcpClient != null)
            {
                tcpClient.Close();
            }
        }
    }
}
