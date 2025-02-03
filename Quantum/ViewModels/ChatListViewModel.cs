using Microsoft.AspNetCore.SignalR.Client;
using Serilog;
using System.Threading.Tasks;
using Avalonia.Threading;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Quantum.Service;
using CommunityToolkit.Mvvm.Input;
using ActiproSoftware.UI.Avalonia.Controls;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using WebAPI.Models.DTO;

namespace Quantum.ViewModels
{
    public partial class ChatListViewModel : ObservableObject
    {
        private readonly MainWindowViewModel _mainWindowViewModel;
        private readonly HomeViewModel _homeViewModel;
        private readonly HubConnectionManager _hubConnectionManager;

        private HubConnection? _chatHubConnection;

        [ObservableProperty]
        private ObservableCollection<ChatModel>? _chatsList = [];
        
        public ChatListViewModel(MainWindowViewModel mainWindowViewModel, HomeViewModel homeViewModel, HubConnectionManager hubConnectionManager)
        {
            _mainWindowViewModel = mainWindowViewModel;
            _homeViewModel = homeViewModel;
            _hubConnectionManager = hubConnectionManager;

            Task.Run(async () => await InitializeConnectionAsync());
        }

        private async Task InitializeConnectionAsync()
        {
            // Получаем подключение к чату
            _chatHubConnection = await _hubConnectionManager.GetOrCreateConnectionAsync("loadUserChat", "https://localhost:7058/loadUserChat");

            //Обновление списка чатов
            _chatHubConnection?.On<int, int>("UpdateUserChat", async (userId, chatId) => {
                await _chatHubConnection!.InvokeAsync("UpdateChatList", userId, chatId);
            });

            //Удаление чата
            _chatHubConnection?.On<int, int>("DeleteUserChat", (userId, chatId) => {
                var chatToRemove = ChatsList?.FirstOrDefault(chat => chat.Id == chatId);
                if (chatToRemove != null)
                {
                    ChatsList?.Remove(chatToRemove);
                    Log.Information($"Chat with ID {chatId} removed from the list for user {userId}");
                }
                else
                {
                    Log.Warning($"Chat with ID {chatId} not found in the list for user {userId}");
                }
            });

            //Получение чатов
            _chatHubConnection?.On<IEnumerable<ChatModel>>("ReceiveUserChat", response => {
                Dispatcher.UIThread.Post(() =>
                {
                    foreach (var chatModel in response)
                    {
                        ChatsList?.Add(chatModel);
                    }
                });
            });

            //Подгрузка чата
            _chatHubConnection?.On<ChatDTO>("ReceiveDataChat", chatData =>
            {
                Dispatcher.UIThread.Post(() =>
                {
                    if (chatData != null)
                    {
                        _homeViewModel.CurrentViewChat = new ChatActiveViewModel(_mainWindowViewModel, _homeViewModel, _hubConnectionManager, chatData);
                    }
                    else
                    {
                        MessageBox.Show("Ошибка: чат не найден.");
                    }
                });
            });

            await _chatHubConnection!.InvokeAsync("SearchUserChats");
        }

        [RelayCommand]
        private async Task SelectedChat(object parameter)
        {
            if(parameter is ChatModel currentChat)
            {
                await _chatHubConnection!.InvokeAsync("GetDataChat", currentChat.Id);
            }
        }
    }
}