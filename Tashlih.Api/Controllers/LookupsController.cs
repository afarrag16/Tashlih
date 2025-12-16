using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Tashlih.Application.Interfaces;

namespace Tashlih.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class LookupsController : ControllerBase
    {
        private readonly ILookupsService _lookupsService;

        public LookupsController(ILookupsService lookupsService)
        {
            _lookupsService = lookupsService;
        }

        /// <summary>
        /// جلب كل البيانات الأساسية دفعة واحدة
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllLookups()
        {
            var result = await _lookupsService.GetAllLookupsAsync();
            return Ok(result);
        }

        /// <summary>
        /// جلب أنواع المركبات
        /// </summary>
        [HttpGet("vehicle-types")]
        public async Task<IActionResult> GetVehicleTypes()
        {
            var result = await _lookupsService.GetVehicleTypesAsync();
            return Ok(result);
        }

        /// <summary>
        /// جلب التصنيفات الفرعية للمركبات
        /// </summary>
        /// <param name="vehicleTypeId">معرف نوع المركبة (اختياري)</param>
        [HttpGet("subcategories")]
        public async Task<IActionResult> GetSubcategories([FromQuery] int? vehicleTypeId = null)
        {
            var result = await _lookupsService.GetSubcategoriesAsync(vehicleTypeId);
            return Ok(result);
        }

        /// <summary>
        /// جلب التصنيفات الفرعية لنوع مركبة محدد
        /// </summary>
        [HttpGet("vehicle-types/{vehicleTypeId}/subcategories")]
        public async Task<IActionResult> GetSubcategoriesByType(int vehicleTypeId)
        {
            var result = await _lookupsService.GetSubcategoriesAsync(vehicleTypeId);
            return Ok(result);
        }

        

        /// <summary>
        /// جلب الشركات المصنعة
        /// </summary>
        /// <param name="vehicleTypeId">معرف نوع المركبة (اختياري)</param>
        [HttpGet("makes")]
        public async Task<IActionResult> GetVehicleMakes([FromQuery] int? vehicleTypeId = null)
        {
            var result = await _lookupsService.GetVehicleMakesAsync(vehicleTypeId);
            return Ok(result);
        }

        /// <summary>
        /// جلب الشركات المصنعة لنوع مركبة محدد
        /// </summary>
        [HttpGet("vehicle-types/{vehicleTypeId}/makes")]
        public async Task<IActionResult> GetMakesByType(int vehicleTypeId)
        {
            var result = await _lookupsService.GetVehicleMakesAsync(vehicleTypeId);
            return Ok(result);
        }

        /// <summary>
        /// جلب الموديلات
        /// </summary>
        /// <param name="makeId">معرف الشركة المصنعة (اختياري)</param>
        [HttpGet("models")]
        public async Task<IActionResult> GetVehicleModels([FromQuery] int? makeId = null)
        {
            var result = await _lookupsService.GetVehicleModelsAsync(makeId);
            return Ok(result);
        }

        /// <summary>
        /// جلب موديلات شركة محددة
        /// </summary>
        [HttpGet("makes/{makeId}/models")]
        public async Task<IActionResult> GetModelsByMake(int makeId)
        {
            var result = await _lookupsService.GetVehicleModelsAsync(makeId);
            return Ok(result);
        }

        /// <summary>
        /// جلب السنوات
        /// </summary>
        [HttpGet("years")]
        public async Task<IActionResult> GetYears()
        {
            var result = await _lookupsService.GetYearsAsync();
            return Ok(result);
        }

        /// <summary>
        /// جلب المدن
        /// </summary>
        [HttpGet("cities")]
        public async Task<IActionResult> GetCities()
        {
            var result = await _lookupsService.GetCitiesAsync();
            return Ok(result);
        }

        /// <summary>
        /// جلب حالات القطع
        /// </summary>
        [HttpGet("part-conditions")]
        public IActionResult GetPartConditions()
        {
            var result = _lookupsService.GetPartConditions();
            return Ok(result);
        }

        /// <summary>
        /// جلب أنواع الضمان
        /// </summary>
        [HttpGet("warranty-types")]
        public IActionResult GetWarrantyTypes()
        {
            var result = _lookupsService.GetWarrantyTypes();
            return Ok(result);
        }

        /// <summary>
        /// جلب فئات القطع
        /// </summary>
        /// <param name="hierarchical">هل تريد البيانات بشكل شجري؟</param>
        [HttpGet("part-categories")]
        public async Task<IActionResult> GetPartCategories([FromQuery] bool hierarchical = false)
        {
            var result = await _lookupsService.GetPartCategoriesAsync(hierarchical);
            return Ok(result);
        }
    }
}
