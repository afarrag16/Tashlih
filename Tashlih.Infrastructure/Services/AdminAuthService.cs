using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Tashlih.Application.DTOs.Admin;
using Tashlih.Application.Interfaces;
using Tashlih.Infrastructure.Models;

namespace Tashlih.Infrastructure.Services;

public class AdminAuthService : IAdminAuthService
{
    private readonly TashlihContext _context;
    private readonly IConfiguration _configuration;

    public AdminAuthService(TashlihContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    /// <summary>
    /// تسجيل دخول الأدمن
    /// </summary>
    public async Task<AdminLoginResponse> LoginAsync(AdminLoginRequest request)
    {
        var admin = await _context.Admins
            .FirstOrDefaultAsync(a => a.Email == request.Email);

        if (admin == null)
        {
            return new AdminLoginResponse
            {
                Success = false,
                Message = "Invalid email or password",
                MessageAr = "البريد الإلكتروني أو كلمة المرور غير صحيحة"
            };
        }

        // التحقق من كلمة المرور
        if (!BCrypt.Net.BCrypt.Verify(request.Password, admin.PasswordHash))
        {
            return new AdminLoginResponse
            {
                Success = false,
                Message = "Invalid email or password",
                MessageAr = "البريد الإلكتروني أو كلمة المرور غير صحيحة"
            };
        }

        // التحقق من الحساب فعال
        if (!admin.IsActive)
        {
            return new AdminLoginResponse
            {
                Success = false,
                Message = "Account is disabled",
                MessageAr = "الحساب معطل"
            };
        }

        // تحديث آخر تسجيل دخول
        admin.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // إنشاء التوكن
        var token = GenerateJwtToken(admin);

        return new AdminLoginResponse
        {
            Success = true,
            Message = "Login successful",
            MessageAr = "تم تسجيل الدخول بنجاح",
            Token = token,
            Admin = MapToAdminDto(admin)
        };
    }

    /// <summary>
    /// جلب بيانات الأدمن
    /// </summary>
    public async Task<AdminProfileResponse> GetProfileAsync(long adminId)
    {
        var admin = await _context.Admins.FindAsync(adminId);

        if (admin == null)
        {
            return new AdminProfileResponse
            {
                Success = false,
                Message = "Admin not found",
                MessageAr = "الأدمن غير موجود"
            };
        }

        return new AdminProfileResponse
        {
            Success = true,
            Admin = MapToAdminDto(admin)
        };
    }

    #region Helper Methods

    private string GenerateJwtToken(Core.Entities.Admin admin)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var secretKey = jwtSettings["Secret"];
        var issuer = jwtSettings["Issuer"];
        var audience = jwtSettings["Audience"];
        var expiryMinutes = int.Parse(jwtSettings["ExpiryMinutes"] ?? "1440");

        var claims = new[]
        {
        new Claim(ClaimTypes.NameIdentifier, admin.Id.ToString()),
        new Claim(ClaimTypes.Email, admin.Email),
        new Claim(ClaimTypes.Name, admin.FullName),
        new Claim("user_type", "admin"),
        new Claim("admin_id", admin.Id.ToString())
    };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private AdminDto MapToAdminDto(Core.Entities.Admin admin)
    {
        return new AdminDto
        {
            Id = admin.Id,
            FullName = admin.FullName,
            Email = admin.Email,
            Phone = admin.Phone,
            IsActive = admin.IsActive,
            LastLoginAt = admin.LastLoginAt,
            CreatedAt = admin.CreatedAt
        };
    }

    #endregion
}
