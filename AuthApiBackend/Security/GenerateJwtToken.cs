using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AuthApiBackend.Security
{
    
    public class GenerateJwtToken(IConfiguration _config, IHttpContextAccessor _httpContext)
    {

        private readonly IConfiguration config = _config;
        private readonly IHttpContextAccessor httpContext = _httpContext;

        public string GenerateToken(string accountId, string name, string surname, string role)
        {
            var key = new SymmetricSecurityKey(Convert.FromBase64String(Environment.GetEnvironmentVariable("JWT_KEY")!));

            var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new Dictionary<string, object>
            {
                { JwtRegisteredClaimNames.Sub, accountId },
                { ClaimTypes.GivenName, name },
                { ClaimTypes.Surname, surname },
                { ClaimTypes.Role, role },
                { JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()}
            };

            var jwt = new SecurityTokenDescriptor
            {
                Issuer = config["JWT:Issuer"],
                Audience = config["JWT:Audience"],
                Expires = DateTime.UtcNow.AddMinutes(10),
                Claims = claims,
                SigningCredentials = signingCredentials,
            };

            var handler = new JwtSecurityTokenHandler();
            var token = handler.WriteToken(handler.CreateToken(jwt));

            httpContext.HttpContext?.Response.Cookies.Append("token", token, new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Strict,
                Secure = true,
                Expires = DateTime.UtcNow.AddMinutes(10)
            });

            var refreshToken = Utilities.GenerateCode.GenerateRetreshToken();

            httpContext.HttpContext?.Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Strict,
                Secure = true,
                Expires = DateTime.UtcNow.AddMinutes(15)
            });

            return refreshToken;
        }

    }

}