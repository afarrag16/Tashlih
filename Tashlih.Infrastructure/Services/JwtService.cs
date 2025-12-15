using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Tashlih.Application.Interfaces;
using Tashlih.Core.Entities;

namespace Tashlih.Infrastructure.Services
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;
        private readonly string _secret;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _expiryMinutes;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
            _secret = _configuration["Jwt:Secret"] ?? throw new ArgumentNullException("Jwt:Secret is not configured");
            _issuer = _configuration["Jwt:Issuer"] ?? "TashlihApi";
            _audience = _configuration["Jwt:Audience"] ?? "TashlihApp";
            _expiryMinutes = _configuration.GetValue<int>("Jwt:ExpiryMinutes", 1440);
        }

        /// <summary>
        /// توليد Access Token للعميل
        /// </summary>
        public string GenerateAccessToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.MobilePhone, user.Phone),
                new Claim(ClaimTypes.Role, "customer"),
                new Claim("user_type", "customer"),
                new Claim("status", user.Status ?? "active")
            };

            if (!string.IsNullOrEmpty(user.Email))
                claims.Add(new Claim(ClaimTypes.Email, user.Email));

            return GenerateToken(claims);
        }

        /// <summary>
        /// توليد Access Token للمورد
        /// </summary>
        public string GenerateSupplierAccessToken(SupplierProfile supplier)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, supplier.Id.ToString()),
                new Claim(ClaimTypes.Name, supplier.FullName ?? supplier.BusinessNameAr),
                new Claim(ClaimTypes.MobilePhone, supplier.Phone ?? ""),
                new Claim(ClaimTypes.Role, "supplier"),
                new Claim("user_type", "supplier"),
                new Claim("status", supplier.Status ?? "active"),
                new Claim("is_verified", supplier.IsVerified.ToString().ToLower()),
                new Claim("verification_status", supplier.VerificationStatus ?? "pending"),
                new Claim("business_name", supplier.BusinessNameAr)
            };

            if (!string.IsNullOrEmpty(supplier.Email))
                claims.Add(new Claim(ClaimTypes.Email, supplier.Email));

            return GenerateToken(claims);
        }

        /// <summary>
        /// توليد Refresh Token
        /// </summary>
        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        /// <summary>
        /// التحقق من صلاحية الـ Token
        /// </summary>
        public bool ValidateToken(string token)
        {
            if (string.IsNullOrEmpty(token))
                return false;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_secret);

            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _issuer,
                    ValidateAudience = true,
                    ValidAudience = _audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// استخراج الـ User ID من الـ Token
        /// </summary>
        public long? GetUserIdFromToken(string token)
        {
            if (string.IsNullOrEmpty(token))
                return null;

            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                var jwtToken = tokenHandler.ReadJwtToken(token);
                var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

                if (userIdClaim != null && long.TryParse(userIdClaim.Value, out var userId))
                    return userId;

                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// استخراج نوع المستخدم من الـ Token
        /// </summary>
        public string? GetUserTypeFromToken(string token)
        {
            if (string.IsNullOrEmpty(token))
                return null;

            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                var jwtToken = tokenHandler.ReadJwtToken(token);
                var userTypeClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "user_type");

                return userTypeClaim?.Value;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// توليد الـ Token الفعلي
        /// </summary>
        private string GenerateToken(List<Claim> claims)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_expiryMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}