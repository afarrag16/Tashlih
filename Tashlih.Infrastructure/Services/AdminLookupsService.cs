using Microsoft.EntityFrameworkCore;
using Tashlih.Application.DTOs.Admin;
using Tashlih.Application.Interfaces;
using Tashlih.Core.Entities;
using Tashlih.Infrastructure.Models;

namespace Tashlih.Infrastructure.Services;

public class AdminLookupsService
{
    private readonly TashlihContext _context;
    private readonly IFileService _fileService;
    public AdminLookupsService(TashlihContext context, IFileService fileService)
    {
        _context = context;
        _fileService = fileService;
    }

    #region Vehicle Types (أنواع المركبات)

    public async Task<LookupResponse> AddVehicleTypeAsync(VehicleTypeRequest request)
    {
        var exists = await _context.VehicleTypes.AnyAsync(v => v.NameAr == request.NameAr);
        if (exists)
            return new LookupResponse { Success = false, Message = "Vehicle type already exists", MessageAr = "نوع المركبة موجود مسبقاً" };

        var entity = new VehicleType
        {
            NameAr = request.NameAr,
            NameEn = request.NameEn ?? "",
            SortOrder = 0,
            IsActive = true
        };

        _context.VehicleTypes.Add(entity);
        await _context.SaveChangesAsync();

        return new LookupResponse
        {
            Success = true,
            Message = "Vehicle type added successfully",
            MessageAr = "تمت إضافة نوع المركبة بنجاح",
            Data = new { entity.Id, entity.NameAr, entity.NameEn }
        };
    }

    public async Task<LookupResponse> UpdateVehicleTypeAsync(int id, VehicleTypeRequest request)
    {
        var entity = await _context.VehicleTypes.FindAsync(id);
        if (entity == null)
            return new LookupResponse { Success = false, Message = "Not found", MessageAr = "غير موجود" };

        entity.NameAr = request.NameAr;
        entity.NameEn = request.NameEn ?? "";

        await _context.SaveChangesAsync();

        return new LookupResponse
        {
            Success = true,
            Message = "Vehicle type updated successfully",
            MessageAr = "تم تعديل نوع المركبة بنجاح",
            Data = new { entity.Id, entity.NameAr, entity.NameEn }
        };
    }

    public async Task<LookupResponse> DeleteVehicleTypeAsync(int id)
    {
        var entity = await _context.VehicleTypes.FindAsync(id);
        if (entity == null)
            return new LookupResponse { Success = false, Message = "Not found", MessageAr = "غير موجود" };

        _context.VehicleTypes.Remove(entity);
        await _context.SaveChangesAsync();

        return new LookupResponse
        {
            Success = true,
            Message = "Vehicle type deleted successfully",
            MessageAr = "تم حذف نوع المركبة بنجاح"
        };
    }

    #endregion

    #region Makes (الشركات المصنعة)

    public async Task<LookupResponse> AddMakeAsync(MakeRequest request)
    {
        var exists = await _context.VehicleMakes.AnyAsync(m => m.NameAr == request.NameAr && m.VehicleTypeId == request.VehicleTypeId);
        if (exists)
            return new LookupResponse { Success = false, Message = "Make already exists", MessageAr = "الشركة المصنعة موجودة مسبقاً" };

        var entity = new VehicleMake
        {
            NameAr = request.NameAr,
            NameEn = request.NameEn ?? "",
            VehicleTypeId = request.VehicleTypeId,
            SortOrder = 0,
            IsActive = true,
            IsPopular = false
        };

        _context.VehicleMakes.Add(entity);
        await _context.SaveChangesAsync();

        return new LookupResponse
        {
            Success = true,
            Message = "Make added successfully",
            MessageAr = "تمت إضافة الشركة المصنعة بنجاح",
            Data = new { entity.Id, entity.NameAr, entity.NameEn, entity.VehicleTypeId }
        };
    }

