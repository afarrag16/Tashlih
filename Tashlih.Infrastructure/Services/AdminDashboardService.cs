using Microsoft.EntityFrameworkCore;
using Tashlih.Application.DTOs.Admin;
using Tashlih.Infrastructure.Models;

namespace Tashlih.Infrastructure.Services;

public class AdminDashboardService
{
    private readonly TashlihContext _context;

    public AdminDashboardService(TashlihContext context)
    {
        _context = context;
    }

    /// <summary>
    /// إحصائيات الداشبورد
    /// </summary>
    public async Task<DashboardStatisticsResponse> GetStatisticsAsync()
    {
        var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

        // إحصائيات الموردين
        var suppliersStats = new SuppliersStats
        {
            Total = await _context.SupplierProfiles.CountAsync(s => s.DeletedAt == null),
            Active = await _context.SupplierProfiles.CountAsync(s => s.DeletedAt == null && s.Status == "active"),
            Inactive = await _context.SupplierProfiles.CountAsync(s => s.DeletedAt == null && s.Status == "inactive"),
            PendingVerification = await _context.SupplierProfiles.CountAsync(s => s.DeletedAt == null && s.VerificationStatus == "pending")
        };

        // إحصائيات العملاء
        var customersStats = new CustomersStats
        {
            Total = await _context.Users.CountAsync(u => u.DeletedAt == null),
            Active = await _context.Users.CountAsync(u => u.DeletedAt == null && u.Status == "active"),
            Inactive = await _context.Users.CountAsync(u => u.DeletedAt == null && u.Status == "inactive")
        };

        // إحصائيات القطع
        var partsStats = new PartsStats
        {
            Total = await _context.Parts.CountAsync(p => p.DeletedAt == null),
            Available = await _context.Parts.CountAsync(p => p.DeletedAt == null && p.Status == "available"),
            Sold = await _context.Parts.CountAsync(p => p.DeletedAt == null && p.Status == "sold")
        };

        // إحصائيات الطلبات
        var ordersStats = new OrdersStats
        {
            Total = await _context.Orders.CountAsync(),
            Completed = await _context.Orders.CountAsync(o => o.Status == "completed"),
            Pending = await _context.Orders.CountAsync(o => o.Status == "pending"),
            Cancelled = await _context.Orders.CountAsync(o => o.Status == "cancelled")
        };

        // إحصائيات الاشتراكات
        var subscriptionsStats = new SubscriptionsStats
        {
            Active = await _context.Subscriptions.CountAsync(s => s.Status == "active"),
            Expired = await _context.Subscriptions.CountAsync(s => s.Status == "expired"),
            Pending = await _context.Subscriptions.CountAsync(s => s.Status == "pending"),
            TotalRevenue = await _context.Subscriptions
                .Where(s => s.AmountPaid.HasValue)
                .SumAsync(s => s.AmountPaid!.Value)
        };

        // إحصائيات هذا الشهر
        var thisMonthStats = new ThisMonthStats
        {
            NewSuppliers = await _context.SupplierProfiles.CountAsync(s => s.CreatedAt >= startOfMonth),
            NewCustomers = await _context.Users.CountAsync(u => u.CreatedAt >= startOfMonth),
            NewOrders = await _context.Orders.CountAsync(o => o.CreatedAt >= startOfMonth),
            NewParts = await _context.Parts.CountAsync(p => p.CreatedAt >= startOfMonth),
            Revenue = await _context.Subscriptions
                .Where(s => s.CreatedAt >= startOfMonth && s.AmountPaid.HasValue)
                .SumAsync(s => s.AmountPaid!.Value)
        };

        // أحدث النشاطات
        var recentActivities = new RecentActivities
        {
            Suppliers = await _context.SupplierProfiles
                .Where(s => s.DeletedAt == null)
                .OrderByDescending(s => s.CreatedAt)
                .Take(5)
                .Select(s => new RecentSupplierDto
                {
                    Id = s.Id,
                    BusinessNameAr = s.BusinessNameAr,
                    Phone = s.Phone,
                    City = s.City,
                    CreatedAt = s.CreatedAt
                })
                .ToListAsync(),

            Customers = await _context.Users
                .Where(u => u.DeletedAt == null)
                .OrderByDescending(u => u.CreatedAt)
                .Take(5)
                .Select(u => new RecentCustomerDto
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Phone = u.Phone,
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync(),

            Orders = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Supplier)
                .OrderByDescending(o => o.CreatedAt)
                .Take(5)
                .Select(o => new RecentOrderDto
                {
                    Id = o.Id,
                    CustomerName = o.Customer.FullName,
                    SupplierName = o.Supplier.BusinessNameAr,
                    Status = o.Status,
                    TotalAmount = o.TotalAmount,
                    CreatedAt = o.CreatedAt
                })
                .ToListAsync()
        };

        return new DashboardStatisticsResponse
        {
            Success = true,
            Suppliers = suppliersStats,
            Customers = customersStats,
            Parts = partsStats,
            Orders = ordersStats,
            Subscriptions = subscriptionsStats,
            ThisMonth = thisMonthStats,
            Recent = recentActivities
        };
    }
}