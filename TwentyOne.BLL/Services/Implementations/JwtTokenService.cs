using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TwentyOne.BLL.Services.Interfaces;
using TwentyOne.DAL.Entities;
using TwentyOne.Shared.Models;

namespace TwentyOne.BLL.Services.Implementations
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly JwtSettings _jwtSettings;

        public JwtTokenService(IConfiguration configuration)
        {
            _jwtSettings = configuration
                .GetSection("JwtSettings")
                .Get<JwtSettings>()!;
        }

        public string GenerateToken(ApplicationUser user, IList<string> roles)
        {
            //info stored inside the token
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.Name, user.FullName ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Add each role as a claim
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // Create the signing key
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));

            var credentials = new SigningCredentials(
                key, SecurityAlgorithms.HmacSha256);

            // Build the token
            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
                signingCredentials: credentials
            );

            // Serialize and return the token string
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}
