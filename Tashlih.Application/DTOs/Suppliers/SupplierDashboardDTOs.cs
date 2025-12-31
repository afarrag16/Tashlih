namespace Tashlih.Application.DTOs.Suppliers;

public class SupplierDashboardResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? MessageAr { get; set; }
    public SupplierOrdersStats? Orders { get; set; }
    public List<OrdersChartItem>? OrdersChart { get; set; }
    public List<TopPartItem>? TopParts { get; set; }
    public PerformanceStats? Performance { get; set; }
}

public class SupplierOrdersStats
{
    public int New { get; set; }
    public int Pending { get; set; }
    public int Completed { get; set; }
    public int Cancelled { get; set; }
    public int Total { get; set; }
}

public class OrdersChartItem
{
    public string? Date { get; set; }
    public string? Day { get; set; }
    public int Count { get; set; }
}

public class TopPartItem
{
    public long PartId { get; set; }
    public string? Name { get; set; }
    public int OrdersCount { get; set; }
}

public class PerformanceStats
{
    public double ResponseRate { get; set; }
    public int RespondedOrders { get; set; }
    public int TotalOrders { get; set; }
}