    public async Task<LookupResponse> UpdateMakeAsync(int id, MakeRequest request)
    {
        var entity = await _context.VehicleMakes.FindAsync(id);
        if (entity == null)
            return new LookupResponse { Success = false, Message = "Not found", MessageAr = "غير موجود" };

        entity.NameAr = request.NameAr;
        entity.NameEn = request.NameEn ?? "";
        entity.VehicleTypeId = request.VehicleTypeId;

        await _context.SaveChangesAsync();

        return new LookupResponse
        {
            Success = true,
            Message = "Make updated successfully",
            MessageAr = "تم تعديل الشركة المصنعة بنجاح",
            Data = new { entity.Id, entity.NameAr, entity.NameEn, entity.VehicleTypeId }
        };
    }

    public async Task<LookupResponse> DeleteMakeAsync(int id)
    {
        var entity = await _context.VehicleMakes.FindAsync(id);
        if (entity == null)
            return new LookupResponse { Success = false, Message = "Not found", MessageAr = "غير موجود" };

        _context.VehicleMakes.Remove(entity);
        await _context.SaveChangesAsync();

        return new LookupResponse
        {
            Success = true,
            Message = "Make deleted successfully",
            MessageAr = "تم حذف الشركة المصنعة بنجاح"
        };
    }

    #endregion

    #region Models (الموديلات)

    public async Task<LookupResponse> AddModelAsync(ModelRequest request)
    {
        var exists = await _context.VehicleModels.AnyAsync(m => m.NameAr == request.NameAr && m.MakeId == request.MakeId);
        if (exists)
            return new LookupResponse { Success = false, Message = "Model already exists", MessageAr = "الموديل موجود مسبقاً" };

        var entity = new VehicleModel
        {
            NameAr = request.NameAr,
            NameEn = request.NameEn ?? "",
            MakeId = request.MakeId,
            IsActive = true
        };

        _context.VehicleModels.Add(entity);
        await _context.SaveChangesAsync();

        return new LookupResponse
        {
            Success = true,
            Message = "Model added successfully",
            MessageAr = "تمت إضافة الموديل بنجاح",
            Data = new { entity.Id, entity.NameAr, entity.NameEn, entity.MakeId }
        };
    }

    public async Task<LookupResponse> UpdateModelAsync(int id, ModelRequest request)
    {
        var entity = await _context.VehicleModels.FindAsync(id);
        if (entity == null)
            return new LookupResponse { Success = false, Message = "Not found", MessageAr = "غير موجود" };

        entity.NameAr = request.NameAr;
        entity.NameEn = request.NameEn ?? "";
        entity.MakeId = request.MakeId;

        await _context.SaveChangesAsync();

        return new LookupResponse
        {
            Success = true,
            Message = "Model updated successfully",
            MessageAr = "تم تعديل الموديل بنجاح",
            Data = new { entity.Id, entity.NameAr, entity.NameEn, entity.MakeId }
        };
    }

    public async Task<LookupResponse> DeleteModelAsync(int id)
    {
        var entity = await _context.VehicleModels.FindAsync(id);
        if (entity == null)
            return new LookupResponse { Success = false, Message = "Not found", MessageAr = "غير موجود" };

        _context.VehicleModels.Remove(entity);
        await _context.SaveChangesAsync();

        return new LookupResponse
        {
            Success = true,
            Message = "Model deleted successfully",
            MessageAr = "تم حذف الموديل بنجاح"
        };
    }

    #endregion

    #region Categories (التصنيفات)

    public async Task<LookupResponse> AddCategoryAsync(CategoryRequest request)
    {
        var exists = await _context.PartCategories.AnyAsync(c => c.NameAr == request.NameAr);
        if (exists)
            return new LookupResponse { Success = false, Message = "Category already exists", MessageAr = "التصنيف موجود مسبقاً" };

        // ✅ رفع الأيقونة
        string? iconUrl = null;
        if (request.Icon != null)
        {
            iconUrl = await _fileService.UploadFileAsync(request.Icon, "categories/icons");
        }

        var entity = new PartCategory
        {
            NameAr = request.NameAr,
            NameEn = request.NameEn,
            Icon = iconUrl,  // ✅ رابط الصورة
            SortOrder = 0,
            Level = 0,
            IsActive = true
        };

        _context.PartCategories.Add(entity);
        await _context.SaveChangesAsync();

        return new LookupResponse
        {
            Success = true,
            Message = "Category added successfully",
            MessageAr = "تمت إضافة التصنيف بنجاح",
            Data = new { entity.Id, entity.NameAr, entity.NameEn, entity.Icon }
        };
    }

