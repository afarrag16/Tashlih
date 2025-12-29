using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Tashlih.Application.DTOs.Parts;
using Tashlih.Application.DTOs.Reviews;
using Tashlih.Application.DTOs.Suppliers;
using Tashlih.Application.Interfaces;
using Tashlih.Infrastructure.Models;


namespace Tashlih.Infrastructure.Services;

public class SuppliersService : ISuppliersService
{
    private readonly TashlihContext _context;
    private readonly string _baseUrl;

    public SuppliersService(TashlihContext context, IConfiguration configuration)
    {
        _context = context;
        _baseUrl = configuration["AppSettings:BaseUrl"] ?? "";
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

        // ✅ جلب القطع مع كل التفاصيل
        var parts = await _context.Parts
            .Where(p => p.SupplierId == supplierId && p.DeletedAt == null && p.Status == "available")
            .Include(p => p.PartImages)
            .Include(p => p.Category)
            .Include(p => p.VehicleType)
            .Include(p => p.VehicleSubcategory)
            .Include(p => p.Make)
            .Include(p => p.Model)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        var partsDto = parts.Select(p => new PartDto
        {
            Id = p.Id,
            SupplierId = p.SupplierId,
            SupplierName = supplier.BusinessNameAr,
            City = supplier.City,
            NameAr = p.NameAr,
            NameEn = p.NameEn,
            Description = p.Description,
            PartNumber = p.PartNumber,
            OemNumber = p.OemNumber,
            VinNumber = p.VinNumber,
            Condition = p.Condition,
            ConditionDetails = p.ConditionDetails,
            WarrantyType = p.WarrantyType,
            WarrantyDays = p.WarrantyDays,
            Price = p.Price,
            OriginalPrice = p.OriginalPrice,
            Currency = p.Currency,
            Quantity = p.Quantity,
            Status = p.Status,
            IsAvailable = p.Status == "available" && p.Quantity > 0,
            CategoryId = p.CategoryId,
            CategoryNameAr = p.Category?.NameAr,
            CustomCategory = p.CustomCategory,
            VehicleTypeId = p.VehicleTypeId,
            VehicleTypeNameAr = p.VehicleType?.NameAr,
            CustomVehicleType = p.CustomVehicleType,
            VehicleSubcategoryId = p.VehicleSubcategoryId,
            SubcategoryNameAr = p.VehicleSubcategory?.NameAr,
            CustomSubcategory = p.CustomSubcategory,
            MakeId = p.MakeId,
            MakeNameAr = p.Make?.NameAr,
            MakeLogoUrl = p.Make?.LogoUrl,
            CustomMake = p.CustomMake,
            ModelId = p.ModelId,
            ModelNameAr = p.Model?.NameAr,
            CustomModel = p.CustomModel,
            YearFrom = p.YearFrom,
            YearTo = p.YearTo,
            DeliveryAvailable = p.DeliveryAvailable,
            DeliveryByShop = p.DeliveryByShop,
            DeliveryNotes = p.DeliveryNotes,
            ViewsCount = p.ViewsCount,
            SalesCount = p.SalesCount,
            FavoritesCount = p.FavoritesCount,
            PrimaryImageUrl = p.PartImages.FirstOrDefault(i => i.IsPrimary)?.ImageUrl
                              ?? p.PartImages.FirstOrDefault()?.ImageUrl,
            Images = p.PartImages.Select(i => new PartImageDto
            {
                Id = i.Id,
                ImageUrl = i.ImageUrl,
                ThumbnailUrl = i.ThumbnailUrl,
                IsPrimary = i.IsPrimary,
                DisplayOrder = i.DisplayOrder
            }).ToList(),
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt
        }).ToList();

        // ✅ جلب التقييمات
        var reviews = await _context.Reviews
            .Where(r => r.SupplierId == supplierId && r.IsVisible)
            .Include(r => r.Customer)
            .OrderByDescending(r => r.CreatedAt)
            .Take(5)
            .ToListAsync();

        var reviewsDto = reviews.Select(r => new ReviewDto
        {
            Id = r.Id,
            OrderId = r.OrderId,
            CustomerId = r.CustomerId,
            CustomerName = r.Customer?.FullName,
            CustomerAvatar = r.Customer?.AvatarUrl,
            Rating = r.OverallRating,
            Comment = r.Comment,
            CreatedAt = r.CreatedAt
        }).ToList();

        // حساب توزيع التقييمات
        var allReviews = await _context.Reviews
            .Where(r => r.SupplierId == supplierId && r.IsVisible)
            .ToListAsync();

        var ratingBreakdown = new RatingBreakdownDto
        {
            Five = allReviews.Count(r => r.OverallRating == 5),
            Four = allReviews.Count(r => r.OverallRating == 4),
            Three = allReviews.Count(r => r.OverallRating == 3),
            Two = allReviews.Count(r => r.OverallRating == 2),
            One = allReviews.Count(r => r.OverallRating == 1)
        };
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
                PartsCount = parts.Count,
                CreatedAt = supplier.CreatedAt,
                Parts = partsDto,
                RatingBreakdown = ratingBreakdown,
                Reviews = reviewsDto
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
            Pagination = new Tashlih.Application.DTOs.Suppliers.PaginationInfo
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

    /// <summary>
    /// جلب إحصائيات المورد
    /// </summary>
    public async Task<SupplierStatisticsResponse> GetSupplierStatisticsAsync(
        long supplierId,
        string? period = null,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        // تحديد الفترة الزمنية
        DateTime? startDate = null;
        DateTime? endDate = null;

        if (fromDate.HasValue)
        {
            // فترة محددة من-إلى
            startDate = fromDate.Value.Date;
            endDate = toDate?.Date.AddDays(1) ?? DateTime.UtcNow;
        }
        else if (!string.IsNullOrEmpty(period))
        {
            endDate = DateTime.UtcNow;
            startDate = period.ToLower() switch
            {
                "today" => DateTime.UtcNow.Date,
                "week" => DateTime.UtcNow.Date.AddDays(-7),
                "month" => DateTime.UtcNow.Date.AddMonths(-1),
                _ => null
            };
        }

        // === إحصائيات الطلبات ===
        var ordersQuery = _context.Orders.Where(o => o.SupplierId == supplierId);

        if (startDate.HasValue)
            ordersQuery = ordersQuery.Where(o => o.CreatedAt >= startDate);

        if (endDate.HasValue)
            ordersQuery = ordersQuery.Where(o => o.CreatedAt <= endDate);

        var ordersStats = new OrdersStatisticsDto
        {
            New = await ordersQuery.CountAsync(o => o.Status == "pending"),
            Completed = await ordersQuery.CountAsync(o => o.Status == "received"),
            Cancelled = await ordersQuery.CountAsync(o => o.Status == "cancelled" || o.Status == "rejected")
        };

        // === القطع الأكثر مبيعاً ===
        var topSellingParts = await _context.Parts
            .Where(p => p.SupplierId == supplierId && p.DeletedAt == null && p.SalesCount > 0)
            .Include(p => p.PartImages.Where(i => i.IsPrimary))
            .OrderByDescending(p => p.SalesCount)
            .Take(10)
            .Select(p => new TopSellingPartDto
            {
                Id = p.Id,
                Name = p.NameAr,
                Image = p.PartImages.FirstOrDefault() != null
                    ? _baseUrl + p.PartImages.FirstOrDefault()!.ImageUrl
                    : null,
                Price = p.Price,
                Currency = p.Currency ?? "SAR",
                SalesCount = p.SalesCount
            })
            .ToListAsync();

        return new SupplierStatisticsResponse
        {
            Success = true,
            Statistics = new SupplierStatisticsDto
            {
                Orders = ordersStats,
                TopSellingParts = topSellingParts
            }
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