using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Tashlih.Application.DTOs.CustomerProfile;
using Tashlih.Application.Interfaces;
using Tashlih.Core.Entities;
using Tashlih.Infrastructure.Models;

namespace Tashlih.Infrastructure.Services;

public class CustomerProfileService : ICustomerProfileService
{
    private readonly TashlihContext _context;
    private readonly IFileService _fileService;

    public CustomerProfileService(TashlihContext context, IFileService fileService)
    {
        _context = context;
        _fileService = fileService;
    }

    /// <summary>
    /// جلب بيانات الملف الشخصي للعميل
    /// </summary>
    public async Task<CustomerProfileDto?> GetProfileAsync(long userId)
    {
        var user = await _context.Users
            .Include(u => u.City)
            .FirstOrDefaultAsync(u => u.Id == userId && u.UserType == "customer");

        if (user == null)
            return null;

        return new CustomerProfileDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Phone = user.Phone,
            Email = user.Email,
            AvatarUrl = user.AvatarUrl,
            PreferredLanguage = user.PreferredLanguage,
            NotificationsEnabled = user.NotificationsEnabled,
            CreatedAt = user.CreatedAt,
            Address = new CustomerAddressDto
            {
                Street = user.Street,
                CityId = user.CityId,
                CityNameAr = user.City?.NameAr,
                CityNameEn = user.City?.NameEn,
                PostalCode = user.PostalCode,
                Latitude = user.Latitude,
                Longitude = user.Longitude
            }
        };
    }

    /// <summary>
    /// تحديث بيانات الملف الشخصي
    /// </summary>
    public async Task<(bool Success, string Message)> UpdateProfileAsync(long userId, UpdateCustomerProfileRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.UserType == "customer");

        if (user == null)
            return (false, "المستخدم غير موجود");

        // تحديث البيانات الشخصية
        if (!string.IsNullOrEmpty(request.FullName))
            user.FullName = request.FullName;

        if (request.Email != null)
            user.Email = request.Email;

        if (!string.IsNullOrEmpty(request.PreferredLanguage))
            user.PreferredLanguage = request.PreferredLanguage;

        if (request.NotificationsEnabled.HasValue)
            user.NotificationsEnabled = request.NotificationsEnabled.Value;

        // تحديث العنوان
        if (request.Street != null)
            user.Street = request.Street;

        if (request.CityId.HasValue)
        {
            // التحقق من وجود المدينة
            var cityExists = await _context.Cities.AnyAsync(c => c.Id == request.CityId.Value);
            if (!cityExists)
                return (false, "المدينة غير موجودة");

            user.CityId = request.CityId.Value;
        }

        if (request.PostalCode != null)
            user.PostalCode = request.PostalCode;

        if (request.Latitude.HasValue)
            user.Latitude = request.Latitude.Value;

        if (request.Longitude.HasValue)
            user.Longitude = request.Longitude.Value;

        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return (true, "تم تحديث البيانات بنجاح");
    }

    /// <summary>
    /// رفع صورة الملف الشخصي
    /// </summary>
    /// <summary>
    /// رفع صورة الملف الشخصي
    /// </summary>
    public async Task<(bool Success, string Message, string? AvatarUrl)> UploadAvatarAsync(long userId, IFormFile file)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.UserType == "customer");

        if (user == null)
            return (false, "المستخدم غير موجود", null);

        // التحقق من نوع الملف
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        if (!_fileService.IsValidFileType(file, allowedExtensions))
            return (false, "نوع الملف غير مدعوم. الأنواع المدعومة: JPG, JPEG, PNG, WEBP", null);

        // التحقق من حجم الملف (5 ميجا كحد أقصى)
        if (!_fileService.IsValidFileSize(file, 5 * 1024 * 1024))
            return (false, "حجم الملف كبير جداً. الحد الأقصى 5 ميجابايت", null);

        // حذف الصورة القديمة إن وجدت
        if (!string.IsNullOrEmpty(user.AvatarUrl))
        {
            await _fileService.DeleteFileAsync(user.AvatarUrl);
        }

        // رفع الصورة الجديدة
        var avatarUrl = await _fileService.UploadFileAsync(file, "avatars/customers");

        // تحديث رابط الصورة
        user.AvatarUrl = avatarUrl;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return (true, "تم رفع الصورة بنجاح", avatarUrl);
    }

    /// <summary>
    /// حذف صورة الملف الشخصي
    /// </summary>
    public async Task<(bool Success, string Message)> DeleteAvatarAsync(long userId)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.UserType == "customer");

        if (user == null)
            return (false, "المستخدم غير موجود");

        if (string.IsNullOrEmpty(user.AvatarUrl))
            return (false, "لا توجد صورة للحذف");

        // حذف الصورة من التخزين
        await _fileService.DeleteFileAsync(user.AvatarUrl);

        // إزالة الرابط
        user.AvatarUrl = null;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return (true, "تم حذف الصورة بنجاح");
    }

    /// <summary>
    /// تحديث العنوان من الموقع
    /// </summary>
    public async Task<(bool Success, string Message)> UpdateLocationAsync(long userId, UpdateLocationRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.UserType == "customer");

        if (user == null)
            return (false, "المستخدم غير موجود");

        // تحديث الإحداثيات
        user.Latitude = request.Latitude;
        user.Longitude = request.Longitude;

        // تحديث بيانات العنوان إن وجدت
        if (request.Street != null)
            user.Street = request.Street;

        if (request.CityId.HasValue)
        {
            var cityExists = await _context.Cities.AnyAsync(c => c.Id == request.CityId.Value);
            if (!cityExists)
                return (false, "المدينة غير موجودة");

            user.CityId = request.CityId.Value;
        }

        if (request.PostalCode != null)
            user.PostalCode = request.PostalCode;

        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return (true, "تم تحديث الموقع بنجاح");
    }

    /// <summary>
    /// جلب قائمة المدن
    /// </summary>
    public async Task<List<CityDto>> GetCitiesAsync()
    {
        return await _context.Cities
            .Where(c => c.IsActive)
            .OrderBy(c => c.NameAr)
            .Select(c => new CityDto
            {
                Id = c.Id,
                NameAr = c.NameAr,
                NameEn = c.NameEn
            })
            .ToListAsync();
    }
}