    public async Task<LookupResponse> UpdateCategoryAsync(long id, CategoryRequest request)
    {
        var entity = await _context.PartCategories.FindAsync(id);
        if (entity == null)
            return new LookupResponse { Success = false, Message = "Not found", MessageAr = "غير موجود" };

        entity.NameAr = request.NameAr;
        entity.NameEn = request.NameEn;

        // ✅ رفع الأيقونة الجديدة (لو موجودة)
        if (request.Icon != null)
        {
            entity.Icon = await _fileService.UploadFileAsync(request.Icon, "categories/icons");
        }

        await _context.SaveChangesAsync();

        return new LookupResponse
        {
            Success = true,
            Message = "Category updated successfully",
            MessageAr = "تم تعديل التصنيف بنجاح",
            Data = new { entity.Id, entity.NameAr, entity.NameEn, entity.Icon }
        };
    }

    public async Task<LookupResponse> DeleteCategoryAsync(long id)
    {
        var entity = await _context.PartCategories.FindAsync(id);
        if (entity == null)
            return new LookupResponse { Success = false, Message = "Not found", MessageAr = "غير موجود" };

        _context.PartCategories.Remove(entity);
        await _context.SaveChangesAsync();

        return new LookupResponse
        {
            Success = true,
            Message = "Category deleted successfully",
            MessageAr = "تم حذف التصنيف بنجاح"
        };
    }

    #endregion

    #region Subcategories (التصنيفات الفرعية)

    public async Task<LookupResponse> AddSubcategoryAsync(SubcategoryRequest request)
    {
        var exists = await _context.VehicleSubcategories.AnyAsync(s => s.NameAr == request.NameAr && s.VehicleTypeId == request.VehicleTypeId);
        if (exists)
            return new LookupResponse { Success = false, Message = "Subcategory already exists", MessageAr = "التصنيف الفرعي موجود مسبقاً" };

        var entity = new VehicleSubcategory
        {
            NameAr = request.NameAr,
            NameEn = request.NameEn ?? "",
            VehicleTypeId = request.VehicleTypeId,
            SortOrder = 0,
            IsActive = true
        };

        _context.VehicleSubcategories.Add(entity);
        await _context.SaveChangesAsync();

        return new LookupResponse
        {
            Success = true,
            Message = "Subcategory added successfully",
            MessageAr = "تمت إضافة التصنيف الفرعي بنجاح",
            Data = new { entity.Id, entity.NameAr, entity.NameEn, entity.VehicleTypeId }
        };
    }

    public async Task<LookupResponse> UpdateSubcategoryAsync(int id, SubcategoryRequest request)
    {
        var entity = await _context.VehicleSubcategories.FindAsync(id);
        if (entity == null)
            return new LookupResponse { Success = false, Message = "Not found", MessageAr = "غير موجود" };

        entity.NameAr = request.NameAr;
        entity.NameEn = request.NameEn ?? "";
        entity.VehicleTypeId = request.VehicleTypeId;

        await _context.SaveChangesAsync();

        return new LookupResponse
        {
            Success = true,
            Message = "Subcategory updated successfully",
            MessageAr = "تم تعديل التصنيف الفرعي بنجاح",
            Data = new { entity.Id, entity.NameAr, entity.NameEn, entity.VehicleTypeId }
        };
    }

    public async Task<LookupResponse> DeleteSubcategoryAsync(int id)
    {
        var entity = await _context.VehicleSubcategories.FindAsync(id);
        if (entity == null)
            return new LookupResponse { Success = false, Message = "Not found", MessageAr = "غير موجود" };

        _context.VehicleSubcategories.Remove(entity);
        await _context.SaveChangesAsync();

        return new LookupResponse
        {
            Success = true,
            Message = "Subcategory deleted successfully",
            MessageAr = "تم حذف التصنيف الفرعي بنجاح"
        };
    }

