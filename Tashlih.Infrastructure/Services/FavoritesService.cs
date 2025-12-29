using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Tashlih.Application.DTOs.Favorites;
using Tashlih.Application.Interfaces;
using Tashlih.Core.Entities;
using Tashlih.Infrastructure.Models;

namespace Tashlih.Infrastructure.Services;

public class FavoritesService : IFavoritesService
{
    private readonly TashlihContext _context;
    private readonly string _baseUrl;

    public FavoritesService(TashlihContext context, IConfiguration configuration)
    {
        _context = context;
        _baseUrl = configuration["AppSettings:BaseUrl"] ?? "";
    }

    #region القطع المفضلة

    /// <summary>
    /// إضافة قطعة للمفضلة
    /// </summary>
    public async Task<FavoriteResponse> AddPartToFavoritesAsync(long customerId, long partId)
    {
        // التحقق من وجود القطعة
        var part = await _context.Parts.FindAsync(partId);
        if (part == null || part.DeletedAt != null)
        {
            return new FavoriteResponse
            {
                Success = false,
                Message = "Part not found",
                MessageAr = "القطعة غير موجودة"
            };
        }

        // التحقق من عدم وجودها في المفضلة
        var exists = await _context.FavoriteParts
            .AnyAsync(f => f.CustomerId == customerId && f.PartId == partId);

        if (exists)
        {
            return new FavoriteResponse
            {
                Success = false,
                Message = "Part already in favorites",
                MessageAr = "القطعة موجودة في المفضلة مسبقاً"
            };
        }

        // إضافة للمفضلة
        var favorite = new FavoritePart
        {
            CustomerId = customerId,
            PartId = partId,
            CreatedAt = DateTime.UtcNow
        };

        _context.FavoriteParts.Add(favorite);
        await _context.SaveChangesAsync();

        // تحديث عداد المفضلة في القطعة
        part.FavoritesCount++;
        await _context.SaveChangesAsync();

        return new FavoriteResponse
        {
            Success = true,
            Message = "Part added to favorites",
            MessageAr = "تم إضافة القطعة للمفضلة"
        };
    }

    /// <summary>
    /// إزالة قطعة من المفضلة
    /// </summary>
    public async Task<FavoriteResponse> RemovePartFromFavoritesAsync(long customerId, long partId)
    {
        var favorite = await _context.FavoriteParts
            .FirstOrDefaultAsync(f => f.CustomerId == customerId && f.PartId == partId);

        if (favorite == null)
        {
            return new FavoriteResponse
            {
                Success = false,
                Message = "Part not in favorites",
                MessageAr = "القطعة غير موجودة في المفضلة"
            };
        }

        _context.FavoriteParts.Remove(favorite);
        await _context.SaveChangesAsync();

        // تحديث عداد المفضلة في القطعة
        var part = await _context.Parts.FindAsync(partId);
        if (part != null && part.FavoritesCount > 0)
        {
            part.FavoritesCount--;
            await _context.SaveChangesAsync();
        }

        return new FavoriteResponse
        {
            Success = true,
            Message = "Part removed from favorites",
            MessageAr = "تم إزالة القطعة من المفضلة"
        };
    }

    /// <summary>
    /// جلب القطع المفضلة
    /// </summary>
    public async Task<FavoritePartsResponse> GetFavoritePartsAsync(long customerId)
    {
        var favorites = await _context.FavoriteParts
            .Where(f => f.CustomerId == customerId)
            .Include(f => f.Part)
                .ThenInclude(p => p.Supplier)
            .Include(f => f.Part)
                .ThenInclude(p => p.PartImages.Where(i => i.IsPrimary))
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();

        var partsDto = favorites
            .Where(f => f.Part != null && f.Part.DeletedAt == null)
            .Select(f => new FavoritePartDto
            {
                Id = f.Id,
                PartId = f.PartId,
                PartName = f.Part.NameAr,
                PartImage = GetFullUrl(f.Part.PartImages.FirstOrDefault()?.ImageUrl),
                Price = f.Part.Price,
                OriginalPrice = f.Part.OriginalPrice,
                Currency = f.Part.Currency ?? "SAR",
                Condition = f.Part.Condition,
                ConditionAr = GetConditionAr(f.Part.Condition),
                SupplierId = f.Part.SupplierId,
                SupplierName = f.Part.Supplier?.BusinessNameAr,
                City = f.Part.Supplier?.City,
                IsAvailable = f.Part.Status == "available" && f.Part.Quantity > 0,
                HasWarranty = f.Part.WarrantyDays > 0,
                AddedAt = f.CreatedAt
            }).ToList();

        return new FavoritePartsResponse
        {
            Success = true,
            Parts = partsDto,
            TotalCount = partsDto.Count
        };
    }

    /// <summary>
    /// التحقق إذا القطعة مفضلة
    /// </summary>
    public async Task<FavoriteCheckResponse> IsPartFavoriteAsync(long customerId, long partId)
    {
        var isFavorite = await _context.FavoriteParts
            .AnyAsync(f => f.CustomerId == customerId && f.PartId == partId);

        return new FavoriteCheckResponse
        {
            Success = true,
            IsFavorite = isFavorite,
             Message = isFavorite ? "Part is in favorites" : "Part is not in favorites",
            MessageAr = isFavorite ? "القطعة في المفضلة" : "القطعة ليست في المفضلة"
        };
    }

