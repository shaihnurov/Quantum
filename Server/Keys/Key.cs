using System.Text.Json;

namespace Server.Keys
{
    public class Key
    {
        public static readonly string EmailVerificationPassword;
        public static readonly string EmailVerificationAddress;
        public static readonly string SecretKeyAuthUser;

        static Key()
        {
            //Локальная сборка
            /*            var json = File.ReadAllText("Keys/Keys.json");
                        var keys = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

                        EmailVerificationPassword = keys["EmailVerificationPassword"];
                        EmailVerificationAddress = keys["EmailVerificationAddress"];
                        SecretKeyAuthUser = keys["SecretKeyAuthUser"];*/

            //Сборка через Render
            EmailVerificationPassword = Environment.GetEnvironmentVariable("EmailVerificationPassword");
            EmailVerificationAddress = Environment.GetEnvironmentVariable("EmailVerificationAddress");
            SecretKeyAuthUser = Environment.GetEnvironmentVariable("SecretKeyAuthUser");
        }
    }
}