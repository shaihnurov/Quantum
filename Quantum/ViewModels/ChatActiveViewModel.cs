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
using Server.Model.DTO;
using ActiproSoftware.UI.Avalonia.Controls;
using Tmds.DBus.Protocol;
using System.Linq;

namespace Quantum.ViewModels
{
    public class ChatActiveViewModel : ViewModelBase
    {
        private readonly MainWindowViewModel _mainWindowViewModel;

        private bool _chatvisible = false;
        private bool _titleTextChat = true;
        private string? _name;
        private string? _chatName;
        private int _chatId;
        private string? _messageTime;
        private string? _message = string.Empty;
        public ObservableCollection<string>? _listMessage;

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
        public string? Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }
        public string? ChatName
        {
            get => _chatName;
            set => SetProperty(ref _chatName, value);
        }
        public string? MessageTime
        {
            get => _messageTime;
            set => SetProperty(ref _messageTime, value);
        }
        public string? Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }
        public ObservableCollection<string>? ListMessage
        {
            get => _listMessage;
            set => SetProperty(ref _listMessage, value);
        }

        public AsyncRelayCommand SendMessageCommand { get; set; }

        public ChatActiveViewModel(MainWindowViewModel mainWindowViewModel, ChatDTO chatData) : base("chathub")
        {
            _mainWindowViewModel = mainWindowViewModel;
            ListMessage = [];
            LoadChat(chatData);

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
                        var messageText = $"{userName}: {message}";
                        MessageTime = TimeOnly.FromDateTime(DateTime.Now).ToString("HH:mm");
                        ListMessage.Add(messageText);
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
                await _hubConnection!.InvokeAsync("SendMessage", userData.Name, Message, _chatId, userData.Id);
                Message = string.Empty;
            }
        }
        private void LoadChat(ChatDTO chatData)
        {
            ChatVisible = true;
            TitleTextChat = false;
            ListMessage?.Clear();

            ChatName = chatData.Name!;

            foreach (var message in chatData.Messages!)
            {
                _chatId = message.ChatId;
                var user = chatData.Users!.FirstOrDefault(u => u.Id == message.UserId);
                if (user != null)
                {
                    var messageText = $"{user.Name}: {message.Content}";
                    MessageTime = message.Timestamp.ToString("HH:mm");
                    ListMessage?.Add(messageText);
                }
            }
        }
    }
}