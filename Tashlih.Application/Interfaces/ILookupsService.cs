using System.Threading.Tasks;
using Tashlih.Application.DTOs.Lookups;

namespace Tashlih.Application.Interfaces
{
    public interface ILookupsService
    {
        // أنواع المركبات
        Task<LookupsResponse<VehicleTypeDto>> GetVehicleTypesAsync();

        // التصنيفات الفرعية
        Task<LookupsResponse<VehicleSubcategoryDto>> GetSubcategoriesAsync(int? vehicleTypeId = null);

        // فئات القطع
        Task<LookupsResponse<PartCategoryDto>> GetPartCategoriesAsync(bool hierarchical = false);

        // الشركات المصنعة
        Task<LookupsResponse<VehicleMakeDto>> GetVehicleMakesAsync(int? vehicleTypeId = null);

        // الموديلات
        Task<LookupsResponse<VehicleModelDto>> GetVehicleModelsAsync(int? makeId = null);

        // السنوات
        Task<LookupsResponse<YearDto>> GetYearsAsync();

        // المدن
        Task<LookupsResponse<CityDto>> GetCitiesAsync();

        // حالات القطع
        LookupsResponse<PartConditionDto> GetPartConditions();

        // أنواع الضمان
        LookupsResponse<WarrantyTypeDto> GetWarrantyTypes();

        // كل البيانات
        Task<AllLookupsResponse> GetAllLookupsAsync();
    }
}
