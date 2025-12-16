using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Tashlih.Application.DTOs.Parts
{
    // ==================== Request DTOs ====================

    /// <summary>
    /// طلب إضافة قطعة جديدة
    /// </summary>
    public class CreatePartRequest
    {
        // معلومات أساسية
        [Required(ErrorMessage = "اسم القطعة بالعربي مطلوب")]
        [StringLength(200, MinimumLength = 2)]
        public string NameAr { get; set; } = null!;

        [StringLength(200)]
        public string? NameEn { get; set; }
         
        public string? Description { get; set; }

        [StringLength(50)]
        public string? PartNumber { get; set; }

        [StringLength(50)]
        public string? OemNumber { get; set; }

        // الحالة والضمان
        [Required(ErrorMessage = "حالة القطعة مطلوبة")]
        public string Condition { get; set; } = "used"; // new, used, refurbished

        public string? ConditionDetails { get; set; }

        public string? WarrantyType { get; set; } // none, week, month, etc.

        public int? WarrantyDays { get; set; }

        // السعر والكمية
        [Required(ErrorMessage = "السعر مطلوب")]
        [Range(0.01, 999999.99, ErrorMessage = "السعر يجب أن يكون أكبر من 0")]
        public decimal Price { get; set; }

        public decimal? OriginalPrice { get; set; }

        [Range(1, 9999, ErrorMessage = "الكمية يجب أن تكون 1 على الأقل")]
        public int Quantity { get; set; } = 1;

        // التصنيف - إما من القائمة أو مخصص
        public long? CategoryId { get; set; }
        public string? CustomCategory { get; set; }

        // معلومات السيارة - إما من القائمة أو مخصص
        public int? VehicleTypeId { get; set; }
        public string? CustomVehicleType { get; set; }

        public int? VehicleSubcategoryId { get; set; }
        public string? CustomSubcategory { get; set; }

        public int? MakeId { get; set; }
        public string? CustomMake { get; set; }

        public int? ModelId { get; set; }
        public string? CustomModel { get; set; }

        public short? YearFrom { get; set; }
        public short? YearTo { get; set; }

        [StringLength(17)]
        public string? VinNumber { get; set; }

        // التوصيل
        public bool DeliveryAvailable { get; set; }
        public bool DeliveryByShop { get; set; }

        [StringLength(500)]
        public string? DeliveryNotes { get; set; }

        // الصور
        public List<IFormFile>? Images { get; set; }
    }

    /// <summary>
    /// طلب تعديل قطعة
    /// </summary>
    public class UpdatePartRequest
    {
        [StringLength(200, MinimumLength = 2)]
        public string? NameAr { get; set; }

        [StringLength(200)]
        public string? NameEn { get; set; }

        public string? Description { get; set; }

        [StringLength(50)]
        public string? PartNumber { get; set; }

        [StringLength(50)]
        public string? OemNumber { get; set; }

        public string? Condition { get; set; }
        public string? ConditionDetails { get; set; }
        public string? WarrantyType { get; set; }
        public int? WarrantyDays { get; set; }

        [Range(0.01, 999999.99)]
        public decimal? Price { get; set; }

        public decimal? OriginalPrice { get; set; }

        [Range(0, 9999)]
        public int? Quantity { get; set; }

        public string? Status { get; set; } // available, sold, reserved, hidden

        public long? CategoryId { get; set; }
        public string? CustomCategory { get; set; }

        public int? VehicleTypeId { get; set; }
        public string? CustomVehicleType { get; set; }

        public int? VehicleSubcategoryId { get; set; }
        public string? CustomSubcategory { get; set; }

        public int? MakeId { get; set; }
        public string? CustomMake { get; set; }

        public int? ModelId { get; set; }
        public string? CustomModel { get; set; }

        public short? YearFrom { get; set; }
        public short? YearTo { get; set; }

        public string? VinNumber { get; set; }

        public bool? DeliveryAvailable { get; set; }
        public bool? DeliveryByShop { get; set; }
        public string? DeliveryNotes { get; set; }
    }

    /// <summary>
    /// طلب البحث عن قطع
    /// </summary>
    public class SearchPartsRequest
    {
        public string? Keyword { get; set; }
        public long? CategoryId { get; set; }
        public int? VehicleTypeId { get; set; }
        public int? VehicleSubcategoryId { get; set; }
        public int? MakeId { get; set; }
        public int? ModelId { get; set; }
        public short? Year { get; set; }
        public string? Condition { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? City { get; set; }
        public bool? HasWarranty { get; set; }
        public bool? DeliveryAvailable { get; set; }
        public string? SortBy { get; set; } // price_asc, price_desc, newest, popular
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// طلب إضافة صورة
    /// </summary>
    public class AddPartImageRequest
    {
        [Required]
        public IFormFile Image { get; set; } = null!;

        public bool IsPrimary { get; set; }
    }

    // ==================== Response DTOs ====================

    /// <summary>
    /// استجابة القطعة
    /// </summary>
    public class PartResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? MessageAr { get; set; }
        public PartDto? Part { get; set; }
    }

    /// <summary>
    /// استجابة قائمة القطع
    /// </summary>
    public class PartsListResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? MessageAr { get; set; }
        public List<PartDto>? Parts { get; set; }
        public PaginationInfo? Pagination { get; set; }
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

    // ==================== Data DTOs ====================

    /// <summary>
    /// بيانات القطعة
    /// </summary>
    public class PartDto
    {
        public long Id { get; set; }
        public long ShopId { get; set; }
        public string? ShopName { get; set; }
        public long SupplierId { get; set; }
        public string? SupplierName { get; set; }
        public string? City { get; set; }

        // معلومات أساسية
        public string? NameAr { get; set; }
        public string? NameEn { get; set; }
        public string? Description { get; set; }
        public string? PartNumber { get; set; }
        public string? OemNumber { get; set; }
        public string? VinNumber { get; set; }

        // الحالة والضمان
        public string? Condition { get; set; }
        public string? ConditionAr { get; set; }
        public string? ConditionDetails { get; set; }
        public string? WarrantyType { get; set; }
        public string? WarrantyTypeAr { get; set; }
        public int? WarrantyDays { get; set; }

        // السعر
        public decimal Price { get; set; }
        public decimal? OriginalPrice { get; set; }
        public string? Currency { get; set; }
        public int? DiscountPercent { get; set; }

        // الكمية والحالة
        public int Quantity { get; set; }
        public string? Status { get; set; }
        public bool IsAvailable { get; set; }

        // التصنيف
        public long? CategoryId { get; set; }
        public string? CategoryNameAr { get; set; }
        public string? CategoryNameEn { get; set; }
        public string? CustomCategory { get; set; }
        /// <summary>
        /// اسم التصنيف النهائي (من القائمة أو المخصص)
        /// </summary>
        public string? CategoryDisplay => CategoryNameAr ?? CustomCategory;

        // نوع المركبة
        public int? VehicleTypeId { get; set; }
        public string? VehicleTypeNameAr { get; set; }
        public string? VehicleTypeNameEn { get; set; }
        public string? CustomVehicleType { get; set; }
        /// <summary>
        /// اسم نوع المركبة النهائي
        /// </summary>
        public string? VehicleTypeDisplay => VehicleTypeNameAr ?? CustomVehicleType;

        // التصنيف الفرعي
        public int? VehicleSubcategoryId { get; set; }
        public string? SubcategoryNameAr { get; set; }
        public string? SubcategoryNameEn { get; set; }
        public string? CustomSubcategory { get; set; }
        /// <summary>
        /// اسم التصنيف الفرعي النهائي
        /// </summary>
        public string? SubcategoryDisplay => SubcategoryNameAr ?? CustomSubcategory;

        // الشركة المصنعة
        public int? MakeId { get; set; }
        public string? MakeNameAr { get; set; }
        public string? MakeNameEn { get; set; }
        public string? MakeLogoUrl { get; set; }
        public string? CustomMake { get; set; }
        /// <summary>
        /// اسم الشركة النهائي
        /// </summary>
        public string? MakeDisplay => MakeNameAr ?? CustomMake;

        // الموديل
        public int? ModelId { get; set; }
        public string? ModelNameAr { get; set; }
        public string? ModelNameEn { get; set; }
        public string? CustomModel { get; set; }
        /// <summary>
        /// اسم الموديل النهائي
        /// </summary>
        public string? ModelDisplay => ModelNameAr ?? CustomModel;

        // السنوات
        public short? YearFrom { get; set; }
        public short? YearTo { get; set; }
        public string? YearRange { get; set; }

        // التوصيل
        public bool DeliveryAvailable { get; set; }
        public bool DeliveryByShop { get; set; }
        public string? DeliveryNotes { get; set; }

        // الإحصائيات
        public int ViewsCount { get; set; }
        public int SalesCount { get; set; }
        public int FavoritesCount { get; set; }

        // الصور
        public string? PrimaryImageUrl { get; set; }
        public List<PartImageDto>? Images { get; set; }

        // التواريخ
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// بيانات صورة القطعة
    /// </summary>
    public class PartImageDto
    {
        public long Id { get; set; }
        public string? ImageUrl { get; set; }
        public string? ThumbnailUrl { get; set; }
        public bool IsPrimary { get; set; }
        public int DisplayOrder { get; set; }
    }

    /// <summary>
    /// بيانات القطعة المختصرة (للقوائم)
    /// </summary>
    public class PartSummaryDto
    {
        public long Id { get; set; }
        public string? NameAr { get; set; }
        public string? PrimaryImageUrl { get; set; }
        public decimal Price { get; set; }
        public decimal? OriginalPrice { get; set; }
        public string? Condition { get; set; }
        public string? ConditionAr { get; set; }
        public string? City { get; set; }
        public string? ShopName { get; set; }
        public string? MakeDisplay { get; set; }
        public string? ModelDisplay { get; set; }
        public string? YearRange { get; set; }
        public bool IsAvailable { get; set; }
        public bool HasWarranty { get; set; }
        public bool DeliveryAvailable { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}