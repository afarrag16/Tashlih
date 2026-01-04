using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tashlih.Application.DTOs.Admin;
using Tashlih.Infrastructure.Services;

namespace Tashlih.Api.Controllers;

[ApiController]
[Route("api/Admin/lookups")]
[Authorize]
public class AdminLookupsController : ControllerBase
{
    private readonly AdminLookupsService _lookupsService;

    public AdminLookupsController(AdminLookupsService lookupsService)
    {
        _lookupsService = lookupsService;
    }

    private bool IsAdmin()
    {
        return User.FindFirst("user_type")?.Value == "admin";
    }

    #region Vehicle Types (أنواع المركبات)

    [HttpPost("vehicle-types")]
    public async Task<IActionResult> AddVehicleType([FromBody] VehicleTypeRequest request)
    {
        if (!IsAdmin()) return Forbid();
        var result = await _lookupsService.AddVehicleTypeAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("vehicle-types/{id}")]
    public async Task<IActionResult> UpdateVehicleType(int id, [FromBody] VehicleTypeRequest request)
    {
        if (!IsAdmin()) return Forbid();
        var result = await _lookupsService.UpdateVehicleTypeAsync(id, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("vehicle-types/{id}")]
    public async Task<IActionResult> DeleteVehicleType(int id)
    {
        if (!IsAdmin()) return Forbid();
        var result = await _lookupsService.DeleteVehicleTypeAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion

    #region Makes (الشركات المصنعة)

    [HttpPost("makes")]
    public async Task<IActionResult> AddMake([FromBody] MakeRequest request)
    {
        if (!IsAdmin()) return Forbid();
        var result = await _lookupsService.AddMakeAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("makes/{id}")]
    public async Task<IActionResult> UpdateMake(int id, [FromBody] MakeRequest request)
    {
        if (!IsAdmin()) return Forbid();
        var result = await _lookupsService.UpdateMakeAsync(id, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("makes/{id}")]
    public async Task<IActionResult> DeleteMake(int id)
    {
        if (!IsAdmin()) return Forbid();
        var result = await _lookupsService.DeleteMakeAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion

    #region Models (الموديلات)

    [HttpPost("models")]
    public async Task<IActionResult> AddModel([FromBody] ModelRequest request)
    {
        if (!IsAdmin()) return Forbid();
        var result = await _lookupsService.AddModelAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("models/{id}")]
    public async Task<IActionResult> UpdateModel(int id, [FromBody] ModelRequest request)
    {
        if (!IsAdmin()) return Forbid();
        var result = await _lookupsService.UpdateModelAsync(id, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("models/{id}")]
    public async Task<IActionResult> DeleteModel(int id)
    {
        if (!IsAdmin()) return Forbid();
        var result = await _lookupsService.DeleteModelAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion

    #region Categories (التصنيفات)

    [HttpPost("categories")]
    public async Task<IActionResult> AddCategory([FromForm] CategoryRequest request)
    {
        if (!IsAdmin()) return Forbid();
        var result = await _lookupsService.AddCategoryAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("categories/{id}")]
    public async Task<IActionResult> UpdateCategory(long id, [FromForm] CategoryRequest request)
    {
        if (!IsAdmin()) return Forbid();
        var result = await _lookupsService.UpdateCategoryAsync(id, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("categories/{id}")]
    public async Task<IActionResult> DeleteCategory(long id)
    {
        if (!IsAdmin()) return Forbid();
        var result = await _lookupsService.DeleteCategoryAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion

    #region Subcategories (التصنيفات الفرعية)

    [HttpPost("subcategories")]
    public async Task<IActionResult> AddSubcategory([FromBody] SubcategoryRequest request)
    {
        if (!IsAdmin()) return Forbid();
        var result = await _lookupsService.AddSubcategoryAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("subcategories/{id}")]
    public async Task<IActionResult> UpdateSubcategory(int id, [FromBody] SubcategoryRequest request)
    {
        if (!IsAdmin()) return Forbid();
        var result = await _lookupsService.UpdateSubcategoryAsync(id, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("subcategories/{id}")]
    public async Task<IActionResult> DeleteSubcategory(int id)
    {
        if (!IsAdmin()) return Forbid();
        var result = await _lookupsService.DeleteSubcategoryAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion

    #region Cities (المدن)

    [HttpPost("cities")]
    public async Task<IActionResult> AddCity([FromBody] CityRequest request)
    {
        if (!IsAdmin()) return Forbid();
        var result = await _lookupsService.AddCityAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("cities/{id}")]
    public async Task<IActionResult> UpdateCity(int id, [FromBody] CityRequest request)
    {
        if (!IsAdmin()) return Forbid();
        var result = await _lookupsService.UpdateCityAsync(id, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("cities/{id}")]
    public async Task<IActionResult> DeleteCity(int id)
    {
        if (!IsAdmin()) return Forbid();
        var result = await _lookupsService.DeleteCityAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion

    #region Part Conditions (حالات القطع)

    [HttpPost("part-conditions")]
    public async Task<IActionResult> AddPartCondition([FromBody] PartConditionRequest request)
    {
        if (!IsAdmin()) return Forbid();
        var result = await _lookupsService.AddPartConditionAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("part-conditions/{id}")]
    public async Task<IActionResult> UpdatePartCondition(long id, [FromBody] PartConditionRequest request)
    {
        if (!IsAdmin()) return Forbid();
        var result = await _lookupsService.UpdatePartConditionAsync(id, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("part-conditions/{id}")]
    public async Task<IActionResult> DeletePartCondition(long id)
    {
        if (!IsAdmin()) return Forbid();
        var result = await _lookupsService.DeletePartConditionAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion

    #region Warranty Types (أنواع الضمان)

    [HttpPost("warranty-types")]
    public async Task<IActionResult> AddWarrantyType([FromBody] WarrantyTypeRequest request)
    {
        if (!IsAdmin()) return Forbid();
        var result = await _lookupsService.AddWarrantyTypeAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("warranty-types/{id}")]
    public async Task<IActionResult> UpdateWarrantyType(long id, [FromBody] WarrantyTypeRequest request)
    {
        if (!IsAdmin()) return Forbid();
        var result = await _lookupsService.UpdateWarrantyTypeAsync(id, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("warranty-types/{id}")]
    public async Task<IActionResult> DeleteWarrantyType(long id)
    {
        if (!IsAdmin()) return Forbid();
        var result = await _lookupsService.DeleteWarrantyTypeAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion

    #region Years (السنوات)

    [HttpPost("years")]
    public async Task<IActionResult> AddYear([FromBody] YearRequest request)
    {
        if (!IsAdmin()) return Forbid();
        var result = await _lookupsService.AddYearAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("years/{id}")]
    public async Task<IActionResult> UpdateYear(long id, [FromBody] YearRequest request)
    {
        if (!IsAdmin()) return Forbid();
        var result = await _lookupsService.UpdateYearAsync(id, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("years/{id}")]
    public async Task<IActionResult> DeleteYear(long id)
    {
        if (!IsAdmin()) return Forbid();
        var result = await _lookupsService.DeleteYearAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion
}
