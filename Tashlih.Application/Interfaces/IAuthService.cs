using System.Threading.Tasks;
using Tashlih.Application.DTOs.Auth;

namespace Tashlih.Application.Interfaces
{
    public interface IAuthService
    {
        // التسجيل
        Task<AuthResponse> RegisterCustomerAsync(CustomerRegisterRequest request);
        Task<AuthResponse> RegisterSupplierAsync(SupplierRegisterRequest request);

        // تسجيل الدخول
        Task<AuthResponse> LoginAsync(LoginRequest request);

        // OTP
        Task<AuthResponse> SendOtpAsync(SendOtpRequest request);
        Task<AuthResponse> VerifyOtpAsync(VerifyOtpRequest request);
        Task<AuthResponse> LoginWithOtpAsync(OtpLoginRequest request);

        // كلمة المرور
        Task<AuthResponse> ChangePasswordAsync(long userId, string userType, ChangePasswordRequest request);
        Task<AuthResponse> ResetPasswordAsync(ResetPasswordRequest request);

        // تسجيل الخروج
        Task<bool> LogoutAsync(long userId, string userType, string token);
        Task<bool> LogoutAllAsync(long userId, string userType);

        // التحقق
        Task<bool> IsPhoneExistsAsync(string phone);
        Task<bool> IsEmailExistsAsync(string email);
    }
}