using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Quantum.Service;

namespace Quantum.ViewModels;

public class MainWindowViewModel : ObservableObject
{
    private object? _currentView;
    private string? _currentNameView;

    private string? _notificationMessage;
    private string? _notificationTitleText;
    private int _notificationStatus;
    private bool _notificationVisible = false;
    
    public object? CurrentView
    {
        get => _currentView;
        set
        {
            SetProperty(ref _currentView, value);

            if (_currentView is IServerConnectionHandler newServerConnectionHandler)
                newServerConnectionHandler.ConnectServer();
        }
    }
    public string? CurrentNameView
    {
        get => _currentNameView;
        set => SetProperty(ref _currentNameView, value);
    }
    public string? NotificationMessage
    {
        get => _notificationMessage;
        set => SetProperty(ref _notificationMessage, value);
    }
    public string? NotificationTitleText
    {
        get => _notificationTitleText;
        set => SetProperty(ref _notificationTitleText, value);
    }
    public int NotificationStatus
    {
        get => _notificationStatus;
        set => SetProperty(ref _notificationStatus, value);
    }
    public bool NotificationVisible
    {
        get => _notificationVisible;
        set => SetProperty(ref _notificationVisible, value);
    }
    
    public MainWindowViewModel()
    {
        CurrentNameView = "Authentication";
        CurrentView = new AuthViewModel(this);
    }
    
    public async Task Notification(string title, string message, bool visibleInfoBar, int statusCode, bool timeLife)
    {
        NotificationTitleText = title;
        NotificationMessage = message;
        NotificationVisible = visibleInfoBar;
        NotificationStatus = statusCode;

        if (timeLife)
        {
            await Task.Delay(3000);
            NotificationVisible = false;
        }
    }
}