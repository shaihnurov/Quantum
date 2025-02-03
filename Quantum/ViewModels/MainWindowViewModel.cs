using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Quantum.Service;

namespace Quantum.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly HubConnectionManager _hubConnectionManager;

    #region ObservableProperty
    [ObservableProperty]
    private object? _currentView;
    [ObservableProperty]
    private string? _currentNameView;
    [ObservableProperty]
    private string? _notificationMessage;
    [ObservableProperty]
    private string? _notificationTitleText;
    [ObservableProperty]
    private int _notificationStatus;
    [ObservableProperty]
    private bool _notificationVisible = false;
    [ObservableProperty]
    private bool _isEnableSettingsBtn = false;
    #endregion

    public MainWindowViewModel()
    {
        _hubConnectionManager = new HubConnectionManager();
        var authViewModel = new AuthViewModel(this, _hubConnectionManager);
        CurrentView = authViewModel;

        CurrentNameView = "Authentication";

        Task.Run(async () => await authViewModel.AutoLogin());
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

    [RelayCommand]
    private void LogOut()
    {
        CurrentNameView = "Authentication";
        CurrentView = new AuthViewModel(this, _hubConnectionManager);

        UserDataStorage.DeleteUserData();
    }
}