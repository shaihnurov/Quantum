using Microsoft.AspNetCore.SignalR;
using Npgsql;
using Serilog;
using System.Text.Json;
using WebAPI.Hubs;

namespace WebAPI.Service
{
    public class UserChatsNotifier
    {
        private readonly string _connectionString;
        private readonly IHubContext<LoadUserChat> _hubContext;
        private readonly UserConnectionsService _userConnectionsService;

        public UserChatsNotifier(IConfiguration connectionString, IHubContext<LoadUserChat> hubContext, UserConnectionsService userConnectionsService)
        {
            _connectionString = connectionString.GetConnectionString("DefaultConnection")!;
            _hubContext = hubContext;
            _userConnectionsService = userConnectionsService;
        }

        public async Task StartListening()
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            connection.Notification += async (sender, args) =>
            {
                var payload = args.Payload;

                try
                {
                    Log.Information($"Received notify: {payload}");

                    var notification = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(payload);

                    if (notification != null)
                    {
                        if (notification.ContainsKey("user_id") && notification.ContainsKey("chat_id") && notification.ContainsKey("operation"))
                        {
                            var userIdObj = notification["user_id"];
                            var chatIdObj = notification["chat_id"];
                            var operationObj = notification["operation"];

                            try
                            {
                                int userId = userIdObj.GetInt32();
                                int chatId = chatIdObj.GetInt32();
                                string operation = operationObj.GetString() ?? string.Empty;

                                // Поиск connectionId для userId через сервис
                                var connectionId = _userConnectionsService.GetConnectionIdByUserId(userId);

                                if (connectionId != null)
                                {
                                    switch (operation.ToUpper())
                                    {
                                        case "INSERT":
                                            await _hubContext.Clients.Client(connectionId).SendAsync("UpdateUserChat", userId, chatId);
                                            Console.WriteLine($"ConnectionID NOTIFIER - {connectionId}");
                                            Log.Information($"Chat added for user {userId}, chat {chatId}");
                                            break;
                                        case "UPDATE":
                                            await _hubContext.Clients.Client(connectionId).SendAsync("UpdateUserChat", userId, chatId);
                                            Console.WriteLine($"ConnectionID NOTIFIER - {connectionId}");
                                            Log.Information($"Chat updated for user {userId}, chat {chatId}");
                                            break;
                                        case "DELETE":
                                            await _hubContext.Clients.Client(connectionId).SendAsync("DeleteUserChat", userId, chatId);
                                            Console.WriteLine($"ConnectionID NOTIFIER - {connectionId}");
                                            Log.Information($"Chat deleted for user {userId}, chat {chatId}");
                                            break;
                                        default:
                                            Log.Warning($"Unknown operation: {operation}");
                                            break;
                                    }
                                }
                                else
                                {
                                    Log.Warning($"No connection found for user {userId}");
                                }
                            }
                            catch (InvalidOperationException ex)
                            {
                                Log.Error($"Error parsing JSON elements: {ex.Message}");
                            }
                        }
                        else
                        {
                            Log.Warning("user_id, chat_id or operation not found in the notification payload.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"Error processing notification: {ex.Message}");
                }
            };

            using var command = new NpgsqlCommand("LISTEN userchats_changes;", connection);
            await command.ExecuteNonQueryAsync();

            while (true)
            {
                await connection.WaitAsync();
            }
        }
    }
}