using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace WebAPI.Service
{
    public static class JwtTokenHelper
    {
        public static string GenerateJwtToken(int userId, string userName, string userEmail, string userLogin, string secretKey, string issuer, string audience)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, userName), // Имя пользователя
                new Claim(ClaimTypes.Email, userEmail), // Email пользователя
                new Claim(ClaimTypes.GivenName, userLogin), // Уникальный логин пользователя
                new Claim(JwtRegisteredClaimNames.Sub, userLogin), // Уникальный идентификатор субъекта токена
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // Уникальный идентификатор токена (JTI)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1), // Время жизни токена
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}