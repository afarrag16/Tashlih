using Tashlih.Core.Entities;

namespace Tashlih.Application.Interfaces
{
    public interface IJwtService
    {
        /// <summary>
        /// توليد Access Token للعميل
        /// </summary>
        string GenerateAccessToken(User user);

        /// <summary>
        /// توليد Access Token للمورد
        /// </summary>
        string GenerateSupplierAccessToken(SupplierProfile supplier);

        /// <summary>
        /// توليد Refresh Token
        /// </summary>
        string GenerateRefreshToken();

        /// <summary>
        /// التحقق من صلاحية الـ Token
        /// </summary>
        bool ValidateToken(string token);

        /// <summary>
        /// استخراج الـ User ID من الـ Token
        /// </summary>
        long? GetUserIdFromToken(string token);

        /// <summary>
        /// استخراج نوع المستخدم من الـ Token
        /// </summary>
        string? GetUserTypeFromToken(string token);
    }
}