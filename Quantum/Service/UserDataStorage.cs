using System.Text.Json;
using System.Text.Encodings.Web;
using Server.Model;
using System.Threading.Tasks;
using System.IO;

namespace Quantum.Service
{
    public class UserDataStorage
    {
        private const string FilePath = "dataUser.json";

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true
        };

        public static async Task SaveUserData(UserDataJson userData)
        {
            var jsonString = JsonSerializer.Serialize(userData, JsonOptions);
            await File.WriteAllTextAsync(FilePath, jsonString);
        }

        public static async Task<UserDataJson> GetUserData()
        {
            if (File.Exists(FilePath))
            {
                string jsonString = await File.ReadAllTextAsync(FilePath);
                return JsonSerializer.Deserialize<UserDataJson>(jsonString, JsonOptions)!;
            }

            return null!;
        }

        public static void DeleteUserData()
        {
            if (File.Exists(FilePath))
            {
                File.Delete(FilePath);
            }
        }
    }
}