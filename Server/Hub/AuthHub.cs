using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Server.Model;
using Server.Service;

namespace Server.Hub;

public class AuthHub : Microsoft.AspNetCore.SignalR.Hub
{
    private readonly ApplicationContextDB _db;

    public AuthHub(ApplicationContextDB db)
    {
        _db = db;
    }

    public async Task AuthUser(string email, string password, bool rememberMe)
    {
        var currentUser = await _db.Users.FirstOrDefaultAsync(s => s.Email == email || s.Login == email);

        if (currentUser != null)
        {
            bool checkPassword = BCrypt.Net.BCrypt.Verify(password, currentUser.Password);
            var userData = new UserDataJson();

            if (checkPassword)
            {
                if (rememberMe)
                {
                    string token = TokenService.GenerateToken(email);

                    userData = new UserDataJson
                    {
                        Token = token,
                        Id = currentUser.Id,
                        Email = currentUser.Email,
                        Name = currentUser.Name,
                        Login = currentUser.Login,
                        ExpiryDate = DateTime.UtcNow.AddHours(7)
                    };
                }
                else
                {
                    userData = new UserDataJson
                    {
                        Id = currentUser.Id,
                        Email = currentUser.Email,
                        Name = currentUser.Name,
                        Login = currentUser.Login,
                    };
                }
                
                await Clients.Caller.SendAsync("FirtsAuthSuccess", "Successful entry", userData);
            }else
                await Clients.Caller.SendAsync("AuthError", "Incorrect password");
        }
        else
            await Clients.Caller.SendAsync("AuthError", "User not found");
    }
    public async Task AuthToken(UserDataJson storedUserData)
    {
        if (storedUserData != null && !string.IsNullOrEmpty(storedUserData.Token))
        {
            var claimsPrincipal = TokenService.ValidateToken(storedUserData.Token);
            if (claimsPrincipal != null)
            {
                await Clients.Caller.SendAsync("AuthSuccess", "Successful entry");
                Log.Information("Авторизация по токену успешна");
            }
            else
            {
                await Clients.Caller.SendAsync("AuthError", "Invalid session");
                Log.Information("Сессия по токену не валидна");
            }
        }
        else
        {
            Log.Information("Токен не найден или отсутствует");
            await Clients.Caller.SendAsync("AuthError", "Please sign in to your account");
        }
    }
}