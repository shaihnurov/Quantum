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
using System.IdentityModel.Tokens.Jwt;

namespace Quantum.ViewModels;

public partial class AuthViewModel : ObservableObject
{
    private readonly HttpClient _httpClient;
    private readonly MainWindowViewModel _mainWindowViewModel;
    private readonly HubConnectionManager _hubConnectionManager;

    [ObservableProperty]
    private string? _email;
    [ObservableProperty]
    private string? _password;
    [ObservableProperty]
    private bool _isRememberMe;

    public AuthViewModel(MainWindowViewModel mainWindowViewModel, HubConnectionManager hubConnectionManager)
    {
        _mainWindowViewModel = mainWindowViewModel;
        _httpClient = new HttpClient { BaseAddress = new Uri("https://localhost:7058/") };
        _hubConnectionManager = hubConnectionManager;
    }

    [RelayCommand]
    public async Task Login()
    {
        var result = await SendRequestAsync("api/auth/login", new { Email, Password });

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

            await _mainWindowViewModel.Notification("Сервер", "Успешная авторизация", true, 1, true);
            Log.Information($"Token на клиенте авторизации - {tokenStr}");
        }
        else
        {
            await _mainWindowViewModel.Notification("Сервер", await result.Content.ReadAsStringAsync(), true, 3, true);
        }
    }
    [RelayCommand]
    public void FormRegister()
    {
        _mainWindowViewModel.CurrentView = new RegisterViewModel(_mainWindowViewModel, _hubConnectionManager);
    }
    private async Task<HttpResponseMessage> SendRequestAsync(string url, object data)
    {
        var json = JsonSerializer.Serialize(data);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        return await _httpClient.PostAsync(url, content);
    }
    public async Task AutoLogin()
    {
        var userData = await UserDataStorage.GetUserData();
        if (userData != null && !string.IsNullOrEmpty(userData.Token))
        {
            // Проверка на валидность токена
            if (IsTokenValid(userData.Token))
            {
                // Авторизуем пользователя с сохраненным токеном
                Log.Information("Найден сохраненный токен. Автоматическая авторизация...");

                _mainWindowViewModel.CurrentView = new HomeViewModel(_mainWindowViewModel, _hubConnectionManager);
                await _mainWindowViewModel.Notification("Сервер", "Автоматическая авторизация выполнена", true, 1, true);
            }
            else
            {
                // Если токен истек, очищаем данные и перенаправляем на экран авторизации
                UserDataStorage.DeleteUserData();
                _mainWindowViewModel.CurrentView = new AuthViewModel(_mainWindowViewModel, _hubConnectionManager);
                await _mainWindowViewModel.Notification("Ошибка", "Токен истек, пожалуйста, авторизуйтесь заново", true, 3, true);
            }
        }
        else
        {
            _mainWindowViewModel.CurrentView = new AuthViewModel(_mainWindowViewModel, _hubConnectionManager);
        }
    }
    public bool IsTokenValid(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var expiryDate = jwtToken.ValidTo;
            return expiryDate > DateTime.UtcNow;
        }
        catch
        {
            return false;
        }
    }

}