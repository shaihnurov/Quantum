using System.Collections.Concurrent;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace WebAPI.Service
{
    public class UserConnectionsService
    {
        // Храним mapping: connectionId -> userId
        private readonly ConcurrentDictionary<string, int> _connectedUsers = new();

        // Метод для добавления подключения
        public void AddConnection(string connectionId, int userId)
        {
            _connectedUsers[connectionId] = userId;
        }

        // Метод для удаления подключения
        public void RemoveConnection(string connectionId)
        {
            _connectedUsers.TryRemove(connectionId, out _);
        }

        // Метод для получения userId по connectionId
        public int? GetUserIdByConnectionId(string connectionId)
        {
            return _connectedUsers.ContainsKey(connectionId) ? _connectedUsers[connectionId] : (int?)null;
        }

        // Метод для получения connectionId по userId
        public string? GetConnectionIdByUserId(int userId)
        {
            var userConnection = _connectedUsers.FirstOrDefault(u => u.Value == userId);
            Console.WriteLine($"ConnectionID Service - {userConnection.Key}");
            return userConnection.Key;
        }
    }
}
