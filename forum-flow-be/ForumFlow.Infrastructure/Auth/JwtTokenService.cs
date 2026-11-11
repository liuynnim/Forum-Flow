using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ForumFlow.Application.Common.Interfaces;
using ForumFlow.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    public string GenerateAccessToken(User user)
    {
        string secretKey = configuration["JwtSettings:SecretKey"]!;
        SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

        SigningCredentials creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("username", user.Username),
            new Claim("displayName", user.DisplayName),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
        };

        int expiresInHours = int.Parse(configuration["JwtSettings:ExpiresInHours"] ?? "24");

        JwtSecurityToken token = new JwtSecurityToken(
            issuer: configuration["JwtSettings:Issuer"],
            audience: configuration["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(expiresInHours),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        byte[] bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }
}
