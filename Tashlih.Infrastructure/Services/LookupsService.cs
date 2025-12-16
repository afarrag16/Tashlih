using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tashlih.Application.DTOs.Lookups;
using Tashlih.Application.Interfaces;
using Tashlih.Infrastructure.Models;

namespace Tashlih.Infrastructure.Services
{
    public class LookupsService : ILookupsService
    {
        private readonly TashlihContext _context;

        public LookupsService(TashlihContext context)
        {
            _context = context;
        }

        /// <summary>
        /// جلب أنواع المركبات
        /// </summary>
        public async Task<LookupsResponse<VehicleTypeDto>> GetVehicleTypesAsync()
        {
            var types = await _context.VehicleTypes
                .Where(t => t.IsActive)
                .OrderBy(t => t.SortOrder)
                .Select(t => new VehicleTypeDto
                {
                    Id = t.Id,
                    NameAr = t.NameAr,
                    NameEn = t.NameEn,
                    Icon = t.Icon,
                    SortOrder = t.SortOrder,
                    SubcategoriesCount = t.VehicleSubcategories.Count(s => s.IsActive)
                })
                .ToListAsync();

            return new LookupsResponse<VehicleTypeDto>
            {
                Success = true,
                Data = types,
                Count = types.Count
            };
        }

        /// <summary>
        /// جلب التصنيفات الفرعية
        /// </summary>
        public async Task<LookupsResponse<VehicleSubcategoryDto>> GetSubcategoriesAsync(int? vehicleTypeId = null)
        {
            var query = _context.VehicleSubcategories
                .Include(s => s.VehicleType)
                .Where(s => s.IsActive);

            if (vehicleTypeId.HasValue)
            {
                query = query.Where(s => s.VehicleTypeId == vehicleTypeId);
            }

            var subcategories = await query
                .OrderBy(s => s.VehicleTypeId)
                .ThenBy(s => s.SortOrder)
                .Select(s => new VehicleSubcategoryDto
                {
                    Id = s.Id,
                    VehicleTypeId = s.VehicleTypeId,
                    VehicleTypeNameAr = s.VehicleType.NameAr,
                    NameAr = s.NameAr,
                    NameEn = s.NameEn,
                    Icon = s.Icon,
                    SortOrder = s.SortOrder
                })
                .ToListAsync();

            return new LookupsResponse<VehicleSubcategoryDto>
            {
                Success = true,
                Data = subcategories,
                Count = subcategories.Count
            };
        }

        /// <summary>
        /// جلب فئات القطع
        /// </summary>
        public async Task<LookupsResponse<PartCategoryDto>> GetPartCategoriesAsync(bool hierarchical = false)
        {
            var categories = await _context.PartCategories
                .Include(c => c.Parent)
                .Where(c => c.IsActive)
                .OrderBy(c => c.Level)
                .ThenBy(c => c.SortOrder)
                .ToListAsync();

            List<PartCategoryDto> result;

            if (hierarchical)
            {
                // بناء الشجرة
                result = categories
                    .Where(c => c.ParentId == null)
                    .Select(c => MapCategoryWithChildren(c, categories))
                    .ToList();
            }
            else
            {
                // قائمة مسطحة
                result = categories.Select(c => new PartCategoryDto
                {
                    Id = c.Id,
                    ParentId = c.ParentId,
                    ParentNameAr = c.Parent?.NameAr,
                    NameAr = c.NameAr,
                    NameEn = c.NameEn,
                    Icon = c.Icon,
                    SortOrder = c.SortOrder,
                    Level = c.Level
                }).ToList();
            }

            return new LookupsResponse<PartCategoryDto>
            {
                Success = true,
                Data = result,
                Count = result.Count
            };
        }

