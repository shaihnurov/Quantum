using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using Quantum.Service;
using WebAPI.Models.DTO;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Quantum.ViewModels
{
    public partial class ChatActiveViewModel : ObservableObject
    {
        private readonly IDialogService _dialogService;
        private readonly MainWindowViewModel _mainWindowViewModel;
        private readonly HomeViewModel _homeViewModel;
        private readonly HubConnectionManager _hubConnectionManager;
        private HubConnection? _chatHubConnection;
        private ChatDTO _chatDTO;

        #region ObservableProperty
        [ObservableProperty]
        private bool _chatVisible = false;
        [ObservableProperty]
        private bool _titleTextChat = true;
        [ObservableProperty]
        private string? _name;
        [ObservableProperty]
        private string? _chatName;
        [ObservableProperty]
        private int _chatId;
        [ObservableProperty]
        private string? _messageTime;
        [ObservableProperty]
        private string? _message = string.Empty;
        [ObservableProperty]
        private ObservableCollection<MessageDTO>? _listMessage;
        #endregion

        public ChatActiveViewModel(MainWindowViewModel mainWindowViewModel, HomeViewModel homeViewModel, HubConnectionManager hubConnectionManager, ChatDTO chatData)
        {
            _dialogService = new DialogService();
            _mainWindowViewModel = mainWindowViewModel;
            _homeViewModel = homeViewModel;
            _hubConnectionManager = hubConnectionManager;

            ListMessage = [];
            _chatDTO = chatData;
            LoadChat(chatData);
            Task.Run(async () => await InitializeConnectionAsync());
        }

        private async Task InitializeConnectionAsync()
        {
            // Получаем подключение к чату
            _chatHubConnection = await _hubConnectionManager.GetOrCreateConnectionAsync("chathub", "https://localhost:7058/chathub");

            _chatHubConnection?.On<string, string, DateTime>("ReceiveMessage", (userName, message, timestamp) =>
            {
                Dispatcher.UIThread.Post(() =>
                {
                    var localTime = TimeZoneInfo.ConvertTimeFromUtc(timestamp, TimeZoneInfo.Local);
                    var messageText = $"{message}";
                    Name = userName;
                    ListMessage?.Add(new MessageDTO { Content = messageText, Timestamp = localTime });
                });
            });

            _chatHubConnection?.On<string>("ReceiveMessageError", async response =>
            {
                await _mainWindowViewModel.Notification("Send", response, true, 3, true);
            });
        }
        private void LoadChat(ChatDTO chatData)
        {
            ChatVisible = true;
            TitleTextChat = false;
            _chatDTO = chatData;
            ListMessage?.Clear();

            ChatName = _chatDTO.Name!;
            ChatId = _chatDTO.Id;

            foreach (var message in _chatDTO.Messages!)
            {
                var user = _chatDTO.Users!.FirstOrDefault(u => u.Id == message.UserId);
                if (user != null)
                {
                    var messageText = $"{message.Content}";
                    var localTime = TimeZoneInfo.ConvertTimeFromUtc(message.Timestamp, TimeZoneInfo.Local);
                    Name = user.Name;
                    ListMessage?.Add(new MessageDTO { Content = messageText, Timestamp = localTime });
                }
            }
        }
        [RelayCommand]
        private async Task SendMessage()
        {
            if (!string.IsNullOrWhiteSpace(Message))
            {
                var userData = await UserDataStorage.GetUserData();
                ChatVisible = true;
                TitleTextChat = false;
                await _chatHubConnection!.InvokeAsync("SendMessage", Message, ChatId);
                Message = string.Empty;
            }
        }
        [RelayCommand]
        private async Task OpenSettings()
        {
            await _dialogService.ShowDialogAsync(_mainWindowViewModel, _homeViewModel, _hubConnectionManager, _chatDTO);
        }
    }
}