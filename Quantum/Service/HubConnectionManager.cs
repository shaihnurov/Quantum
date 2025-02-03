using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Quantum.ViewModels;
using Serilog;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Quantum.Service
{
    public class HubConnectionManager
    {
        private readonly Dictionary<string, HubConnection> _hubConnections = [];

        // Метод для создания и подключения к хабу
        public async Task<HubConnection> GetOrCreateConnectionAsync(string hubName, string url)
        {
            try
            {
                if (_hubConnections.ContainsKey(hubName))
                {
                    // Если подключение уже существует, возвращаем его
                    Log.Information($"Подключение к хабу {hubName} уже существует.");
                    return _hubConnections[hubName];
                }

                var dataUser = await UserDataStorage.GetUserData();

                // Создаем новое подключение
                var hubConnection = new HubConnectionBuilder()
                    .WithUrl(url, options =>
                    {
                        options.AccessTokenProvider = () => Task.FromResult(dataUser.Token);
                    })
                    .Build();

                // Сохраняем подключение в словарь
                _hubConnections[hubName] = hubConnection;

                Log.Information($"Создание подключения к хабу {hubName}...");

                await hubConnection.StartAsync();

                return hubConnection;
            }
            catch (HttpRequestException ex)
            {
                Log.Error(ex, "Не удалось подключиться к серверу. ", ex.Message);
                throw;
            }
            catch (SocketException ex)
            {
                Log.Error(ex, "Сетевая ошибка при подключении к серверу. ", ex.Message);
                throw;
            }
            catch (HubException ex)
            {
                Log.Error(ex, "Ошибка подключения к SignalR хабу. ", ex.Message);
                throw;
            }
            catch (InvalidOperationException ex)
            {
                Log.Error(ex, "Ошибка в приложении. ", ex.Message);
                throw;
            }
            catch (TimeoutException ex)
            {
                Log.Error(ex, "Соединение с сервером прервано. ", ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Произошла непредвиденная ошибка. ", ex.Message);
                throw;
            }
        }

        // Метод для отключения всех хабов
        public async Task DisconnectAllAsync()
        {
            foreach (var connection in _hubConnections.Values)
            {
                await connection.StopAsync();
                await connection.DisposeAsync();
            }

            _hubConnections.Clear();
        }
    }
}