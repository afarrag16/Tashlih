using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Tashlih.Application.DTOs.Admin;
using Tashlih.Application.DTOs.Parts;
using Tashlih.Application.Interfaces;
using Tashlih.Infrastructure.Models;

namespace Tashlih.Infrastructure.Services;

public class AdminCustomerService
{
    private readonly TashlihContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogService _logService;

    public AdminCustomerService(TashlihContext context, IConfiguration configuration, ILogService logService)
    {
        _context = context;
        _configuration = configuration;
        _logService = logService;
    }

    /// <summary>
    /// عرض كل العملاء
    /// </summary>
    public async Task<AdminCustomersResponse> GetAllCustomersAsync(AdminCustomersRequest request)
    {
        var query = _context.Users
            .Include(u => u.City)
            .Where(u => u.DeletedAt == null);

        // البحث
        if (!string.IsNullOrEmpty(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(u =>
                (u.FullName != null && u.FullName.ToLower().Contains(search)) ||
                (u.Phone != null && u.Phone.Contains(search)) ||
                (u.Email != null && u.Email.ToLower().Contains(search)));
        }

        // فلترة بالحالة
        if (!string.IsNullOrEmpty(request.Status))
        {
            query = query.Where(u => u.Status == request.Status);
        }

        // فلترة بالمدينة
        if (!string.IsNullOrEmpty(request.City))
        {
            query = query.Where(u => u.City != null && u.City.NameAr == request.City);
        }

        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize);

        var customers = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        // جلب عدد الطلبات
        var customerIds = customers.Select(c => c.Id).ToList();

        var ordersCounts = await _context.Orders
            .Where(o => customerIds.Contains(o.CustomerId))
            .GroupBy(o => o.CustomerId)
            .Select(g => new { CustomerId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CustomerId, x => x.Count);

        var customersDto = customers.Select(c => new AdminCustomerDto
        {
            Id = c.Id,
            FullName = c.FullName,
            Phone = c.Phone,
            Email = c.Email,
            City = c.City?.NameAr,
            AvatarUrl = GetFullUrl(c.AvatarUrl),
            Status = c.Status,
            IsPhoneVerified = c.IsPhoneVerified,
            TotalOrders = ordersCounts.GetValueOrDefault(c.Id, 0),
            FavoritesCount = 0,
            CreatedAt = c.CreatedAt,
            LastLoginAt = c.LastLoginAt
        }).ToList();

        return new AdminCustomersResponse
        {
            Success = true,
            Customers = customersDto,
            TotalCount = totalItems,
            Pagination = new PaginationInfo
            {
                CurrentPage = request.Page,
                PageSize = request.PageSize,
                TotalItems = totalItems,
                TotalPages = totalPages,
                HasNext = request.Page < totalPages,
                HasPrevious = request.Page > 1
            }
        };
    }

    /// <summary>
    /// تفاصيل عميل
    /// </summary>
    public async Task<AdminCustomerDetailResponse> GetCustomerByIdAsync(long customerId)
    {
        var customer = await _context.Users
            .Include(u => u.City)
            .FirstOrDefaultAsync(u => u.Id == customerId && u.DeletedAt == null);

        if (customer == null)
        {
            return new AdminCustomerDetailResponse
            {
                Success = false,
                Message = "Customer not found",
                MessageAr = "العميل غير موجود"
            };
        }

        var totalOrders = await _context.Orders.CountAsync(o => o.CustomerId == customerId);
        var completedOrders = await _context.Orders.CountAsync(o => o.CustomerId == customerId && o.Status == "completed");
        var cancelledOrders = await _context.Orders.CountAsync(o => o.CustomerId == customerId && o.Status == "cancelled");
        var reviewsCount = await _context.Reviews.CountAsync(r => r.CustomerId == customerId);

        var customerDto = new AdminCustomerDetailDto
        {
            Id = customer.Id,
            FullName = customer.FullName,
            Phone = customer.Phone,
            Email = customer.Email,
            City = customer.City?.NameAr,
            District = null,
            Address = customer.Street,
            AvatarUrl = GetFullUrl(customer.AvatarUrl),
            Status = customer.Status,
            IsPhoneVerified = customer.IsPhoneVerified,
            PreferredLanguage = customer.PreferredLanguage,
            TotalOrders = totalOrders,
            CompletedOrders = completedOrders,
            CancelledOrders = cancelledOrders,
            FavoritesCount = 0,
            ReviewsCount = reviewsCount,
            CreatedAt = customer.CreatedAt,
            LastLoginAt = customer.LastLoginAt
        };

        return new AdminCustomerDetailResponse
        {
            Success = true,
            Customer = customerDto
        };
    }

    /// <summary>
    /// تفعيل عميل
    /// </summary>
    public async Task<AdminCustomerActionResponse> ActivateCustomerAsync(long customerId, AdminCustomerActionRequest request, long adminId)
    {
        var customer = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == customerId && u.DeletedAt == null);

        if (customer == null)
        {
            return new AdminCustomerActionResponse
            {
                Success = false,
                Message = "Customer not found",
                MessageAr = "العميل غير موجود"
            };
        }

        var oldStatus = customer.Status;

        customer.Status = "active";
        customer.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // ✅ تسجيل العملية
        await _logService.LogAsync(
            userId: adminId,
            userType: "admin",
            userName: "Admin",
            action: "activate",
            actionAr: "تفعيل",
            entityType: "user",
            entityTypeAr: "عميل",
            entityId: customer.Id,
            oldValues: new { Status = oldStatus },
            newValues: new { Status = customer.Status },
            description: $"تم تفعيل حساب العميل: {customer.FullName}"
        );

        return new AdminCustomerActionResponse
        {
            Success = true,
            Message = "Customer activated successfully",
            MessageAr = "تم تفعيل العميل بنجاح"
        };
    }

