using System.Threading.Tasks;
using Tashlih.Application.DTOs.Parts;

namespace Tashlih.Application.Interfaces
{
    public interface IPartsService
    {
        // ==================== للمورد ====================

        /// <summary>
        /// إضافة قطعة جديدة
        /// </summary>
        Task<PartResponse> CreatePartAsync(long supplierId, CreatePartRequest request);

        /// <summary>
        /// تعديل قطعة
        /// </summary>
        Task<PartResponse> UpdatePartAsync(long supplierId, long partId, UpdatePartRequest request);

        /// <summary>
        /// حذف قطعة
        /// </summary>
        Task<PartResponse> DeletePartAsync(long supplierId, long partId);

        /// <summary>
        /// عرض قطع المورد
        /// </summary>
        Task<PartsListResponse> GetSupplierPartsAsync(long supplierId, int page = 1, int pageSize = 20, string? status = null);

        /// <summary>
        /// إضافة صورة للقطعة
        /// </summary>
        Task<PartResponse> AddPartImageAsync(long supplierId, long partId, AddPartImageRequest request);

        /// <summary>
        /// حذف صورة من القطعة
        /// </summary>
        Task<PartResponse> DeletePartImageAsync(long supplierId, long partId, long imageId);

        /// <summary>
        /// تعيين صورة رئيسية
        /// </summary>
        Task<PartResponse> SetPrimaryImageAsync(long supplierId, long partId, long imageId);

        // ==================== للعميل ====================

        /// <summary>
        /// عرض كل القطع المتاحة
        /// </summary>
        Task<PartsListResponse> GetAllPartsAsync(int page = 1, int pageSize = 20);

        /// <summary>
        /// عرض تفاصيل قطعة
        /// </summary>
        Task<PartResponse> GetPartByIdAsync(long partId);

        /// <summary>
        /// البحث عن قطع
        /// </summary>
        Task<PartsListResponse> SearchPartsAsync(SearchPartsRequest request);

        /// <summary>
        /// عرض قطع حسب التصنيف
        /// </summary>
        Task<PartsListResponse> GetPartsByCategoryAsync(long categoryId, int page = 1, int pageSize = 20);

        /// <summary>
        /// عرض قطع حسب المتجر
        /// </summary>
        Task<PartsListResponse> GetPartsByShopAsync(long shopId, int page = 1, int pageSize = 20);

        /// <summary>
        /// عرض القطع المميزة
        /// </summary>
        Task<PartsListResponse> GetFeaturedPartsAsync(int count = 10);

        /// <summary>
        /// عرض أحدث القطع
        /// </summary>
        Task<PartsListResponse> GetLatestPartsAsync(int count = 10);

        /// <summary>
        /// زيادة عداد المشاهدات
        /// </summary>
        Task IncrementViewCountAsync(long partId);
    }
}
