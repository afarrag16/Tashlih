using Tashlih.Application.DTOs.Suppliers;

namespace Tashlih.Application.Interfaces;

public interface ISuppliersService
{
    /// <summary>
    /// جلب تفاصيل مورد
    /// </summary>
    Task<SupplierDetailsResponse> GetSupplierDetailsAsync(long supplierId);

    /// <summary>
    /// جلب قائمة الموردين
    /// </summary>
    Task<SuppliersListResponse> GetSuppliersListAsync(string? city = null, int page = 1, int pageSize = 20);

    /// <summary>
    /// جلب الموردين القريبين
    /// </summary>
    Task<SuppliersNearbyResponse> GetNearbySuppliersAsync(decimal latitude, decimal longitude, double radiusKm = 10);

    Task<SupplierStatisticsResponse> GetSupplierStatisticsAsync(long supplierId, string? period = null, DateTime? fromDate = null, DateTime? toDate = null);
}