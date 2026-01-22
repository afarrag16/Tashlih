using Tashlih.Application.DTOs;

namespace Tashlih.Application.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentResponse> CreatePaymentAsync(long userId, string userType, CreatePaymentRequest request);
        Task<PaymentStatusResponse> VerifyPaymentAsync(string invoiceId);
        Task<bool> ProcessCallbackAsync(string paymentId, string status);
      
        Task<PaymentStatusResponse> VerifyPaymentByExternalPaymentIdAsync(string paymentIdExternal);
        Task<bool> ProcessWebhookAsync(MyFatoorahWebhookRequest request);
        Task<PaymentHistoryResponse> GetPaymentHistoryAsync(long userId, string userType, int page = 1, int pageSize = 20);
        // للأدمن
        Task<AdminPaymentHistoryResponse> GetAllPaymentsAsync(int page = 1, int pageSize = 20, string? status = null);
        Task<AdminPaymentHistoryResponse> GetSupplierPaymentsAsync(long supplierId, int page = 1, int pageSize = 20);
    }
}
