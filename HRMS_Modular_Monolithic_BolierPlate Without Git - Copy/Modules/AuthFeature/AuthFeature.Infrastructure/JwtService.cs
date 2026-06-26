
// ================================================================
// FILE: Modules/AuthFeature/AuthFeature.Infrastructure/JwtService.cs
// WHY: Concrete JWT implementation using System.IdentityModel.Tokens.Jwt
// INTERVIEW: Explains how JWT works — header.payload.signature (HMACSHA256)
// ================================================================
using AuthFeature.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
 
namespace AuthFeature.Infrastructure
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _config;
 
        public JwtService(IConfiguration config) => _config = config;
 
        public string GenerateAccessToken(string userId, string email, string role)
        {
            var key     = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"]!));
            var creds   = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(double.Parse(_config["Jwt:AccessTokenMinutes"] ?? "60"));
 
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Email,          email),
                new Claim(ClaimTypes.Role,           role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
            };
 
            var token = new JwtSecurityToken(
                issuer:   _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims:   claims,
                expires:  expires,
                signingCredentials: creds
            );
 
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
 
        public string GenerateRefreshToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(64);
            return Convert.ToBase64String(bytes);
        }
 
        public (string userId, string email, string role)? ValidateAccessToken(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var key     = Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"]!);
                handler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey         = new SymmetricSecurityKey(key),
                    ValidateIssuer           = true,
                    ValidIssuer              = _config["Jwt:Issuer"],
                    ValidateAudience         = true,
                    ValidAudience            = _config["Jwt:Audience"],
                    ValidateLifetime         = true,
                    ClockSkew                = TimeSpan.Zero
                }, out var validated);
 
                var jwt    = (JwtSecurityToken)validated;
                var userId = jwt.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
                var email  = jwt.Claims.First(c => c.Type == ClaimTypes.Email).Value;
                var role   = jwt.Claims.First(c => c.Type == ClaimTypes.Role).Value;
                return (userId, email, role);
            }
            catch
            {
                return null;
            }
        }
    }
 
 