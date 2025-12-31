using Microsoft.EntityFrameworkCore;
using Tashlih.Application.DTOs.Suppliers;
using Tashlih.Infrastructure.Models;

namespace Tashlih.Infrastructure.Services;

public class SupplierDashboardService
{
    private readonly TashlihContext _context;

    public SupplierDashboardService(TashlihContext context)
    {
        _context = context;
    }

    /// <summary>
    /// إحصائيات داشبورد المورد
    /// </summary>
    public async Task<SupplierDashboardResponse> GetDashboardAsync(long supplierId, string period = "week")
    {
        // 1️⃣ إحصائيات الطلبات
        var ordersStats = new SupplierOrdersStats
        {
            New = await _context.Orders.CountAsync(o => o.SupplierId == supplierId && o.Status == "pending"),
            Pending = await _context.Orders.CountAsync(o => o.SupplierId == supplierId && o.Status == "confirmed"),
            Completed = await _context.Orders.CountAsync(o => o.SupplierId == supplierId && o.Status == "completed"),
            Cancelled = await _context.Orders.CountAsync(o => o.SupplierId == supplierId && o.Status == "cancelled"),
            Total = await _context.Orders.CountAsync(o => o.SupplierId == supplierId)
        };

        // 2️⃣ نشاط الطلبات (رسم بياني)
        var days = period == "month" ? 30 : 7;
        var startDate = DateTime.UtcNow.Date.AddDays(-days + 1);

        var ordersPerDay = await _context.Orders
            .Where(o => o.SupplierId == supplierId && o.CreatedAt != null && o.CreatedAt >= startDate)
            .GroupBy(o => o.CreatedAt!.Value.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Date, x => x.Count);

        var arabicDays = new[] { "الأحد", "الاثنين", "الثلاثاء", "الأربعاء", "الخميس", "الجمعة", "السبت" };

        var ordersChart = Enumerable.Range(0, days)
            .Select(i =>
            {
                var date = startDate.AddDays(i);
                return new OrdersChartItem
                {
                    Date = date.ToString("yyyy-MM-dd"),
                    Day = arabicDays[(int)date.DayOfWeek],
                    Count = ordersPerDay.GetValueOrDefault(date, 0)
                };
            })
            .ToList();

        // 3️⃣ القطع الأكثر طلباً (أعلى 6)
        var topParts = await _context.OrderItems
            .Where(oi => oi.Order.SupplierId == supplierId)
            .GroupBy(oi => new { oi.PartId, oi.PartNameSnapshot })
            .Select(g => new TopPartItem
            {
                PartId = g.Key.PartId ?? 0,
                Name = g.Key.PartNameSnapshot,
                OrdersCount = g.Count()
            })
            .OrderByDescending(x => x.OrdersCount)
            .Take(6)
            .ToListAsync();

        // 4️⃣ أداء الاستجابة
        var totalOrders = await _context.Orders
            .CountAsync(o => o.SupplierId == supplierId);

        var respondedOrders = await _context.Orders
            .CountAsync(o => o.SupplierId == supplierId && o.Status != "pending");

        var responseRate = totalOrders > 0
            ? Math.Round((respondedOrders * 100.0 / totalOrders), 0)
            : 0;

        var performance = new PerformanceStats
        {
            ResponseRate = responseRate,
            RespondedOrders = respondedOrders,
            TotalOrders = totalOrders
        };

        return new SupplierDashboardResponse
        {
            Success = true,
            Orders = ordersStats,
            OrdersChart = ordersChart,
            TopParts = topParts,
            Performance = performance
        };
    }
}