using Microsoft.AspNetCore.Http;

namespace Tashlih.Application.DTOs.Admin;

// Response عام
public class LookupResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? MessageAr { get; set; }
    public object? Data { get; set; }
}

// إضافة/تعديل نوع مركبة
public class VehicleTypeRequest
{
    public string NameAr { get; set; } = null!;
    public string? NameEn { get; set; }
}

// إضافة/تعديل شركة مصنعة
public class MakeRequest
{
    public string NameAr { get; set; } = null!;
    public string? NameEn { get; set; }
    public int VehicleTypeId { get; set; }
}

// إضافة/تعديل موديل
public class ModelRequest
{
    public string NameAr { get; set; } = null!;
    public string? NameEn { get; set; }
    public int MakeId { get; set; }
}

// إضافة/تعديل تصنيف
public class CategoryRequest
{
    public string NameAr { get; set; } = null!;
    public string? NameEn { get; set; }
    public IFormFile? Icon { get; set; }
}

// إضافة/تعديل تصنيف فرعي
public class SubcategoryRequest
{
    public string NameAr { get; set; } = null!;
    public string? NameEn { get; set; }
    public int VehicleTypeId { get; set; }
}

// إضافة/تعديل مدينة
public class CityRequest
{
    public string NameAr { get; set; } = null!;
    public string? NameEn { get; set; }
}

// إضافة/تعديل حالة قطعة
public class PartConditionRequest
{
    public string Key { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public string? NameEn { get; set; }
}

// إضافة/تعديل نوع ضمان
public class WarrantyTypeRequest
{
    public string Key { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public string? NameEn { get; set; }
    public int Days { get; set; }
}

// إضافة/تعديل سنة صنع
public class YearRequest
{
    public int Year { get; set; }
}