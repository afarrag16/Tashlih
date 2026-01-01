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
            var years = await _context.Years
                .OrderByDescending(y => y.YearValue)
                .Select(y => new YearDto
                {
                    Id = (int)y.Id,
                    Year = y.YearValue
                })
                .ToListAsync();

            return new LookupsResponse<YearDto>
            {
                Success = true,
                Data = years,
                Count = years.Count
            };
        }

        /// <summary>
        /// جلب المدن
        /// </summary>
        public async Task<LookupsResponse<CityDto>> GetCitiesAsync()
        {
            var cities = await _context.Cities
                .Where(c => c.IsActive)
                .OrderBy(c => c.NameAr)
                .Select(c => new CityDto
                {
                    Id = c.Id,
                    NameAr = c.NameAr,
                    NameEn = c.NameEn
                })
                .ToListAsync();

            return new LookupsResponse<CityDto>
            {
                Success = true,
                Data = cities,
                Count = cities.Count
            };
        }

        /// <summary>
        /// جلب حالات القطع
        /// </summary>
        public async Task<LookupsResponse<PartConditionDto>> GetPartConditionsAsync()
        {
            var conditions = await _context.PartConditions
                .OrderBy(c => c.Id)
                .Select(c => new PartConditionDto
                {
                    Id = (int)c.Id,
                    Key = c.Key,
                    NameAr = c.NameAr,
                    NameEn = c.NameEn
                })
                .ToListAsync();

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
        public async Task<LookupsResponse<WarrantyTypeDto>> GetWarrantyTypesAsync()
        {
            var warranties = await _context.WarrantyTypes
                .OrderBy(w => w.Days)
                .Select(w => new WarrantyTypeDto
                {
                    Id = (int)w.Id,
                    Key = w.Key,
                    NameAr = w.NameAr,
                    NameEn = w.NameEn,
                    Days = w.Days
                })
                .ToListAsync();

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
            var conditions = await GetPartConditionsAsync();
            var warranties = await GetWarrantyTypesAsync();
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
