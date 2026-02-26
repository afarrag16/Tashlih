using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using Tashlih.Application.DTOs.Subscriptions;
using Tashlih.Application.Interfaces;
using Tashlih.Core.Entities;
using Tashlih.Infrastructure.Models;

namespace Tashlih.Infrastructure.Services;

public class SubscriptionService : ISubscriptionService
{
    private readonly TashlihContext _context;
    private readonly string _baseUrl;
    private readonly string _uploadPath;
    private readonly ILogService _logService;

    public SubscriptionService(TashlihContext context, IConfiguration configuration, ILogService logService)
    {
        _context = context;
        _baseUrl = configuration["AppSettings:BaseUrl"] ?? "";
        _uploadPath = configuration["AppSettings:UploadPath"] ?? "wwwroot/uploads";
        _logService = logService;
    }

    #region للمورد

    /// <summary>
    /// جلب الباقات المتاحة
    /// </summary>
    public async Task<SubscriptionPlansResponse> GetPlansAsync()
    {
        var plans = await _context.SubscriptionPlans
            .Where(p => p.IsActive)
            .OrderBy(p => p.SortOrder)
            .ToListAsync();

        var plansDto = plans.Select(MapToPlanDto).ToList();

        return new SubscriptionPlansResponse
        {
            Success = true,
            Plans = plansDto
        };
    }

    /// <summary>
    /// جلب اشتراك المورد الحالي
    /// </summary>
    public async Task<MySubscriptionResponse> GetMySubscriptionAsync(long supplierId)
    {
        var subscription = await _context.Subscriptions
            .Include(s => s.Plan)
            .Where(s => s.SupplierId == supplierId && s.Status == "active")
            .FirstOrDefaultAsync();

        if (subscription == null)
        {
            return new MySubscriptionResponse
            {
                Success = false,
                Message = "No active subscription",
                MessageAr = "لا يوجد اشتراك فعال"
            };
        }

        // حساب الاستخدام
        var currentParts = await _context.Parts
            .CountAsync(p => p.SupplierId == supplierId && p.DeletedAt == null);

        var maxParts = subscription.Plan.MaxParts;
        var partsRemaining = maxParts.HasValue ? maxParts.Value - currentParts : int.MaxValue;

        return new MySubscriptionResponse
        {
            Success = true,
            Subscription = MapToSubscriptionDto(subscription),
            Plan = MapToPlanDto(subscription.Plan),
            Usage = new SubscriptionUsageDto
            {
                CurrentParts = currentParts,
                MaxParts = maxParts,
                PartsRemaining = partsRemaining > 0 ? partsRemaining : 0,
                MaxImagesPerPart = subscription.Plan.MaxImagesPerPart,
                CanAddPart = !maxParts.HasValue || currentParts < maxParts,
                UpgradeMessage = partsRemaining <= 0 ? "Upgrade your plan to add more parts" : null,
                UpgradeMessageAr = partsRemaining <= 0 ? "قم بترقية باقتك لإضافة المزيد من القطع" : null
            }
        };
    }

    /// <summary>
    /// اشتراك في باقة
    /// </summary>
    public async Task<SubscribeResponse> SubscribeAsync(long supplierId, SubscribeRequest request)
    {
        var plan = await _context.SubscriptionPlans.FindAsync(request.PlanId);
        if (plan == null || !plan.IsActive)
        {
            return new SubscribeResponse
            {
                Success = false,
                Message = "Plan not found",
                MessageAr = "الباقة غير موجودة"
            };
        }

        // إلغاء الاشتراك الحالي لو موجود
        var currentSubscription = await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.SupplierId == supplierId && s.Status == "active");

        if (currentSubscription != null)
        {
            currentSubscription.Status = "cancelled";
            currentSubscription.CancelledAt = DateTime.UtcNow;
            currentSubscription.CancellationReason = "Upgraded to new plan";

            // ✅ تسجيل إلغاء الاشتراك القديم
            LogSubscriptionHistory(
                subscriptionId: currentSubscription.Id,
                supplierId: supplierId,
                action: "cancelled",
                oldStatus: "active",
                newStatus: "cancelled",
                oldPlanId: currentSubscription.PlanId,
                newPlanId: null,
                amount: null,
                notes: "Upgraded to new plan"
            );
        }

