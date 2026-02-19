using Microsoft.AspNetCore.Http;
using Tashlih.Application.DTOs.CustomerProfile;

namespace Tashlih.Application.Interfaces;

public interface ICustomerProfileService
{
    /// <summary>
    /// جلب بيانات الملف الشخصي للعميل
    /// </summary>
    Task<CustomerProfileDto?> GetProfileAsync(long userId);

    /// <summary>
    /// تحديث بيانات الملف الشخصي
    /// </summary>
    Task<(bool Success, string Message)> UpdateProfileAsync(long userId, UpdateCustomerProfileRequest request);

    /// <summary>
    /// رفع صورة الملف الشخصي
    /// </summary>
    Task<(bool Success, string Message, string? AvatarUrl)> UploadAvatarAsync(long userId, IFormFile file);

    /// <summary>
    /// حذف صورة الملف الشخصي
    /// </summary>
    Task<(bool Success, string Message)> DeleteAvatarAsync(long userId);

    /// <summary>
    /// تحديث العنوان من الموقع
    /// </summary>
    Task<(bool Success, string Message)> UpdateLocationAsync(long userId, UpdateLocationRequest request);

    /// <summary>
    /// جلب قائمة المدن
    /// </summary>
    Task<List<CityDto>> GetCitiesAsync();

    /// <summary>
    /// حذف الحساب
    /// </summary>
    Task<DeleteAccountResponse> DeleteAccountAsync(long userId, DeleteAccountRequest request);
}