    #endregion

    #region الموردين المفضلين

    /// <summary>
    /// إضافة مورد للمفضلة
    /// </summary>
    public async Task<FavoriteResponse> AddSupplierToFavoritesAsync(long customerId, long supplierId)
    {
        // التحقق من وجود المورد
        var supplier = await _context.SupplierProfiles.FindAsync(supplierId);
        if (supplier == null || supplier.DeletedAt != null)
        {
            return new FavoriteResponse
            {
                Success = false,
                Message = "Supplier not found",
                MessageAr = "المورد غير موجود"
            };
        }

        // التحقق من عدم وجوده في المفضلة
        var exists = await _context.FavoriteSuppliers
            .AnyAsync(f => f.CustomerId == customerId && f.SupplierId == supplierId);

        if (exists)
        {
            return new FavoriteResponse
            {
                Success = false,
                Message = "Supplier already in favorites",
                MessageAr = "المورد موجود في المفضلة مسبقاً"
            };
        }

        // إضافة للمفضلة
        var favorite = new FavoriteSupplier
        {
            CustomerId = customerId,
            SupplierId = supplierId,
            CreatedAt = DateTime.UtcNow
        };

        _context.FavoriteSuppliers.Add(favorite);
        await _context.SaveChangesAsync();

        return new FavoriteResponse
        {
            Success = true,
            Message = "Supplier added to favorites",
            MessageAr = "تم إضافة المورد للمفضلة"
        };
    }

    /// <summary>
    /// إزالة مورد من المفضلة
    /// </summary>
    public async Task<FavoriteResponse> RemoveSupplierFromFavoritesAsync(long customerId, long supplierId)
    {
        var favorite = await _context.FavoriteSuppliers
            .FirstOrDefaultAsync(f => f.CustomerId == customerId && f.SupplierId == supplierId);

        if (favorite == null)
        {
            return new FavoriteResponse
            {
                Success = false,
                Message = "Supplier not in favorites",
                MessageAr = "المورد غير موجود في المفضلة"
            };
        }

        _context.FavoriteSuppliers.Remove(favorite);
        await _context.SaveChangesAsync();

        return new FavoriteResponse
        {
            Success = true,
            Message = "Supplier removed from favorites",
            MessageAr = "تم إزالة المورد من المفضلة"
        };
    }

    /// <summary>
    /// جلب الموردين المفضلين
    /// </summary>
    public async Task<FavoriteSuppliersResponse> GetFavoriteSuppliersAsync(long customerId)
    {
        var favorites = await _context.FavoriteSuppliers
            .Where(f => f.CustomerId == customerId)
            .Include(f => f.Supplier)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();

        // جلب عدد القطع لكل مورد
        var supplierIds = favorites.Select(f => f.SupplierId).ToList();
        var partsCounts = await _context.Parts
            .Where(p => supplierIds.Contains(p.SupplierId) && p.DeletedAt == null && p.Status == "available")
            .GroupBy(p => p.SupplierId)
            .Select(g => new { SupplierId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.SupplierId, x => x.Count);

        var suppliersDto = favorites
            .Where(f => f.Supplier != null && f.Supplier.DeletedAt == null)
            .Select(f => new FavoriteSupplierDto
            {
                Id = f.Id,
                SupplierId = f.SupplierId,
                SupplierName = f.Supplier.BusinessNameAr,
                SupplierLogo = GetFullUrl(f.Supplier.LogoUrl),
                City = f.Supplier.City,
                District = f.Supplier.District,
                Phone = f.Supplier.Phone,
                RatingAverage = f.Supplier.RatingAverage,
                RatingCount = f.Supplier.RatingCount,
                PartsCount = partsCounts.GetValueOrDefault(f.SupplierId, 0),
                IsVerified = f.Supplier.IsVerified,
                AddedAt = f.CreatedAt
            }).ToList();

        return new FavoriteSuppliersResponse
        {
            Success = true,
            Suppliers = suppliersDto,
            TotalCount = suppliersDto.Count
        };
    }

    /// <summary>
    /// التحقق إذا المورد مفضل
    /// </summary>
    public async Task<FavoriteCheckResponse> IsSupplierFavoriteAsync(long customerId, long supplierId)
    {
        var isFavorite = await _context.FavoriteSuppliers
            .AnyAsync(f => f.CustomerId == customerId && f.SupplierId == supplierId);

        return new FavoriteCheckResponse
        {
            Success = true,
            IsFavorite = isFavorite,
             Message = isFavorite ? "Supplier is in favorites" : "Supplier is not in favorites",
            MessageAr = isFavorite ? "المورد في المفضلة" : "المورد ليس في المفضلة"
        };
    }

    #endregion

    #region Helper Methods

    private string? GetFullUrl(string? path)
    {
        if (string.IsNullOrEmpty(path)) return null;
        if (path.StartsWith("http")) return path;
        return _baseUrl + path;
    }

    private string GetConditionAr(string? condition)
    {
        return condition?.ToLower() switch
        {
            "new" => "جديد",
            "used" => "مستعمل",
            "refurbished" => "مجدد",
            _ => condition ?? ""
        };
    }

    #endregion
}