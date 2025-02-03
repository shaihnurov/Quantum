using System;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Quantum.Service;
using Serilog;
using Quantum.Models;

namespace Quantum.ViewModels;

public partial class RegisterViewModel : ObservableObject
{
    private readonly HttpClient _httpClient;
    private readonly MainWindowViewModel _mainWindowViewModel;
    private readonly HubConnectionManager _hubConnectionManager;

    #region ObservableProperty
    [ObservableProperty]
    private string? _email;
    [ObservableProperty]
    private string? _login;
    [ObservableProperty]
    private string? _name;
    [ObservableProperty]
    private string? _password;
    [ObservableProperty]
    private string? _confirmPassword;
    [ObservableProperty]
    private bool _isRememberMe;
    #endregion

    public RegisterViewModel(MainWindowViewModel mainWindowViewModel, HubConnectionManager hubConnectionManager)
    {
        _mainWindowViewModel = mainWindowViewModel;
        _httpClient = new HttpClient { BaseAddress = new Uri("https://localhost:7058/") };
        _hubConnectionManager = hubConnectionManager;
    }

    [RelayCommand]
    public async Task Register()
    {
        var result = await SendRequestAsync("api/auth/register", new { Email, Login, Name, Password });

        if (result.IsSuccessStatusCode)
        {
            var content = await result.Content.ReadAsStringAsync();

            var tokenObj = JsonSerializer.Deserialize<JsonElement>(content);
            var tokenStr = tokenObj.GetProperty("token").GetString();

            var userData = new UserDataJson
            {
                Token = tokenStr,
                IsRememberMe = IsRememberMe
            }; 
            await UserDataStorage.SaveUserData(userData);

            _mainWindowViewModel.CurrentView = new HomeViewModel(_mainWindowViewModel, _hubConnectionManager);

            await _mainWindowViewModel.Notification("Сервер", "Успешная регистрация", true, 1, true);
            Log.Information($"Token на клиенте регистрации - {tokenStr}");
        }
        else
        {
            await _mainWindowViewModel.Notification("Сервер", await result.Content.ReadAsStringAsync(), true, 3, true);
        }
    }
    [RelayCommand]
    public void FormAuth()
    {
        _mainWindowViewModel.CurrentView = new AuthViewModel(_mainWindowViewModel, _hubConnectionManager);
    }
    private async Task<HttpResponseMessage> SendRequestAsync(string url, object data)
    {
        var json = JsonSerializer.Serialize(data);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        return await _httpClient.PostAsync(url, content);
    }
}