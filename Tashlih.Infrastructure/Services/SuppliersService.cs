using Microsoft.EntityFrameworkCore;
using Tashlih.Application.DTOs.Suppliers;
using Tashlih.Application.Interfaces;
using Tashlih.Core.Entities ;
using Tashlih.Infrastructure.Models;

namespace Tashlih.Infrastructure.Services;

public class SuppliersService : ISuppliersService
{
    private readonly TashlihContext _context;

    public SuppliersService(TashlihContext context)
    {
        _context = context;
    }

    /// <summary>
    /// جلب تفاصيل مورد
    /// </summary>
    public async Task<SupplierDetailsResponse> GetSupplierDetailsAsync(long supplierId)
    {
        var supplier = await _context.SupplierProfiles
            .FirstOrDefaultAsync(s => s.Id == supplierId
                && s.IsVerified
                && s.Status == "active"
                && s.DeletedAt == null);

        if (supplier == null)
        {
            return new SupplierDetailsResponse
            {
                Success = false,
                Message = "Supplier not found",
                MessageAr = "المورد غير موجود"
            };
        }

        var partsCount = await _context.Parts
            .CountAsync(p => p.SupplierId == supplierId && p.DeletedAt == null && p.Status == "available");

        return new SupplierDetailsResponse
        {
            Success = true,
            Supplier = new SupplierDetailsDto
            {
                Id = supplier.Id,
                FullName = supplier.FullName,
                BusinessNameAr = supplier.BusinessNameAr,
                BusinessNameEn = supplier.BusinessNameEn,
                Description = supplier.Description,
                BusinessType = supplier.BusinessType,
                LogoUrl = supplier.LogoUrl,
                City = supplier.City,
                District = supplier.District,
                Phone = supplier.Phone,
                Latitude = supplier.Latitude,
                Longitude = supplier.Longitude,
                IsVerified = supplier.IsVerified,
                RatingAverage = supplier.RatingAverage,
                RatingCount = supplier.RatingCount,
                TotalOrders = supplier.TotalOrders,
                CompletedOrders = supplier.CompletedOrders,
                PartsCount = partsCount,
                CreatedAt = supplier.CreatedAt
            }
        };
    }

    /// <summary>
    /// جلب قائمة الموردين
    /// </summary>
    public async Task<SuppliersListResponse> GetSuppliersListAsync(string? city = null, int page = 1, int pageSize = 20)
    {
        var query = _context.SupplierProfiles
            .Where(s => s.IsVerified && s.Status == "active" && s.DeletedAt == null);

        // فلترة حسب المدينة
        if (!string.IsNullOrEmpty(city))
        {
            query = query.Where(s => s.City == city);
        }

        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        var suppliers = await query
            .OrderByDescending(s => s.RatingAverage)
            .ThenByDescending(s => s.CompletedOrders)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // جلب عدد القطع لكل مورد
        var supplierIds = suppliers.Select(s => s.Id).ToList();
        var partsCounts = await _context.Parts
            .Where(p => supplierIds.Contains(p.SupplierId) && p.DeletedAt == null && p.Status == "available")
            .GroupBy(p => p.SupplierId)
            .Select(g => new { SupplierId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.SupplierId, x => x.Count);

        var suppliersDto = suppliers.Select(s => new SupplierListDto
        {
            Id = s.Id,
            BusinessNameAr = s.BusinessNameAr,
            BusinessNameEn = s.BusinessNameEn,
            LogoUrl = s.LogoUrl,
            City = s.City,
            District = s.District,
            IsVerified = s.IsVerified,
            RatingAverage = s.RatingAverage,
            RatingCount = s.RatingCount,
            PartsCount = partsCounts.GetValueOrDefault(s.Id, 0)
        }).ToList();

        return new SuppliersListResponse
        {
            Success = true,
            Suppliers = suppliersDto,
            Pagination = new PaginationInfo
            {
                CurrentPage = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = totalPages,
                HasNext = page < totalPages,
                HasPrevious = page > 1
            }
        };
    }

    /// <summary>
    /// جلب الموردين القريبين
    /// </summary>
    public async Task<SuppliersNearbyResponse> GetNearbySuppliersAsync(decimal latitude, decimal longitude, double radiusKm = 10)
    {
        // جلب كل الموردين اللي عندهم إحداثيات
        var suppliers = await _context.SupplierProfiles
            .Where(s => s.IsVerified
                && s.Status == "active"
                && s.DeletedAt == null
                && s.Latitude != null
                && s.Longitude != null)
            .ToListAsync();

        // جلب عدد القطع لكل مورد
        var supplierIds = suppliers.Select(s => s.Id).ToList();
        var partsCounts = await _context.Parts
            .Where(p => supplierIds.Contains(p.SupplierId) && p.DeletedAt == null && p.Status == "available")
            .GroupBy(p => p.SupplierId)
            .Select(g => new { SupplierId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.SupplierId, x => x.Count);

        // حساب المسافة وفلترة حسب النطاق
        var nearbySuppliers = suppliers
            .Select(s => new
            {
                Supplier = s,
                Distance = CalculateDistance(latitude, longitude, s.Latitude!.Value, s.Longitude!.Value)
            })
            .Where(x => x.Distance <= radiusKm)
            .OrderBy(x => x.Distance)
            .Select(x => new SupplierNearbyDto
            {
                Id = x.Supplier.Id,
                BusinessNameAr = x.Supplier.BusinessNameAr,
                BusinessNameEn = x.Supplier.BusinessNameEn,
                LogoUrl = x.Supplier.LogoUrl,
                City = x.Supplier.City,
                District = x.Supplier.District,
                Latitude = x.Supplier.Latitude,
                Longitude = x.Supplier.Longitude,
                Distance = Math.Round(x.Distance, 2),
                IsVerified = x.Supplier.IsVerified,
                RatingAverage = x.Supplier.RatingAverage,
                PartsCount = partsCounts.GetValueOrDefault(x.Supplier.Id, 0)
            })
            .ToList();

        return new SuppliersNearbyResponse
        {
            Success = true,
            Suppliers = nearbySuppliers
        };
    }

    #region Helper Methods

    /// <summary>
    /// حساب المسافة بين نقطتين بالكيلومتر (Haversine Formula)
    /// </summary>
    private static double CalculateDistance(decimal lat1, decimal lng1, decimal lat2, decimal lng2)
    {
        const double R = 6371; // نصف قطر الأرض بالكيلومتر

        var dLat = ToRad((double)(lat2 - lat1));
        var dLng = ToRad((double)(lng2 - lng1));

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRad((double)lat1)) * Math.Cos(ToRad((double)lat2)) *
                Math.Sin(dLng / 2) * Math.Sin(dLng / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return R * c;
    }

    private static double ToRad(double deg) => deg * (Math.PI / 180);

    #endregion
}