    #endregion

    #region Cities (المدن)

    public async Task<LookupResponse> AddCityAsync(CityRequest request)
    {
        var exists = await _context.Cities.AnyAsync(c => c.NameAr == request.NameAr);
        if (exists)
            return new LookupResponse { Success = false, Message = "City already exists", MessageAr = "المدينة موجودة مسبقاً" };

        var entity = new City
        {
            NameAr = request.NameAr,
            NameEn = request.NameEn ?? "",
            IsActive = true
        };

        _context.Cities.Add(entity);
        await _context.SaveChangesAsync();

        return new LookupResponse
        {
            Success = true,
            Message = "City added successfully",
            MessageAr = "تمت إضافة المدينة بنجاح",
            Data = new { entity.Id, entity.NameAr, entity.NameEn }
        };
    }

    public async Task<LookupResponse> UpdateCityAsync(int id, CityRequest request)
    {
        var entity = await _context.Cities.FindAsync(id);
        if (entity == null)
            return new LookupResponse { Success = false, Message = "Not found", MessageAr = "غير موجود" };

        entity.NameAr = request.NameAr;
        entity.NameEn = request.NameEn ?? "";

        await _context.SaveChangesAsync();

        return new LookupResponse
        {
            Success = true,
            Message = "City updated successfully",
            MessageAr = "تم تعديل المدينة بنجاح",
            Data = new { entity.Id, entity.NameAr, entity.NameEn }
        };
    }

    public async Task<LookupResponse> DeleteCityAsync(int id)
    {
        var entity = await _context.Cities.FindAsync(id);
        if (entity == null)
            return new LookupResponse { Success = false, Message = "Not found", MessageAr = "غير موجود" };

        _context.Cities.Remove(entity);
        await _context.SaveChangesAsync();

        return new LookupResponse
        {
            Success = true,
            Message = "City deleted successfully",
            MessageAr = "تم حذف المدينة بنجاح"
        };
    }

    #endregion

    #region Part Conditions (حالات القطع)

    public async Task<LookupResponse> AddPartConditionAsync(PartConditionRequest request)
    {
        var exists = await _context.PartConditions.AnyAsync(p => p.Key == request.Key || p.NameAr == request.NameAr);
        if (exists)
            return new LookupResponse { Success = false, Message = "Part condition already exists", MessageAr = "حالة القطعة موجودة مسبقاً" };

        var entity = new PartCondition
        {
            Key = request.Key,
            NameAr = request.NameAr,
            NameEn = request.NameEn
        };

        _context.PartConditions.Add(entity);
        await _context.SaveChangesAsync();

        return new LookupResponse
        {
            Success = true,
            Message = "Part condition added successfully",
            MessageAr = "تمت إضافة حالة القطعة بنجاح",
            Data = new { entity.Id, entity.Key, entity.NameAr, entity.NameEn }
        };
    }

    public async Task<LookupResponse> UpdatePartConditionAsync(long id, PartConditionRequest request)
    {
        var entity = await _context.PartConditions.FindAsync(id);
        if (entity == null)
            return new LookupResponse { Success = false, Message = "Not found", MessageAr = "غير موجود" };

        entity.Key = request.Key;
        entity.NameAr = request.NameAr;
        entity.NameEn = request.NameEn;

        await _context.SaveChangesAsync();

        return new LookupResponse
        {
            Success = true,
            Message = "Part condition updated successfully",
            MessageAr = "تم تعديل حالة القطعة بنجاح",
            Data = new { entity.Id, entity.Key, entity.NameAr, entity.NameEn }
        };
    }

    public async Task<LookupResponse> DeletePartConditionAsync(long id)
    {
        var entity = await _context.PartConditions.FindAsync(id);
        if (entity == null)
            return new LookupResponse { Success = false, Message = "Not found", MessageAr = "غير موجود" };

        _context.PartConditions.Remove(entity);
        await _context.SaveChangesAsync();

        return new LookupResponse
        {
            Success = true,
            Message = "Part condition deleted successfully",
            MessageAr = "تم حذف حالة القطعة بنجاح"
        };
    }

