using API.Entities;
using API.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace API.Services;

public class TokenService(IConfiguration configuration) : ITokenService
{
    public string CreateToken(AppUser appUser)
    {
        string tokenKey = configuration["TokenKey"] ?? throw new ArgumentNullException("TokenKey is missing from appsettings.json");
        if (tokenKey.Length < 64) throw new ArgumentException("TokenKey must be at least 64 characters long");
        SymmetricSecurityKey symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey));
        
        List<Claim> claims =
        [
            new(JwtRegisteredClaimNames.NameId, appUser.UserName)
        ];

        SecurityTokenDescriptor securityTokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha512Signature)
        };

        JwtSecurityTokenHandler jwtSecurityTokenHandler = new();
        SecurityToken securityToken = jwtSecurityTokenHandler.CreateToken(securityTokenDescriptor);
        
        return jwtSecurityTokenHandler.WriteToken(securityToken);
    }
}
