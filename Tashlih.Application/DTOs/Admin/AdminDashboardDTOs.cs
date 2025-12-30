namespace Tashlih.Application.DTOs.Admin;

public class DashboardStatisticsResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? MessageAr { get; set; }
    public SuppliersStats? Suppliers { get; set; }
    public CustomersStats? Customers { get; set; }
    public PartsStats? Parts { get; set; }
    public OrdersStats? Orders { get; set; }
    public SubscriptionsStats? Subscriptions { get; set; }
    public ThisMonthStats? ThisMonth { get; set; }
    public RecentActivities? Recent { get; set; }
}

public class SuppliersStats
{
    public int Total { get; set; }
    public int Active { get; set; }
    public int Inactive { get; set; }
    public int PendingVerification { get; set; }
}

public class CustomersStats
{
    public int Total { get; set; }
    public int Active { get; set; }
    public int Inactive { get; set; }
}

public class PartsStats
{
    public int Total { get; set; }
    public int Available { get; set; }
    public int Sold { get; set; }
}

public class OrdersStats
{
    public int Total { get; set; }
    public int Completed { get; set; }
    public int Pending { get; set; }
    public int Cancelled { get; set; }
}

public class SubscriptionsStats
{
    public int Active { get; set; }
    public int Expired { get; set; }
    public int Pending { get; set; }
    public decimal TotalRevenue { get; set; }
}

public class ThisMonthStats
{
    public int NewSuppliers { get; set; }
    public int NewCustomers { get; set; }
    public int NewOrders { get; set; }
    public int NewParts { get; set; }
    public decimal Revenue { get; set; }
}

public class RecentActivities
{
    public List<RecentSupplierDto>? Suppliers { get; set; }
    public List<RecentCustomerDto>? Customers { get; set; }
    public List<RecentOrderDto>? Orders { get; set; }
}

public class RecentSupplierDto
{
    public long Id { get; set; }
    public string? BusinessNameAr { get; set; }
    public string? Phone { get; set; }
    public string? City { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class RecentCustomerDto
{
    public long Id { get; set; }
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class RecentOrderDto
{
    public long Id { get; set; }
    public string? CustomerName { get; set; }
    public string? SupplierName { get; set; }
    public string? Status { get; set; }
    public decimal? TotalAmount { get; set; }
    public DateTime? CreatedAt { get; set; }
}
