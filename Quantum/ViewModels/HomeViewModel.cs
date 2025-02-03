using CommunityToolkit.Mvvm.ComponentModel;
using Quantum.Service;

namespace Quantum.ViewModels;

public partial class HomeViewModel : ObservableObject
{
    private readonly MainWindowViewModel _mainWindowViewModel;
    private readonly HubConnectionManager _hubConnectionManager;

    [ObservableProperty]
    private object? _currentViewChat;
    [ObservableProperty]
    private object? _currentViewChatList;

    public HomeViewModel(MainWindowViewModel mainWindowViewModel, HubConnectionManager hubConnectionManager)
    {
        _mainWindowViewModel = mainWindowViewModel;
        _hubConnectionManager = hubConnectionManager;
        _mainWindowViewModel.IsEnableSettingsBtn = true;
        _mainWindowViewModel.CurrentNameView = "Chats";

        CurrentViewChatList = new ChatListViewModel(_mainWindowViewModel, this, _hubConnectionManager);
    }
}