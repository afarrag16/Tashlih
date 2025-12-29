namespace Tashlih.Application.DTOs.Admin;

#region Request DTOs

public class AdminLoginRequest
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}

#endregion

#region Response DTOs

public class AdminLoginResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? MessageAr { get; set; }
    public string? Token { get; set; }
    public AdminDto? Admin { get; set; }
}

public class AdminProfileResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? MessageAr { get; set; }
    public AdminDto? Admin { get; set; }
}

#endregion

#region Data DTOs

public class AdminDto
{
    public long Id { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime? CreatedAt { get; set; }
}

#endregion