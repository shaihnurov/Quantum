using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.SignalR;
using Serilog;
using System;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using Quantum.Service;

namespace Quantum.ViewModels
{
    public class ChatActiveViewModel : ViewModelBase
    {
        private readonly MainWindowViewModel _mainWindowViewModel;

        private ObservableCollection<string> _messagesList;
        private bool _chatvisible = false;
        private bool _titleTextChat = true;
        private string _name;
        private string _message = string.Empty;

        public ObservableCollection<string> MessagesList
        {
            get => _messagesList;
            set => SetProperty(ref _messagesList, value);
        }
        public bool ChatVisible
        {
            get => _chatvisible;
            set => SetProperty(ref _chatvisible, value);
        }
        public bool TitleTextChat
        {
            get => _titleTextChat;
            set => SetProperty(ref _titleTextChat, value);
        }
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }
        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }

        public AsyncRelayCommand SendMessageCommand { get; set; }

        public ChatActiveViewModel(MainWindowViewModel mainWindowViewModel) : base("chathub")
        {
            _mainWindowViewModel = mainWindowViewModel;
            MessagesList = [];

            SendMessageCommand = new AsyncRelayCommand(SendMessage);
        }

        public override async Task ConnectServer()
        {
            try
            {
                await base.ConnectServer();

                _hubConnection?.On<string, string>("ReceiveMessage", (userName, message) => {
                    Dispatcher.UIThread.Post(() =>
                    {
                        var newMessage = $"{userName}: {message}";
                        MessagesList.Add(newMessage);
                    });
                });

                _hubConnection?.On<string>("ReceiveMessageError", async response =>
                {
                    await _mainWindowViewModel.Notification("Send", response, true, 3, true);
                });
            }
            catch (HttpRequestException ex)
            {
                Log.Error(ex, "Не удалось подключиться к серверу.");
                await _mainWindowViewModel.Notification("Server", "Failed to connect to the server", true, 3, true);
            }
            catch (SocketException ex)
            {
                Log.Error(ex, "Сетевая ошибка при подключении к серверу.");
                await _mainWindowViewModel.Notification("Network", "A network error occurred while connecting to the server", true, 3, true);
            }
            catch (HubException ex)
            {
                Log.Error(ex, "Ошибка подключения к SignalR хабу.");
                await _mainWindowViewModel.Notification("Server", "An error occurred when connecting the SignalR hub", true, 3, true);
            }
            catch (InvalidOperationException ex)
            {
                Log.Error(ex, "Ошибка в приложении.");
                await _mainWindowViewModel.Notification("Error", "An error has occurred in the application. Please try again", true, 3, true);
            }
            catch (TimeoutException ex)
            {
                Log.Error(ex, "Соединение с сервером прервано.");
                await _mainWindowViewModel.Notification("Timeout", "The connection to the server has been terminated. Please try again", true, 3, true);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Произошла непредвиденная ошибка.");
                await _mainWindowViewModel.Notification("Error", "There's been an unforeseen error", true, 3, true);
            }
        }
        private async Task SendMessage()
        {
            if(Message != string.Empty)
            {
                var userData = await UserDataStorage.GetUserData();
                ChatVisible = true;
                TitleTextChat = false;
                await _hubConnection!.InvokeAsync("SendMessage", userData.Name, Message);
                Message = string.Empty;
            }
        }
    }
}