        // إنشاء اشتراك جديد
        var subscription = new Subscription
        {
            SupplierId = supplierId,
            PlanId = request.PlanId,
            Status = plan.Price == 0 ? "active" : "pending",
            StartsAt = DateTime.UtcNow,
            EndsAt = DateTime.UtcNow.AddDays(plan.DurationDays),
            AmountPaid = plan.Price,
            PaymentMethod = request.PaymentMethod,
            AutoRenew = request.AutoRenew,
            CreatedAt = DateTime.UtcNow
        };

        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync();

        // ✅ تسجيل الاشتراك الجديد
        string action = currentSubscription != null ? "upgraded" : "created";
        LogSubscriptionHistory(
            subscriptionId: subscription.Id,
            supplierId: supplierId,
            action: action,
            oldStatus: null,
            newStatus: subscription.Status,
            oldPlanId: currentSubscription?.PlanId,
            newPlanId: subscription.PlanId,
            amount: plan.Price,
            notes: null
        );
        await _context.SaveChangesAsync();

        // لو الباقة مجانية، تتفعل مباشرة
        if (plan.Price == 0)
        {
            return new SubscribeResponse
            {
                Success = true,
                Message = "Subscription activated",
                MessageAr = "تم تفعيل الاشتراك",
                Subscription = MapToSubscriptionDto(subscription)
            };
        }