    #endregion

    #region Warranty Types (أنواع الضمان)

    public async Task<LookupResponse> AddWarrantyTypeAsync(WarrantyTypeRequest request)
    {
        var exists = await _context.WarrantyTypes.AnyAsync(w => w.Key == request.Key || w.NameAr == request.NameAr);
        if (exists)
            return new LookupResponse { Success = false, Message = "Warranty type already exists", MessageAr = "نوع الضمان موجود مسبقاً" };

        var entity = new WarrantyType
        {
            Key = request.Key,
            NameAr = request.NameAr,
            NameEn = request.NameEn,
            Days = request.Days
        };

        _context.WarrantyTypes.Add(entity);
        await _context.SaveChangesAsync();

        return new LookupResponse
        {
            Success = true,
            Message = "Warranty type added successfully",
            MessageAr = "تمت إضافة نوع الضمان بنجاح",
            Data = new { entity.Id, entity.Key, entity.NameAr, entity.NameEn, entity.Days }
        };
    }

    public async Task<LookupResponse> UpdateWarrantyTypeAsync(long id, WarrantyTypeRequest request)
    {
        var entity = await _context.WarrantyTypes.FindAsync(id);
        if (entity == null)
            return new LookupResponse { Success = false, Message = "Not found", MessageAr = "غير موجود" };

        entity.Key = request.Key;
        entity.NameAr = request.NameAr;
        entity.NameEn = request.NameEn;
        entity.Days = request.Days;

        await _context.SaveChangesAsync();

        return new LookupResponse
        {
            Success = true,
            Message = "Warranty type updated successfully",
            MessageAr = "تم تعديل نوع الضمان بنجاح",
            Data = new { entity.Id, entity.Key, entity.NameAr, entity.NameEn, entity.Days }
        };
    }

    public async Task<LookupResponse> DeleteWarrantyTypeAsync(long id)
    {
        var entity = await _context.WarrantyTypes.FindAsync(id);
        if (entity == null)
            return new LookupResponse { Success = false, Message = "Not found", MessageAr = "غير موجود" };

        _context.WarrantyTypes.Remove(entity);
        await _context.SaveChangesAsync();

        return new LookupResponse
        {
            Success = true,
            Message = "Warranty type deleted successfully",
            MessageAr = "تم حذف نوع الضمان بنجاح"
        };
    }

    #endregion

    #region Years (السنوات)

    public async Task<LookupResponse> AddYearAsync(YearRequest request)
    {
        var exists = await _context.Years.AnyAsync(y => y.YearValue == request.Year);
        if (exists)
            return new LookupResponse { Success = false, Message = "Year already exists", MessageAr = "السنة موجودة مسبقاً" };

        var entity = new Year
        {
            YearValue = request.Year
        };

        _context.Years.Add(entity);
        await _context.SaveChangesAsync();

        return new LookupResponse
        {
            Success = true,
            Message = "Year added successfully",
            MessageAr = "تمت إضافة السنة بنجاح",
            Data = new { entity.Id, Year = entity.YearValue }
        };
    }

    public async Task<LookupResponse> UpdateYearAsync(long id, YearRequest request)
    {
        var entity = await _context.Years.FindAsync(id);
        if (entity == null)
            return new LookupResponse { Success = false, Message = "Not found", MessageAr = "غير موجود" };

        entity.YearValue = request.Year;

        await _context.SaveChangesAsync();

        return new LookupResponse
        {
            Success = true,
            Message = "Year updated successfully",
            MessageAr = "تم تعديل السنة بنجاح",
            Data = new { entity.Id, Year = entity.YearValue }
        };
    }

    public async Task<LookupResponse> DeleteYearAsync(long id)
    {
        var entity = await _context.Years.FindAsync(id);
        if (entity == null)
            return new LookupResponse { Success = false, Message = "Not found", MessageAr = "غير موجود" };

        _context.Years.Remove(entity);
        await _context.SaveChangesAsync();

        return new LookupResponse
        {
            Success = true,
            Message = "Year deleted successfully",
            MessageAr = "تم حذف السنة بنجاح"
        };
    }

    #endregion
}