namespace Tashlih.Application.DTOs.Admin;

public class LogsRequest
{
    public string? Action { get; set; }
    public string? EntityType { get; set; }
    public long? UserId { get; set; }
    public string? UserType { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class LogsResponse
{
    public bool Success { get; set; }
    public List<LogDto> Logs { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class LogDetailResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? MessageAr { get; set; }
    public LogDto? Log { get; set; }
}

public class LogDto
{
    public long Id { get; set; }
    public long? UserId { get; set; }
    public string? UserType { get; set; }
    public string? UserName { get; set; }
    public string? Action { get; set; }
    public string? ActionAr { get; set; }
    public string? EntityType { get; set; }
    public string? EntityTypeAr { get; set; }
    public long? EntityId { get; set; }
    public string? Description { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime? CreatedAt { get; set; }
}