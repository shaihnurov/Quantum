using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.SignalR;
using Quantum.Service;
using WebAPI.Models.DTO;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Quantum.ViewModels
{
    public partial class DialogWindowViewModel : ObservableObject
    {
        private Window _window;
        private readonly MainWindowViewModel _mainWindowViewModel;
        private readonly HomeViewModel _homeViewModel1;
        private readonly HubConnectionManager _hubConnectionManager;
        private HubConnection? _chatHubConnection;
        private ChatDTO _chatDTO;

        #region ObservableProperty
        [ObservableProperty]
        private string? _chatName;
        [ObservableProperty]
        private ObservableCollection<UserDTO>? _usersList;
        #endregion

        public DialogWindowViewModel(Window window, MainWindowViewModel mainWindowViewModel, HomeViewModel homeViewModel, HubConnectionManager hubConnectionManager, ChatDTO chatDTO)
        {
            _window = window;
            _mainWindowViewModel = mainWindowViewModel;
            _homeViewModel1 = homeViewModel;
            _hubConnectionManager = hubConnectionManager;

            UsersList = [];
            _chatDTO = chatDTO;

            ChatName = chatDTO.Name!;
            LoadListUsers(chatDTO);

            Task.Run(async () => await InitializeConnectionAsync());
        }

        private async Task InitializeConnectionAsync()
        {
            // Получаем подключение к чату
            _chatHubConnection = await _hubConnectionManager.GetOrCreateConnectionAsync("loadUserChat", "https://localhost:7058/loadUserChat");

            _chatHubConnection?.On<string>("SuccReceive", async response =>
            {
                Dispatcher.UIThread.Post(() =>
                {
                    _window.Close();
                    _homeViewModel1.CurrentViewChat = null;
                });

                await _mainWindowViewModel.Notification("Quantum", response, true, 2, true);
            });

            _chatHubConnection?.On<string>("ErrorReceive", async response =>
            {
                Dispatcher.UIThread.Post(() =>
                {
                    _window.Close();
                });

                await _mainWindowViewModel.Notification("Quantum", response, true, 3, true);
            });
        }

        private void LoadListUsers(ChatDTO chatDTO)
        {
            foreach(var user in chatDTO.Users!)
            {
                UsersList!.Add(user);
            }
        }
        [RelayCommand]
        private async Task LeaveChat()
        {
            await _chatHubConnection!.InvokeAsync("LeaveChat", _chatDTO.Id);
        }
    }
}