        /// <summary>
        /// جلب الشركات المصنعة
        /// </summary>
        public async Task<LookupsResponse<VehicleMakeDto>> GetVehicleMakesAsync(int? vehicleTypeId = null)
        {
            var query = _context.VehicleMakes
                .Include(m => m.VehicleType)
                .Where(m => m.IsActive);

            if (vehicleTypeId.HasValue)
            {
                query = query.Where(m => m.VehicleTypeId == vehicleTypeId);
            }

            var makes = await query
                .OrderBy(m => m.SortOrder)
                .Select(m => new VehicleMakeDto
                {
                    Id = m.Id,
                    VehicleTypeId = m.VehicleTypeId,
                    VehicleTypeNameAr = m.VehicleType.NameAr,
                    NameAr = m.NameAr,
                    NameEn = m.NameEn,
                    LogoUrl = m.LogoUrl,
                    Country = m.Country,
                    SortOrder = m.SortOrder,
                    IsPopular = m.IsPopular,
                    ModelsCount = m.VehicleModels.Count(v => v.IsActive)
                })
                .ToListAsync();

            return new LookupsResponse<VehicleMakeDto>
            {
                Success = true,
                Data = makes,
                Count = makes.Count
            };
        }

        /// <summary>
        /// جلب الموديلات
        /// </summary>
        public async Task<LookupsResponse<VehicleModelDto>> GetVehicleModelsAsync(int? makeId = null)
        {
            var query = _context.VehicleModels
                .Include(m => m.Make)
                .Where(m => m.IsActive);

            if (makeId.HasValue)
            {
                query = query.Where(m => m.MakeId == makeId);
            }

            var models = await query
                .OrderBy(m => m.MakeId)
                .ThenBy(m => m.NameAr)
                .Select(m => new VehicleModelDto
                {
                    Id = m.Id,
                    MakeId = m.MakeId,
                    MakeNameAr = m.Make.NameAr,
                    NameAr = m.NameAr,
                    NameEn = m.NameEn,
                    YearFrom = m.YearFrom,
                    YearTo = m.YearTo
                })
                .ToListAsync();

            return new LookupsResponse<VehicleModelDto>
            {
                Success = true,
                Data = models,
                Count = models.Count
            };
        }

        /// <summary>
        /// جلب السنوات
        /// </summary>
        public async Task<LookupsResponse<YearDto>> GetYearsAsync()
        {
            var currentYear = DateTime.Now.Year;
            var years = Enumerable.Range(1970, currentYear - 1970 + 2)
                .OrderByDescending(y => y)
                .Select(y => new YearDto { Year = y })
                .ToList();

            return await Task.FromResult(new LookupsResponse<YearDto>
            {
                Success = true,
                Data = years,
                Count = years.Count
            });
        }

        /// <summary>
        /// جلب المدن
        /// </summary>
        public async Task<LookupsResponse<CityDto>> GetCitiesAsync()
        {
            var cities = new List<CityDto>
            {
                new() { NameAr = "الرياض", NameEn = "Riyadh" },
                new() { NameAr = "جدة", NameEn = "Jeddah" },
                new() { NameAr = "مكة المكرمة", NameEn = "Makkah" },
                new() { NameAr = "المدينة المنورة", NameEn = "Madinah" },
                new() { NameAr = "الدمام", NameEn = "Dammam" },
                new() { NameAr = "الخبر", NameEn = "Khobar" },
                new() { NameAr = "الظهران", NameEn = "Dhahran" },
                new() { NameAr = "الأحساء", NameEn = "Al-Ahsa" },
                new() { NameAr = "القطيف", NameEn = "Qatif" },
                new() { NameAr = "الجبيل", NameEn = "Jubail" },
                new() { NameAr = "الطائف", NameEn = "Taif" },
                new() { NameAr = "تبوك", NameEn = "Tabuk" },
                new() { NameAr = "بريدة", NameEn = "Buraidah" },
                new() { NameAr = "عنيزة", NameEn = "Unaizah" },
                new() { NameAr = "حائل", NameEn = "Hail" },
                new() { NameAr = "أبها", NameEn = "Abha" },
                new() { NameAr = "خميس مشيط", NameEn = "Khamis Mushait" },
                new() { NameAr = "نجران", NameEn = "Najran" },
                new() { NameAr = "جازان", NameEn = "Jazan" },
                new() { NameAr = "الباحة", NameEn = "Al-Baha" },
                new() { NameAr = "سكاكا", NameEn = "Sakaka" },
                new() { NameAr = "عرعر", NameEn = "Arar" },
                new() { NameAr = "القريات", NameEn = "Qurayyat" },
                new() { NameAr = "ينبع", NameEn = "Yanbu" },
                new() { NameAr = "رابغ", NameEn = "Rabigh" },
                new() { NameAr = "القنفذة", NameEn = "Al-Qunfudhah" }
            };

            return await Task.FromResult(new LookupsResponse<CityDto>
            {
                Success = true,
                Data = cities,
                Count = cities.Count
            });
        }

