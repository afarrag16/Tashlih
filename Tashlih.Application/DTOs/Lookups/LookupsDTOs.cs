using System;
using System.Collections.Generic;

namespace Tashlih.Application.DTOs.Lookups
{
    // ==================== Response DTOs ====================

    public class LookupsResponse<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? MessageAr { get; set; }
        public List<T>? Data { get; set; }
        public int Count { get; set; }
    }

    // ==================== Vehicle Types ====================

    public class VehicleTypeDto
    {
        public int Id { get; set; }
        public string? NameAr { get; set; }
        public string? NameEn { get; set; }
        public string? Icon { get; set; }
        public int SortOrder { get; set; }
        public int SubcategoriesCount { get; set; }
    }

    // ==================== Vehicle Subcategories ====================

    public class VehicleSubcategoryDto
    {
        public int Id { get; set; }
        public int VehicleTypeId { get; set; }
        public string? VehicleTypeNameAr { get; set; }
        public string? NameAr { get; set; }
        public string? NameEn { get; set; }
        public string? Icon { get; set; }
        public int SortOrder { get; set; }
    }

    // ==================== Part Categories ====================

    public class PartCategoryDto
    {
        public long Id { get; set; }
        public long? ParentId { get; set; }
        public string? ParentNameAr { get; set; }
        public string? NameAr { get; set; }
        public string? NameEn { get; set; }
        public string? Icon { get; set; }
        public int SortOrder { get; set; }
        public int Level { get; set; }
        public List<PartCategoryDto>? Children { get; set; }
    }

    // ==================== Vehicle Makes ====================

    public class VehicleMakeDto
    {
        public int Id { get; set; }
        public int VehicleTypeId { get; set; }
        public string? VehicleTypeNameAr { get; set; }
        public string? NameAr { get; set; }
        public string? NameEn { get; set; }
        public string? LogoUrl { get; set; }
        public string? Country { get; set; }
        public int SortOrder { get; set; }
        public bool IsPopular { get; set; }
        public int ModelsCount { get; set; }
    }

    // ==================== Vehicle Models ====================

    public class VehicleModelDto
    {
        public int Id { get; set; }
        public int MakeId { get; set; }
        public string? MakeNameAr { get; set; }
        public string? NameAr { get; set; }
        public string? NameEn { get; set; }
        public short? YearFrom { get; set; }
        public short? YearTo { get; set; }
    }

    // ==================== Years ====================

    public class YearDto
    {
        public int Year { get; set; }
    }

    // ==================== Cities ====================

    public class CityDto
    {
        public string? NameAr { get; set; }
        public string? NameEn { get; set; }
    }

    // ==================== Warranty Types ====================

    public class WarrantyTypeDto
    {
        public string? Key { get; set; }
        public string? NameAr { get; set; }
        public string? NameEn { get; set; }
        public int? Days { get; set; }
    }

    // ==================== Part Conditions ====================

    public class PartConditionDto
    {
        public string? Key { get; set; }
        public string? NameAr { get; set; }
        public string? NameEn { get; set; }
    }

    // ==================== All Lookups Response ====================

    public class AllLookupsResponse
    {
        public bool Success { get; set; }
        public List<VehicleTypeDto>? VehicleTypes { get; set; }
        public List<PartCategoryDto>? PartCategories { get; set; }
        public List<VehicleMakeDto>? VehicleMakes { get; set; }
        public List<PartConditionDto>? PartConditions { get; set; }
        public List<WarrantyTypeDto>? WarrantyTypes { get; set; }
        public List<CityDto>? Cities { get; set; }
    }
}
