
using Tashlih.Application.DTOs.Parts;
using Tashlih.Application.DTOs.Reviews;

namespace Tashlih.Application.DTOs.Suppliers;

#region Response DTOs

/// <summary>
/// تفاصيل المورد الكاملة
/// </summary>
public class SupplierDetailsDto
{
    public long Id { get; set; }
    public string? FullName { get; set; }
    public string BusinessNameAr { get; set; } = null!;
    public string? BusinessNameEn { get; set; }
    public string? Description { get; set; }
    public string? BusinessType { get; set; }
    public string? LogoUrl { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }
    public string? Phone { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public bool IsVerified { get; set; }
    public decimal RatingAverage { get; set; }
    public int RatingCount { get; set; }
    public int TotalOrders { get; set; }
    public int CompletedOrders { get; set; }
    public int PartsCount { get; set; }
    public DateTime? CreatedAt { get; set; }
    
    public List<PartDto>? Parts { get; set; }
    public RatingBreakdownDto? RatingBreakdown { get; set; }
    public List<ReviewDto>? Reviews { get; set; }
}

/// <summary>
/// بيانات المورد للقوائم
/// </summary>
public class SupplierListDto
{
    public long Id { get; set; }
    public string BusinessNameAr { get; set; } = null!;
    public string? BusinessNameEn { get; set; }
    public string? LogoUrl { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }
    public bool IsVerified { get; set; }
    public decimal RatingAverage { get; set; }
    public int RatingCount { get; set; }
    public int PartsCount { get; set; }
}

/// <summary>
/// بيانات المورد مع المسافة
/// </summary>
public class SupplierNearbyDto
{
    public long Id { get; set; }
    public string BusinessNameAr { get; set; } = null!;
    public string? BusinessNameEn { get; set; }
    public string? LogoUrl { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public double Distance { get; set; }
    public bool IsVerified { get; set; }
    public decimal RatingAverage { get; set; }
    public int PartsCount { get; set; }
}

#endregion

#region Response Wrappers

/// <summary>
/// استجابة تفاصيل المورد
/// </summary>
public class SupplierDetailsResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? MessageAr { get; set; }
    public SupplierDetailsDto? Supplier { get; set; }
}

/// <summary>
/// استجابة قائمة الموردين
/// </summary>
public class SuppliersListResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? MessageAr { get; set; }
    public List<SupplierListDto>? Suppliers { get; set; }
    public PaginationInfo? Pagination { get; set; }
}

/// <summary>
/// استجابة الموردين القريبين
/// </summary>
public class SuppliersNearbyResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? MessageAr { get; set; }
    public List<SupplierNearbyDto>? Suppliers { get; set; }
}

/// <summary>
/// معلومات الصفحات
/// </summary>
public class PaginationInfo
{
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
    public bool HasNext { get; set; }
    public bool HasPrevious { get; set; }
}


/// <summary>
/// استجابة إحصائيات المورد
/// </summary>
public class SupplierStatisticsResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? MessageAr { get; set; }
    public SupplierStatisticsDto? Statistics { get; set; }
}

/// <summary>
/// إحصائيات المورد
/// </summary>
public class SupplierStatisticsDto
{
    public OrdersStatisticsDto? Orders { get; set; }
    public List<TopSellingPartDto>? TopSellingParts { get; set; }
}

/// <summary>
/// إحصائيات الطلبات
/// </summary>
public class OrdersStatisticsDto
{
    public int New { get; set; }        // pending
    public int Completed { get; set; }  // received
    public int Cancelled { get; set; }  // cancelled + rejected
}

/// <summary>
/// القطع الأكثر مبيعاً
/// </summary>
public class TopSellingPartDto
{
    public long Id { get; set; }
    public string? Name { get; set; }
    public string? Image { get; set; }
    public decimal Price { get; set; }
    public string? Currency { get; set; }
    public int SalesCount { get; set; }
}

#endregion