        /// <summary>
        /// جلب حالات القطع
        /// </summary>
        public LookupsResponse<PartConditionDto> GetPartConditions()
        {
            var conditions = new List<PartConditionDto>
            {
                new() { Key = "new", NameAr = "جديد", NameEn = "New" },
                new() { Key = "used", NameAr = "مستعمل", NameEn = "Used" },
                new() { Key = "refurbished", NameAr = "مجدد", NameEn = "Refurbished" }
            };

            return new LookupsResponse<PartConditionDto>
            {
                Success = true,
                Data = conditions,
                Count = conditions.Count
            };
        }

        /// <summary>
        /// جلب أنواع الضمان
        /// </summary>
        public LookupsResponse<WarrantyTypeDto> GetWarrantyTypes()
        {
            var warranties = new List<WarrantyTypeDto>
            {
                new() { Key = "none", NameAr = "بدون ضمان", NameEn = "No Warranty", Days = 0 },
                new() { Key = "week", NameAr = "أسبوع", NameEn = "1 Week", Days = 7 },
                new() { Key = "two_weeks", NameAr = "أسبوعين", NameEn = "2 Weeks", Days = 14 },
                new() { Key = "month", NameAr = "شهر", NameEn = "1 Month", Days = 30 },
                new() { Key = "two_months", NameAr = "شهرين", NameEn = "2 Months", Days = 60 },
                new() { Key = "three_months", NameAr = "3 أشهر", NameEn = "3 Months", Days = 90 },
                new() { Key = "six_months", NameAr = "6 أشهر", NameEn = "6 Months", Days = 180 },
                new() { Key = "year", NameAr = "سنة", NameEn = "1 Year", Days = 365 }
            };

            return new LookupsResponse<WarrantyTypeDto>
            {
                Success = true,
                Data = warranties,
                Count = warranties.Count
            };
        }

        /// <summary>
        /// جلب كل البيانات الأساسية
        /// </summary>
        public async Task<AllLookupsResponse> GetAllLookupsAsync()
        {
            var vehicleTypes = await GetVehicleTypesAsync();
            var partCategories = await GetPartCategoriesAsync(hierarchical: true);
            var makes = await GetVehicleMakesAsync();
            var conditions = GetPartConditions();
            var warranties = GetWarrantyTypes();
            var cities = await GetCitiesAsync();

            return new AllLookupsResponse
            {
                Success = true,
                VehicleTypes = vehicleTypes.Data,
                PartCategories = partCategories.Data,
                VehicleMakes = makes.Data,
                PartConditions = conditions.Data,
                WarrantyTypes = warranties.Data,
                Cities = cities.Data
            };
        }

        #region Helper Methods

        private PartCategoryDto MapCategoryWithChildren(Core.Entities.PartCategory category, List<Core.Entities.PartCategory> allCategories)
        {
            var children = allCategories
                .Where(c => c.ParentId == category.Id)
                .Select(c => MapCategoryWithChildren(c, allCategories))
                .ToList();

            return new PartCategoryDto
            {
                Id = category.Id,
                ParentId = category.ParentId,
                NameAr = category.NameAr,
                NameEn = category.NameEn,
                Icon = category.Icon,
                SortOrder = category.SortOrder,
                Level = category.Level,
                Children = children.Any() ? children : null
            };
        }

        #endregion
    }
}
