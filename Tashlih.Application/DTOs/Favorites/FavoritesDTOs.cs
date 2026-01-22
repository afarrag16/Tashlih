namespace Tashlih.Application.DTOs.Favorites;

#region Response DTOs

/// <summary>
/// استجابة بسيطة
/// </summary>
public class FavoriteResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? MessageAr { get; set; }
}

/// <summary>
/// استجابة القطع المفضلة
/// </summary>
public class FavoritePartsResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? MessageAr { get; set; }
    public List<FavoritePartDto>? Parts { get; set; }
    public int TotalCount { get; set; }
}

/// <summary>
/// استجابة الموردين المفضلين
/// </summary>
public class FavoriteSuppliersResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? MessageAr { get; set; }
    public List<FavoriteSupplierDto>? Suppliers { get; set; }
    public int TotalCount { get; set; }
}

/// <summary>
/// استجابة التحقق من المفضلة
/// </summary>
public class FavoriteCheckResponse
{
    public bool Success { get; set; }
    public bool IsFavorite { get; set; }
    public string? Message { get; set; }
    public string? MessageAr { get; set; }
}

#endregion

#region Data DTOs

/// <summary>
/// بيانات القطعة المفضلة
/// </summary>
public class FavoritePartDto
{
    public long Id { get; set; }
    public long PartId { get; set; }
    public string? PartName { get; set; }
    public string? PartImage { get; set; }
    public decimal Price { get; set; }
    public decimal? OriginalPrice { get; set; }
    public string? Currency { get; set; }
    public string? Condition { get; set; }
    public string? ConditionAr { get; set; }
    public long SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public string? CategoryName { get; set; }
    public string? CategoryNameEn { get; set; }
    public int? Quantity { get; set; }
    public string? City { get; set; }
    public bool IsAvailable { get; set; }
    public bool HasWarranty { get; set; }
    public DateTime? AddedAt { get; set; }
}

/// <summary>
/// بيانات المورد المفضل
/// </summary>
public class FavoriteSupplierDto
{
    public long Id { get; set; }
    public long SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public string? BusinessType { get; set; }
    public string? SupplierLogo { get; set; }
    public string? City { get; set; }
    public string? District { get; set; }
    public string? Phone { get; set; }
    public decimal RatingAverage { get; set; }
    public int RatingCount { get; set; }
    public int PartsCount { get; set; }
    public bool IsVerified { get; set; }
    public DateTime? AddedAt { get; set; }
}

#endregion
