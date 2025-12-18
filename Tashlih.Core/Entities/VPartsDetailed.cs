using System;

namespace Tashlih.Core.Entities;

public partial class VPartsDetailed
{
    // معلومات القطعة الأساسية
    public long Id { get; set; }
    public long SupplierId { get; set; }
    public string NameAr { get; set; } = null!;
    public string? NameEn { get; set; }
    public string? Description { get; set; }
    public string? PartNumber { get; set; }
    public string? OemNumber { get; set; }
    public string Condition { get; set; } = null!;
    public string? ConditionDetails { get; set; }
    public string? WarrantyType { get; set; }
    public int? WarrantyDays { get; set; }
    public decimal Price { get; set; }
    public decimal? OriginalPrice { get; set; }
    public string Currency { get; set; } = null!;
    public int Quantity { get; set; }
    public string Status { get; set; } = null!;
    public bool IsFeatured { get; set; }
    public DateOnly? FeaturedUntil { get; set; }
    public int ViewsCount { get; set; }
    public int SalesCount { get; set; }
    public int FavoritesCount { get; set; }

    // التصنيف
    public long? CategoryId { get; set; }
    public string? CategoryNameAr { get; set; }
    public string? CategoryNameEn { get; set; }
    public string? CustomCategory { get; set; }

    // نوع المركبة
    public int? VehicleTypeId { get; set; }
    public string? VehicleTypeNameAr { get; set; }
    public string? VehicleTypeNameEn { get; set; }
    public string? CustomVehicleType { get; set; }

    // التصنيف الفرعي
    public int? VehicleSubcategoryId { get; set; }
    public string? SubcategoryNameAr { get; set; }
    public string? SubcategoryNameEn { get; set; }
    public string? CustomSubcategory { get; set; }

    // الشركة المصنعة
    public int? MakeId { get; set; }
    public string? MakeNameAr { get; set; }
    public string? MakeNameEn { get; set; }
    public string? MakeLogoUrl { get; set; }
    public string? CustomMake { get; set; }

    // الموديل
    public int? ModelId { get; set; }
    public string? ModelNameAr { get; set; }
    public string? ModelNameEn { get; set; }
    public string? CustomModel { get; set; }

    // السنوات
    public short? YearFrom { get; set; }
    public short? YearTo { get; set; }
    public string? VinNumber { get; set; }

    // التوصيل
    public bool DeliveryAvailable { get; set; }
    public bool DeliveryByShop { get; set; }
    public string? DeliveryNotes { get; set; }

    // معلومات المورد
    public string? SupplierName { get; set; }
    public string? BusinessNameAr { get; set; }
    public string? BusinessNameEn { get; set; }
    public string? SupplierCity { get; set; }
    public string? SupplierDistrict { get; set; }
    public string? SupplierPhone { get; set; }
    public string? SupplierLogoUrl { get; set; }
    public bool SupplierIsVerified { get; set; }
    public string? SupplierVerificationStatus { get; set; }
    public decimal? SupplierRating { get; set; }
    public int? SupplierRatingCount { get; set; }

    // الصور
    public string? PrimaryImageUrl { get; set; }
    public int ImagesCount { get; set; }

    // التواريخ
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}