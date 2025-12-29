using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using Tashlih.Application.DTOs.Parts;
using Tashlih.Application.Interfaces;

namespace Tashlih.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PartsController : ControllerBase
    {
        private readonly IPartsService _partsService;

        public PartsController(IPartsService partsService)
        {
            _partsService = partsService;
        }

        // ==================== Endpoints للمورد ====================

        /// <summary>
        /// إضافة قطعة جديدة (للمورد)
        /// </summary>
        [HttpPost]
        [Authorize]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreatePart([FromForm] CreatePartRequest request)
        {
            if (!IsSupplier())
                return Forbid();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var supplierId = GetUserId();
            var result = await _partsService.CreatePartAsync(supplierId, request);

            if (!result.Success)
                return BadRequest(result);

            return CreatedAtAction(nameof(GetPartById), new { id = result.Part?.Id }, result);
        }

        /// <summary>
        /// تعديل قطعة (للمورد)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdatePart(long id, [FromBody] UpdatePartRequest request)
        {
            if (!IsSupplier())
                return Forbid();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var supplierId = GetUserId();
            var result = await _partsService.UpdatePartAsync(supplierId, id, request);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// حذف قطعة (للمورد)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeletePart(long id)
        {
            if (!IsSupplier())
                return Forbid();

            var supplierId = GetUserId();
            var result = await _partsService.DeletePartAsync(supplierId, id);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// عرض قطع المورد
        /// </summary>
        [HttpGet("my-parts")]
        [Authorize]
        public async Task<IActionResult> GetMyParts([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? status = null)
        {
            if (!IsSupplier())
                return Forbid();

            var supplierId = GetUserId();
            var result = await _partsService.GetSupplierPartsAsync(supplierId, page, pageSize, status);

            return Ok(result);
        }

        /// <summary>
        /// إضافة صورة للقطعة (للمورد)
        /// </summary>
        [HttpPost("{id}/images")]
        [Authorize]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> AddPartImage(long id, [FromForm] AddPartImageRequest request)
        {
            if (!IsSupplier())
                return Forbid();

            var supplierId = GetUserId();
            var result = await _partsService.AddPartImageAsync(supplierId, id, request);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

       

        /// <summary>
        /// تعيين صورة رئيسية (للمورد)
        /// </summary>
        [HttpPut("{partId}/images/{imageId}/primary")]
        [Authorize]
        public async Task<IActionResult> SetPrimaryImage(long partId, long imageId)
        {
            if (!IsSupplier())
                return Forbid();

            var supplierId = GetUserId();
            var result = await _partsService.SetPrimaryImageAsync(supplierId, partId, imageId);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        // ==================== Endpoints للعميل ====================

        /// <summary>
        /// عرض كل القطع المتاحة
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllParts([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var result = await _partsService.GetAllPartsAsync(page, pageSize);
            return Ok(result);
        }

        /// <summary>
        /// عرض تفاصيل قطعة
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPartById(long id)
        {
            // زيادة عداد المشاهدات
            await _partsService.IncrementViewCountAsync(id);

            var result = await _partsService.GetPartByIdAsync(id);

            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        /// <summary>
        /// البحث عن قطع
        /// </summary>
        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<IActionResult> SearchParts([FromQuery] SearchPartsRequest request)
        {
            var result = await _partsService.SearchWithFiltersAsync(request);
            return Ok(result);
        }

       

        /// <summary>
        /// عرض قطع حسب التصنيف
        /// </summary>
        [HttpGet("category/{categoryId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPartsByCategory(long categoryId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var result = await _partsService.GetPartsByCategoryAsync(categoryId, page, pageSize);
            return Ok(result);
        }

        // <summary>
        /// عرض قطع حسب المورد
        /// </summary>
        [HttpGet("supplier/{supplierId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPartsBySupplier(long supplierId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var result = await _partsService.GetPartsBySupplierAsync(supplierId, page, pageSize);
            return Ok(result);
        }

        /// <summary>
        /// عرض القطع المميزة
        /// </summary>
        [HttpGet("featured")]
        [AllowAnonymous]
        public async Task<IActionResult> GetFeaturedParts([FromQuery] int count = 10)
        {
            var result = await _partsService.GetFeaturedPartsAsync(count);
            return Ok(result);
        }

        /// <summary>
        /// عرض أحدث القطع
        /// </summary>
        [HttpGet("latest")]
        [AllowAnonymous]
        public async Task<IActionResult> GetLatestParts([FromQuery] int count = 10)
        {
            var result = await _partsService.GetLatestPartsAsync(count);
            return Ok(result);
        }

        // ==================== Helper Methods ====================

        private long GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return long.TryParse(userIdClaim, out var id) ? id : 0;
        }

        private bool IsSupplier()
        {
            return User.FindFirst("user_type")?.Value == "supplier";
        }
    }
}