    /// <summary>
    /// إيقاف عميل
    /// </summary>
    public async Task<AdminCustomerActionResponse> DeactivateCustomerAsync(long customerId, AdminCustomerActionRequest request, long adminId)
    {
        var customer = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == customerId && u.DeletedAt == null);

        if (customer == null)
        {
            return new AdminCustomerActionResponse
            {
                Success = false,
                Message = "Customer not found",
                MessageAr = "العميل غير موجود"
            };
        }

        var oldStatus = customer.Status;

        customer.Status = "inactive";
        customer.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // ✅ تسجيل العملية
        await _logService.LogAsync(
            userId: adminId,
            userType: "admin",
            userName: "Admin",
            action: "deactivate",
            actionAr: "إيقاف",
            entityType: "user",
            entityTypeAr: "عميل",
            entityId: customer.Id,
            oldValues: new { Status = oldStatus },
            newValues: new { Status = customer.Status },
            description: $"تم إيقاف حساب العميل: {customer.FullName}"
        );

        return new AdminCustomerActionResponse
        {
            Success = true,
            Message = "Customer deactivated successfully",
            MessageAr = "تم إيقاف العميل بنجاح"
        };
    }

    /// <summary>
    /// حذف عميل (Soft Delete)
    /// </summary>
    public async Task<AdminCustomerActionResponse> DeleteCustomerAsync(long customerId, long adminId)
    {
        var customer = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == customerId && u.DeletedAt == null);

        if (customer == null)
        {
            return new AdminCustomerActionResponse
            {
                Success = false,
                Message = "Customer not found",
                MessageAr = "العميل غير موجود"
            };
        }

        var oldStatus = customer.Status;

        customer.DeletedAt = DateTime.UtcNow;
        customer.Status = "deleted";

        await _context.SaveChangesAsync();

        // ✅ تسجيل العملية
        await _logService.LogAsync(
            userId: adminId,
            userType: "admin",
            userName: "Admin",
            action: "delete",
            actionAr: "حذف",
            entityType: "user",
            entityTypeAr: "عميل",
            entityId: customer.Id,
            oldValues: new { Status = oldStatus },
            newValues: new { Status = "deleted" },
            description: $"تم حذف حساب العميل: {customer.FullName}"
        );

        return new AdminCustomerActionResponse
        {
            Success = true,
            Message = "Customer deleted successfully",
            MessageAr = "تم حذف العميل بنجاح"
        };
    }

    #region Helper Methods

    private string? GetFullUrl(string? path)
    {
        if (string.IsNullOrEmpty(path)) return null;
        if (path.StartsWith("http")) return path;

        var baseUrl = _configuration["AppSettings:BaseUrl"] ?? "";
        return $"{baseUrl}/{path.TrimStart('/')}";
    }

    #endregion
}