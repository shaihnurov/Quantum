using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.SignalR;
using Serilog;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using System;
using Avalonia.Threading;
using Server.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Quantum.Service;
using CommunityToolkit.Mvvm.Input;
using ActiproSoftware.UI.Avalonia.Controls;
using Server.Model.DTO;

namespace Quantum.ViewModels
{
    public class ChatListViewModel : ViewModelBase
    {
        private readonly MainWindowViewModel _mainWindowViewModel;
        private readonly HomeViewModel _homeViewModel;

        private ObservableCollection<ChatModel>? _chatsList;
        
        public ObservableCollection<ChatModel>? ChatsList
        {
            get => _chatsList;
            set => SetProperty(ref _chatsList, value);
        }

        public AsyncRelayCommand<object> SelectedChatCommand { get; set; }

        public ChatListViewModel(MainWindowViewModel mainWindowViewModel, HomeViewModel homeViewModel) : base ("loaduserchat")
        {
            _mainWindowViewModel = mainWindowViewModel;
            _homeViewModel = homeViewModel;
            ChatsList = [];

            SelectedChatCommand = new AsyncRelayCommand<object>(SelectedChat);
        }

        public override async Task ConnectServer()
        {
            try
            {
                await base.ConnectServer();

                _hubConnection?.On<IEnumerable<ChatModel>>("ReceiveUserChat", response => {
                    Dispatcher.UIThread.Post(() =>
                    {
                        ChatsList?.Clear();
                        foreach (var chatModel in response)
                        {
                            ChatsList?.Add(chatModel);
                        }
                    });
                });

                _hubConnection?.On<ChatDTO>("ReceiveDataChat", chatData =>
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        if (chatData != null)
                        {
                            _homeViewModel.CurrentViewChat = new ChatActiveViewModel(_mainWindowViewModel, chatData);
                        }
                        else
                        {
                            MessageBox.Show("Ошибка: чата не найдено.");
                        }
                    });
                });

                var userData = await UserDataStorage.GetUserData();
                await _hubConnection!.InvokeAsync("SearchUserChats", userData);
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
        private async Task SelectedChat(object parameter)
        {
            if(parameter is ChatModel currentChat)
            {
                await _hubConnection!.InvokeAsync("GetDataChat", currentChat.Id);
            }
        }
    }
}