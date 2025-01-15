using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Server.Model;

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

            if (checkPassword)
            {
                if (rememberMe)
                {
                    
                }
                
                await Clients.Caller.SendAsync("AuthSuccess", "Successful entry");
            }else
                await Clients.Caller.SendAsync("AuthError", "Incorrect password");
        }
        else
            await Clients.Caller.SendAsync("AuthError", "User not found");
    }
}