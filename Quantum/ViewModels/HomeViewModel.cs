using CommunityToolkit.Mvvm.ComponentModel;
using Quantum.Service;
using Quantum.Views;

namespace Quantum.ViewModels;

public class HomeViewModel : ObservableObject
{
    private readonly MainWindowViewModel _mainWindowViewModel;

    private object? _currentViewChat;
    private object? _currentViewChatList;

    public object? CurrentViewChat
    {
        get => _currentViewChat;
        set
        {
            SetProperty(ref _currentViewChat, value);

            if (_currentViewChat is IServerConnectionHandler newServerConnectionHandler)
                newServerConnectionHandler.ConnectServer();
        }
    }
    public object? CurrentViewChatList
    {
        get => _currentViewChatList;
        set
        {
            SetProperty(ref _currentViewChatList, value);

            if (_currentViewChatList is IServerConnectionHandler newServerConnectionHandler)
                newServerConnectionHandler.ConnectServer();
        }
    }

    public HomeViewModel(MainWindowViewModel mainWindowViewModel)
    {
        _mainWindowViewModel = mainWindowViewModel;

        CurrentViewChatList = new ChatListViewModel(_mainWindowViewModel, this);
    }
}