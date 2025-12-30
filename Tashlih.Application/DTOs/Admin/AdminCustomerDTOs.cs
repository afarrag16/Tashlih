using Tashlih.Application.DTOs.Parts;

namespace Tashlih.Application.DTOs.Admin;

#region Response DTOs

public class AdminCustomersResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? MessageAr { get; set; }
    public List<AdminCustomerDto>? Customers { get; set; }
    public int TotalCount { get; set; }
    public PaginationInfo? Pagination { get; set; }
}

public class AdminCustomerDetailResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? MessageAr { get; set; }
    public AdminCustomerDetailDto? Customer { get; set; }
}

public class AdminCustomerActionResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? MessageAr { get; set; }
}

#endregion

#region Data DTOs

public class AdminCustomerDto
{
    public long Id { get; set; }
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? City { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Status { get; set; }
    public bool IsPhoneVerified { get; set; }
    public int TotalOrders { get; set; }
    public int FavoritesCount { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}

public class AdminCustomerDetailDto : AdminCustomerDto
{
    public string? District { get; set; }
    public string? Address { get; set; }
    public string? PreferredLanguage { get; set; }
    public int CompletedOrders { get; set; }
    public int CancelledOrders { get; set; }
    public int ReviewsCount { get; set; }
}

#endregion

#region Request DTOs

public class AdminCustomersRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
    public string? Status { get; set; }
    public string? City { get; set; }
}

public class AdminCustomerActionRequest
{
    public string? Reason { get; set; }
    public string? AdminNotes { get; set; }
}

#endregion