        // لو مدفوعة، نرجع رابط الدفع (هنضيفه لاحقاً)
        return new SubscribeResponse
        {
            Success = true,
            Message = "Proceed to payment",
            MessageAr = "تابع عملية الدفع",
            Subscription = MapToSubscriptionDto(subscription),
            PaymentUrl = $"/api/Subscriptions/pay/{subscription.Id}"
        };
    }

    #endregion

    #region للأدمن

    /// <summary>
    /// جلب كل الباقات (للأدمن)
    /// </summary>
    public async Task<AdminPlansResponse> GetAllPlansAsync()
    {
        var plans = await _context.SubscriptionPlans
            .OrderBy(p => p.SortOrder)
            .ToListAsync();

        return new AdminPlansResponse
        {
            Success = true,
            Plans = plans.Select(MapToPlanDto).ToList(),
            TotalCount = plans.Count
        };
    }

    /// <summary>
    /// إنشاء باقة جديدة
    /// </summary>
    public async Task<SubscriptionResponse> CreatePlanAsync(AdminCreatePlanRequest request, IFormFile? logo)
    {
        var plan = new SubscriptionPlan
        {
            NameAr = request.NameAr,
            NameEn = request.NameEn,
            DescriptionAr = request.DescriptionAr,
            DescriptionEn = request.DescriptionEn,
            Price = request.Price,
            Currency = request.Currency,
            DurationDays = request.DurationDays,
            MaxParts = request.MaxParts,
            MaxImagesPerPart = request.MaxImagesPerPart,
            MaxShops = request.MaxShops,
            Features = request.Features != null ? JsonSerializer.Serialize(request.Features) : null,
            SortOrder = request.SortOrder,
            IsPopular = request.IsPopular,
            BadgeText = request.BadgeText,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _context.SubscriptionPlans.Add(plan);
        await _context.SaveChangesAsync();

        // رفع اللوجو
        if (logo != null)
        {
            plan.LogoUrl = await SaveLogoAsync(logo, plan.Id);
            await _context.SaveChangesAsync();
        }

        return new SubscriptionResponse
        {
            Success = true,
            Message = "Plan created successfully",
            MessageAr = "تم إنشاء الباقة بنجاح"
        };
    }

    /// <summary>
    /// تعديل باقة
    /// </summary>
    public async Task<SubscriptionResponse> UpdatePlanAsync(long planId, AdminUpdatePlanRequest request, IFormFile? logo, long adminId)
    {
        var plan = await _context.SubscriptionPlans.FindAsync(planId);
        if (plan == null)
        {
            return new SubscriptionResponse
            {
                Success = false,
                Message = "Plan not found",
                MessageAr = "الباقة غير موجودة"
            };
        }

        // حفظ القيم القديمة للـ Log
        var oldValues = new
        {
            plan.NameAr,
            plan.Price,
            plan.DurationDays,
            plan.MaxParts,
            plan.IsActive
        };

        // تحديث الحقول
        if (request.NameAr != null) plan.NameAr = request.NameAr;
        if (request.NameEn != null) plan.NameEn = request.NameEn;
        if (request.DescriptionAr != null) plan.DescriptionAr = request.DescriptionAr;
        if (request.DescriptionEn != null) plan.DescriptionEn = request.DescriptionEn;
        if (request.Price.HasValue) plan.Price = request.Price.Value;
        if (request.Currency != null) plan.Currency = request.Currency;
        if (request.DurationDays.HasValue) plan.DurationDays = request.DurationDays.Value;
        if (request.MaxParts.HasValue) plan.MaxParts = request.MaxParts;
        if (request.MaxImagesPerPart.HasValue) plan.MaxImagesPerPart = request.MaxImagesPerPart.Value;
        if (request.MaxShops.HasValue) plan.MaxShops = request.MaxShops.Value;
        if (request.Features != null) plan.Features = JsonSerializer.Serialize(request.Features);
        if (request.SortOrder.HasValue) plan.SortOrder = request.SortOrder.Value;
        if (request.IsPopular.HasValue) plan.IsPopular = request.IsPopular.Value;
        if (request.BadgeText != null) plan.BadgeText = request.BadgeText;
        if (request.IsActive.HasValue) plan.IsActive = request.IsActive.Value;
        plan.UpdatedAt = DateTime.UtcNow;

        // رفع لوجو جديد
        if (logo != null)
        {
            if (!string.IsNullOrEmpty(plan.LogoUrl))
            {
                DeleteOldLogo(plan.LogoUrl);
            }
            plan.LogoUrl = await SaveLogoAsync(logo, plan.Id);
        }

        await _context.SaveChangesAsync();

        // ✅ تسجيل العملية
        await _logService.LogAsync(
            userId: adminId,
            userType: "admin",
            userName: "Admin",
            action: "update",
            actionAr: "تعديل",
            entityType: "plan",
            entityTypeAr: "باقة",
            entityId: plan.Id,
            oldValues: oldValues,
            newValues: new { plan.NameAr, plan.Price, plan.DurationDays, plan.MaxParts, plan.IsActive },
            description: $"تم تعديل الباقة: {plan.NameAr}"
        );

        return new SubscriptionResponse
        {
            Success = true,
            Message = "Plan updated successfully",
            MessageAr = "تم تعديل الباقة بنجاح"
        };
    }

    /// <summary>
    /// حذف باقة
    /// </summary>
    public async Task<SubscriptionResponse> DeletePlanAsync(long planId, long adminId)
    {
        var plan = await _context.SubscriptionPlans.FindAsync(planId);
        if (plan == null)
        {
            return new SubscriptionResponse
            {
                Success = false,
                Message = "Plan not found",
                MessageAr = "الباقة غير موجودة"
            };
        }

        // التحقق من عدم وجود اشتراكات فعالة
        var activeSubscriptions = await _context.Subscriptions
            .AnyAsync(s => s.PlanId == planId && s.Status == "active");

        if (activeSubscriptions)
        {
            return new SubscriptionResponse
            {
                Success = false,
                Message = "Cannot delete plan with active subscriptions",
                MessageAr = "لا يمكن حذف باقة لها اشتراكات فعالة"
            };
        }

        var planName = plan.NameAr;

        // حذف اللوجو
        if (!string.IsNullOrEmpty(plan.LogoUrl))
        {
            DeleteOldLogo(plan.LogoUrl);
        }

        _context.SubscriptionPlans.Remove(plan);
        await _context.SaveChangesAsync();

        // ✅ تسجيل العملية
        await _logService.LogAsync(
            userId: adminId,
            userType: "admin",
            userName: "Admin",
            action: "delete",
            actionAr: "حذف",
            entityType: "plan",
            entityTypeAr: "باقة",
            entityId: planId,
            oldValues: new { PlanName = planName },
            newValues: null,
            description: $"تم حذف الباقة: {planName}"
        );

        return new SubscriptionResponse
        {
            Success = true,
            Message = "Plan deleted successfully",
            MessageAr = "تم حذف الباقة بنجاح"
        };
    }

    /// <summary>
    /// جلب كل الاشتراكات (للأدمن)
    /// </summary>
    public async Task<AdminSubscriptionsResponse> GetAllSubscriptionsAsync()
    {
        // جيب آخر اشتراك لكل مورد
        var subscriptions = await _context.Subscriptions
            .Include(s => s.Plan)
            .Include(s => s.Supplier)
            .GroupBy(s => s.SupplierId)
            .Select(g => g.OrderByDescending(s => s.CreatedAt).First())
            .ToListAsync();

        var subscriptionsDto = subscriptions
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new AdminSubscriptionDto
            {
                Id = s.Id,
                SupplierId = s.SupplierId,
                SupplierName = s.Supplier?.BusinessNameAr,
                SupplierPhone = s.Supplier?.Phone,
                PlanName = s.Plan?.NameAr,
                Price = s.Plan?.Price ?? 0,
                Status = s.Status,
                StatusAr = GetStatusAr(s.Status),
                StartsAt = s.StartsAt,
                EndsAt = s.EndsAt,
                AmountPaid = s.AmountPaid,
                PaymentMethod = s.PaymentMethod,
                CreatedAt = s.CreatedAt
            }).ToList();

        return new AdminSubscriptionsResponse
        {
            Success = true,
            Subscriptions = subscriptionsDto,
            TotalCount = subscriptionsDto.Count
        };
    }

    #endregion

    #region للتسجيل

    /// <summary>
    /// تعيين الباقة المجانية للمورد الجديد
    /// </summary>
    public async Task AssignFreePlanAsync(long supplierId)
    {
        // البحث عن الباقة المجانية (السعر = 0)
        var freePlan = await _context.SubscriptionPlans
            .FirstOrDefaultAsync(p => p.Price == 0 && p.IsActive);

        if (freePlan == null) return;

        var subscription = new Subscription
        {
            SupplierId = supplierId,
            PlanId = freePlan.Id,
            Status = "active",
            StartsAt = DateTime.UtcNow,
            EndsAt = DateTime.UtcNow.AddDays(freePlan.DurationDays),
            AmountPaid = 0,
            AutoRenew = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync();
    }

    #endregion

    #region للتحقق من الحدود

    /// <summary>
    /// هل المورد يقدر يضيف قطعة؟
    /// </summary>
    public async Task<bool> CanAddPartAsync(long supplierId)
    {
        var subscription = await _context.Subscriptions
            .Include(s => s.Plan)
            .FirstOrDefaultAsync(s => s.SupplierId == supplierId && s.Status == "active");

        if (subscription == null) return false;

        var maxParts = subscription.Plan.MaxParts;
        if (!maxParts.HasValue) return true; // غير محدود

        var currentParts = await _context.Parts
            .CountAsync(p => p.SupplierId == supplierId && p.DeletedAt == null);

        return currentParts < maxParts;
    }

    /// <summary>
    /// جلب الحد الأقصى للصور لكل قطعة
    /// </summary>
    public async Task<int> GetMaxImagesPerPartAsync(long supplierId)
    {
        var subscription = await _context.Subscriptions
            .Include(s => s.Plan)
            .FirstOrDefaultAsync(s => s.SupplierId == supplierId && s.Status == "active");

        return subscription?.Plan.MaxImagesPerPart ?? 3; // افتراضي 3
    }

    /// <summary>
    /// تسجيل في سجل الاشتراكات
    /// </summary>
    private void LogSubscriptionHistory(
        long subscriptionId,
        long supplierId,
        string action,
        string? oldStatus,
        string? newStatus,
        long? oldPlanId,
        long? newPlanId,
        decimal? amount,
        string? notes,
        long? performedBy = null)
    {
        var history = new SubscriptionHistory
        {
            SubscriptionId = subscriptionId,
            SupplierId = supplierId,
            Action = action,
            OldStatus = oldStatus,
            NewStatus = newStatus,
            OldPlanId = oldPlanId,
            NewPlanId = newPlanId,
            Amount = amount,
            Notes = notes,
            PerformedBy = performedBy,
            CreatedAt = DateTime.UtcNow
        };

        _context.SubscriptionHistories.Add(history);
    }

    #endregion

    #region Helper Methods

    private SubscriptionPlanDto MapToPlanDto(SubscriptionPlan plan)
    {
        List<string>? features = null;
        if (!string.IsNullOrEmpty(plan.Features))
        {
            try
            {
                features = JsonSerializer.Deserialize<List<string>>(plan.Features);
            }
            catch { }
        }

        return new SubscriptionPlanDto
        {
            Id = plan.Id,
            NameAr = plan.NameAr,
            NameEn = plan.NameEn,
            DescriptionAr = plan.DescriptionAr,
            DescriptionEn = plan.DescriptionEn,
            LogoUrl = GetFullUrl(plan.LogoUrl),
            Price = plan.Price,
            Currency = plan.Currency,
            DurationDays = plan.DurationDays,
            MaxParts = plan.MaxParts,
            MaxImagesPerPart = plan.MaxImagesPerPart,
            MaxShops = plan.MaxShops,
            Features = features,
            IsPopular = plan.IsPopular,
            BadgeText = plan.BadgeText
        };
    }

    private SubscriptionDto MapToSubscriptionDto(Subscription subscription)
    {
        var daysRemaining = subscription.EndsAt.HasValue
                   ? (subscription.EndsAt.Value - DateTime.UtcNow).Days
                   :  0;

        return new SubscriptionDto
        {
            Id = subscription.Id,
            SupplierId = subscription.SupplierId,
            PlanId = subscription.PlanId,
            PlanName = subscription.Plan?.NameAr,
            Status = subscription.Status,
            StatusAr = GetStatusAr(subscription.Status),
            StartsAt = subscription.StartsAt,
            EndsAt = subscription.EndsAt,
            DaysRemaining = daysRemaining > 0 ? daysRemaining : 0,
            AmountPaid = subscription.AmountPaid,
            PaymentMethod = subscription.PaymentMethod,
            AutoRenew = subscription.AutoRenew,
            CreatedAt = subscription.CreatedAt
        };
    }

    private string GetStatusAr(string? status)
    {
        return status?.ToLower() switch
        {
            "active" => "فعال",
            "expired" => "منتهي",
            "cancelled" => "ملغي",
            "pending" => "في انتظار الدفع",
            _ => status ?? ""
        };
    }

    private string? GetFullUrl(string? path)
    {
        if (string.IsNullOrEmpty(path)) return null;
        if (path.StartsWith("http")) return path;
        return _baseUrl + path;
    }

    private async Task<string> SaveLogoAsync(IFormFile logo, long planId)
    {
        var folderPath = Path.Combine(_uploadPath, "plans", planId.ToString());
        Directory.CreateDirectory(folderPath);

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(logo.FileName)}";
        var filePath = Path.Combine(folderPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await logo.CopyToAsync(stream);
        }

        return $"/uploads/plans/{planId}/{fileName}";
    }

    private void DeleteOldLogo(string logoUrl)
    {
        try
        {
            var relativePath = logoUrl.TrimStart('/');
            var fullPath = Path.Combine(_uploadPath.Replace("/uploads", ""), relativePath);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
        catch { }
    }

    #endregion
}
