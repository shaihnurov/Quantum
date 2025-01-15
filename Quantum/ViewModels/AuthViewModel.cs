using System;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Serilog;

namespace Quantum.ViewModels;

public class AuthViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _mainWindowViewModel;
    
    private string? _email;
    private string? _password;
    private bool _isRememberMe;

    public string? Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
    }
    public string? Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }
    public bool IsRememberMe
    {
        get => _isRememberMe;
        set => SetProperty(ref _isRememberMe, value);
    }

    public RelayCommand RegisterCommand { get; set; }
    public AsyncRelayCommand AuthCommand { get; set; }

    public AuthViewModel(MainWindowViewModel mainWindowViewModel) : base("auth")
    {
        _mainWindowViewModel = mainWindowViewModel;
        
        RegisterCommand = new RelayCommand(() => {
            mainWindowViewModel.CurrentView = new RegisterViewModel(mainWindowViewModel);
        });
        AuthCommand = new AsyncRelayCommand(AuthUser);
    }
    
    public override async Task ConnectServer()
    {
        try
        {
            await base.ConnectServer();
            
            _hubConnection.On<string>("AuthSuccess", async response => {
                Dispatcher.UIThread.Post(() =>
                { 
                    _mainWindowViewModel.CurrentView = new HomeViewModel();
                });
                await _mainWindowViewModel.Notification("Auth", response, true, 1, true);
            });
            
            _hubConnection.On<string>("AuthError", async response =>
            {
                await _mainWindowViewModel.Notification("Auth", response, true, 3, true);
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
    private async Task AuthUser()
    {
        await _hubConnection.InvokeAsync("AuthUser", Email, Password, IsRememberMe);
    }
}