using Microsoft.AspNetCore.Mvc;
using WebAPI.Models;
using WebAPI.Service;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationContextDB _db;
        private readonly IConfiguration _configuration;

        public AuthController(IConfiguration configuration, ApplicationContextDB db)
        {
            _db = db;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserModel user)
        {
            if (_db.Users.Any(u => u.Email == user.Email || u.Login == user.Login))
                return BadRequest("ѕользователь с таким логином или почтой уже существует.");

            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var token = JwtTokenHelper.GenerateJwtToken(
                userId: user.Id,
                userName: user.Name!,
                userEmail: user.Email!,
                userLogin: user.Login!,
                secretKey: _configuration["JwtSettings:SecretKey"]!,
                issuer: _configuration["JwtSettings:Issuer"]!,
                audience: _configuration["JwtSettings:Audience"]!
            );

            return Ok(new { Token = token });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] UserModel user)
        {
            var currentUser = _db.Users.FirstOrDefault(u => u.Login == user.Email || u.Email == user.Email);
            if (currentUser == null || !BCrypt.Net.BCrypt.Verify(user.Password, currentUser.Password))
                return Unauthorized("Ќеверные данные.");

            var token = JwtTokenHelper.GenerateJwtToken(
                userId: currentUser.Id,
                userName: currentUser.Name!,
                userEmail: user.Email!,
                userLogin: currentUser.Login!,
                secretKey: _configuration["JwtSettings:SecretKey"]!,
                issuer: _configuration["JwtSettings:Issuer"]!,
                audience: _configuration["JwtSettings:Audience"]!
            );

            return Ok(new { Token = token });
        }
    }
}
