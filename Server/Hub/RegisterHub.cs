using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Server.Model;

namespace Server.Hub;

public class RegisterHub : Microsoft.AspNetCore.SignalR.Hub
{
    private readonly ApplicationContextDB _db;

    public RegisterHub(ApplicationContextDB db)
    {
        _db = db;
    }

    public async Task RegisterUser(string email, string login, string password)
    {
        var currentUser = await _db.Users.FirstOrDefaultAsync(s => s.Email == email);

        if (currentUser == null)
        {
            var userModel = new UserModel
            {
                Email = email,
                Login = login,
                Password = BCrypt.Net.BCrypt.HashPassword(password)
            };
            
            await _db.Users.AddAsync(userModel);
            await _db.SaveChangesAsync();
            
            await Clients.Caller.SendAsync("RegisterSuccess", "User registered successfully");
        }
        else
            await Clients.Caller.SendAsync("RegisterError", "User already registered");
    }
}