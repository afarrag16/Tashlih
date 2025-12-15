using System.Threading.Tasks;
using Tashlih.Application.DTOs.SupplierProfile;

namespace Tashlih.Application.Interfaces
{
    public interface ISupplierProfileService
    {
        // الملف الشخصي
        Task<SupplierProfileResponse> GetMyProfileAsync(long supplierId);
        Task<SupplierProfileResponse> GetProfileByIdAsync(long supplierId);
        Task<SupplierProfileResponse> UpdateProfileAsync(long supplierId, UpdateSupplierProfileRequest request);

        // التوثيق
        Task<VerificationResponse> UploadVerificationDocumentAsync(long supplierId, UploadVerificationDocumentRequest request);
        Task<VerificationResponse> UpdateVerificationDataAsync(long supplierId, UpdateVerificationDataRequest request);
        Task<VerificationResponse> GetVerificationStatusAsync(long supplierId);
        Task<VerificationResponse> RequestVerificationAsync(long supplierId);
        Task<VerificationResponse> VerifySupplierAsync(long adminId, VerifySupplierRequest request);

        // الإحصائيات
        Task<SupplierStatsResponse> GetSupplierStatsAsync(long supplierId);
    }
}
