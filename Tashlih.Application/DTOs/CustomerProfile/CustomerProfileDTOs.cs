namespace Tashlih.Application.DTOs.CustomerProfile;

#region Response DTOs

/// <summary>
/// بيانات الملف الشخصي للعميل
/// </summary>
public class CustomerProfileDto
{
    public long Id { get; set; }
    public string FullName { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string? Email { get; set; }
    public string? AvatarUrl { get; set; }
    public string PreferredLanguage { get; set; } = null!;
    public bool NotificationsEnabled { get; set; }
    public CustomerAddressDto? Address { get; set; }
    public DateTime? CreatedAt { get; set; }
    public bool IsPhoneVerified { get; set; }
}

/// <summary>
/// بيانات العنوان
/// </summary>
public class CustomerAddressDto
{
    public string? Street { get; set; }
    public int? CityId { get; set; }
    public string? CityNameAr { get; set; }
    public string? CityNameEn { get; set; }
    public string? PostalCode { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
}

/// <summary>
/// بيانات المدينة
/// </summary>
public class CityDto
{
    public int Id { get; set; }
    public string NameAr { get; set; } = null!;
    public string NameEn { get; set; } = null!;
}

#endregion

#region Request DTOs

/// <summary>
/// تحديث بيانات الملف الشخصي
/// </summary>
public class UpdateCustomerProfileRequest
{
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? PreferredLanguage { get; set; }
    public bool? NotificationsEnabled { get; set; }

    // بيانات العنوان
    public string? Street { get; set; }
    public int? CityId { get; set; }
    public string? PostalCode { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
}

/// <summary>
/// تحديث العنوان فقط
/// </summary>
public class UpdateAddressRequest
{
    public string? Street { get; set; }
    public int? CityId { get; set; }
    public string? PostalCode { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
}

/// <summary>
/// تحديث العنوان من الموقع (GPS)
/// </summary>
public class UpdateLocationRequest
{
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public string? Street { get; set; }
    public int? CityId { get; set; }
    public string? PostalCode { get; set; }
}

#endregion