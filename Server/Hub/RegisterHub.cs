using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Server.Model;
using Server.Service;

namespace Server.Hub;

public class RegisterHub : Microsoft.AspNetCore.SignalR.Hub
{
    private readonly ApplicationContextDB _db;

    public RegisterHub(ApplicationContextDB db)
    {
        _db = db;
    }

    public async Task RegisterUser(string email, string login, string name, string password, bool rememberMe)
    {
        var currentUser = await _db.Users.FirstOrDefaultAsync(s => s.Email == email);
        var userData = new UserDataJson();

        if (currentUser == null)
        {
            if (rememberMe)
            {
                string token = TokenService.GenerateToken(email);

                userData = new UserDataJson
                {
                    Token = token,
                    Name = name,
                    Email = email,
                    Login = login,
                    ExpiryDate = DateTime.UtcNow.AddHours(7)
                };
            }
            else
            {
                userData = new UserDataJson
                {
                    Email = email,
                    Name = name,
                    Login = login,
                };
            }

            var userModel = new UserModel
            {
                Email = email,
                Name = name,
                Login = login,
                Password = BCrypt.Net.BCrypt.HashPassword(password)
            };
            
            await _db.Users.AddAsync(userModel);
            await _db.SaveChangesAsync();
            
            await Clients.Caller.SendAsync("RegisterSuccess", "User registered successfully", userData);
        }
        else
            await Clients.Caller.SendAsync("RegisterError", "User already registered");
    }
}