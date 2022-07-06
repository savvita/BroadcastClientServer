using GalaSoft.MvvmLight.Command;
using ServerWPF.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace ServerWPF.ViewModel
{
    public class ServerViewModel : INotifyPropertyChanged
    {
        private readonly ServerModel server;
        private readonly SynchronizationContext? context;

        /// <summary>
        /// List of clients currently connected to the server
        /// </summary>
        private ObservableCollection<ClientModel> clients;
        public ObservableCollection<ClientModel> Clients
        {
            get => clients;
            set
            {
                clients = value;
                OnPropertyChanged(nameof(Clients));
            }
        }

        /// <summary>
        /// Selected client at the list
        /// </summary>
        private ClientModel? selectedClient;
        public ClientModel? SelectedClient
        {
            get => selectedClient;
            set
            {
                selectedClient = value;
                OnPropertyChanged(nameof(SelectedClient));
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

        public ServerViewModel()
        {
            server = new ServerModel();
            clients = new ObservableCollection<ClientModel>(server.Clients);

            server.ClientConnected += ClientConnected;
            server.ClientDisconnected += ClientDisconnected;
            
            context = SynchronizationContext.Current;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #region Commands

        private readonly RelayCommand? startCommand;

        public RelayCommand StartCommand
        {
            get => startCommand ?? new RelayCommand(() =>
            {
                IsConnected = true;
                IsConnectPossible = false;
                Task.Factory.StartNew(server.Listen);
            });
        }

        private readonly RelayCommand? stopCommand;

        public RelayCommand StopCommand
        {
            get => stopCommand ?? new RelayCommand(() =>
            {
                IsConnected = false;
                IsConnectPossible = true;
                server.Close();
            });
        }

        private readonly RelayCommand? banCommand;
        public RelayCommand BanCommand
        {
            get => banCommand ?? new RelayCommand(() =>
            {
                if (SelectedClient != null)
                {
                    server.Ban(SelectedClient.Id);
                }
            });
        }

        private readonly RelayCommand? unbanCommand;
        public RelayCommand UnbanCommand
        {
            get => unbanCommand ?? new RelayCommand(() =>
            {
                if (SelectedClient != null)
                {
                    server.Unban(SelectedClient.Id);
                }
            });
        }

        private readonly RelayCommand<object>? sendMessageCommand;
        public RelayCommand<object> SendMessageCommand
        {
            get => sendMessageCommand ?? new RelayCommand<object>((obj) =>
            {
                if (SelectedClient != null)
                {
                    server.SendMessage(SelectedClient.Id, obj as string);
                }
            });
        } 
        #endregion

        private void ClientConnected(ClientModel client)
        {
            context?.Send(Add, client);
        }

        private void ClientDisconnected(ClientModel client)
        {
            context?.Send(Remove, client);
        }

        /// <summary>
        /// Add new client to the list
        /// </summary>
        /// <param name="obj">New client</param>
        private void Add(object? obj)
        {
            if (obj is ClientModel client)
            {
                Clients.Add(client);
            }
        }

        /// <summary>
        /// Remove client from the list
        /// </summary>
        /// <param name="obj">Client to remove</param>
        private void Remove(object? obj)
        {
            ClientModel? clientObj = obj as ClientModel;

            if (clientObj != null)
            {
                ClientModel? client = Clients.Where(x => x.Id.Equals(clientObj.Id)).FirstOrDefault();

                if (client != null)
                {
                    Clients.Remove(client);
                }
            }
        }

    }
}
