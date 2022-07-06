using Client.Model;
using GalaSoft.MvvmLight.Command;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Client.ViewModel
{
    public class ClientViewModel : INotifyPropertyChanged
    {
        private readonly ClientModel client;

        //TODO do not keep it at memory! Save and read from file

        /// <summary>
        /// All messages at the chat at current session
        /// </summary>
        private string chatText;

        public string ChatText
        {
            get => chatText;
            set
            {
                chatText = value;
                OnPropertyChanged(nameof(ChatText));
            }
        }

        /// <summary>
        /// Name of the client
        /// </summary>
        public string? Name
        {
            get => client.Name;
            set
            {
                client.Name = value;
            }
        }

        /// <summary>
        /// True if connection is set otherwise false
        /// </summary>
        private bool isConnected = false;

        public bool IsConnected
        {
            get => isConnected;
            set
            {
                isConnected = value;
                OnPropertyChanged(nameof(IsConnected));
            }
        }

        /// <summary>
        /// True if connection is NOT set otherwise false
        /// </summary>
        private bool isConnectPossible = true;
        public bool IsConnectPossible
        {
            get => isConnectPossible;
            set
            {
                isConnectPossible = value;
                OnPropertyChanged(nameof(IsConnectPossible));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public ClientViewModel()
        {
            client = new ClientModel("127.0.0.1", 8008);
            chatText = String.Empty;
        }

        /// <summary>
        /// Connect to the server
        /// </summary>
        private void Connect()
        {
            if (Name == String.Empty)
            {
                AppendLine("Enter your name");
                return;
            }

            bool result = client.Connect();

            if (result)
            {
                AppendLine($"[{DateTime.Now.ToString()}] Connected");
                Thread thread = new Thread(ReceivingMessages);
                thread.Start();
                IsConnected = true;
                IsConnectPossible = false;
            }
            else
            {
                AppendLine($"[{DateTime.Now.ToString()}] Cannot connect to the server");
            }
        }

        /// <summary>
        /// Receiving messages unless the connection is not failed or stopped
        /// </summary>
        private void ReceivingMessages()
        {
            bool isSuccess;
            do
            {
                string msg = client.ReceiveMessage(out isSuccess);
                AppendLine(msg);

                if (!isSuccess)
                {
                    IsConnected = false;
                    IsConnectPossible = true;
                    break;
                }
            } while (true);
        }

        /// <summary>
        /// Append new line with text to the chat
        /// </summary>
        /// <param name="text">Text to append</param>
        private void AppendLine(string text)
        {
            ChatText += text + Environment.NewLine;
        }

        private void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #region Commands
        private readonly RelayCommand? connectCommand;
        private readonly RelayCommand<object>? sendCommand;
        private readonly RelayCommand? closeCommand;

        public RelayCommand ConnectCommand
        {
            get => connectCommand ?? new RelayCommand(() => Connect());
        }

        public RelayCommand<object> SendCommand
        {
            get => sendCommand ?? new RelayCommand<object>((obj) => client.SendMessage(obj as string));
        }

        public RelayCommand CloseCommand
        {
            get => closeCommand ?? new RelayCommand(() =>
            {
                client.Close();
                Environment.Exit(0);
            });
        } 
        #endregion

    }
}
