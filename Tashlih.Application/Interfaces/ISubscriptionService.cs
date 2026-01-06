using Microsoft.AspNetCore.Http;
using Tashlih.Application.DTOs.Subscriptions;

namespace Tashlih.Application.Interfaces;

public interface ISubscriptionService
{
    // للمورد
    Task<SubscriptionPlansResponse> GetPlansAsync();
    Task<MySubscriptionResponse> GetMySubscriptionAsync(long supplierId);
    Task<SubscribeResponse> SubscribeAsync(long supplierId, SubscribeRequest request);

    // للأدمن
    Task<AdminPlansResponse> GetAllPlansAsync();
    Task<SubscriptionResponse> CreatePlanAsync(AdminCreatePlanRequest request, IFormFile? logo);
    Task<SubscriptionResponse> UpdatePlanAsync(long planId, AdminUpdatePlanRequest request, IFormFile? logo, long adminId);
    Task<SubscriptionResponse> DeletePlanAsync(long planId, long adminId);
    Task<AdminSubscriptionsResponse> GetAllSubscriptionsAsync();

    // للتسجيل (المورد الجديد)
    Task AssignFreePlanAsync(long supplierId);

    // للتحقق من الحدود
    Task<bool> CanAddPartAsync(long supplierId);
    Task<int> GetMaxImagesPerPartAsync(long supplierId);
}