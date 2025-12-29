using Tashlih.Application.DTOs.Admin;

namespace Tashlih.Application.Interfaces;

public interface IAdminAuthService
{
    Task<AdminLoginResponse> LoginAsync(AdminLoginRequest request);
    Task<AdminProfileResponse> GetProfileAsync(long adminId);
}
