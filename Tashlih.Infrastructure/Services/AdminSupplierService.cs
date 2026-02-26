using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using Tashlih.Application.DTOs.Admin;
using Tashlih.Application.DTOs.Parts;
using Tashlih.Application.DTOs.SupplierProfile;
using Tashlih.Application.Interfaces;
using Tashlih.Core.Entities;
using Tashlih.Infrastructure.Models;

namespace Tashlih.Infrastructure.Services;

public class AdminSupplierService
{
    private readonly TashlihContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogService _logService;

    public AdminSupplierService(TashlihContext context, IConfiguration configuration, ILogService logService)
    {
        _context = context;
        _configuration = configuration;
        _logService = logService;
    }

    /// <summary>
    /// عرض كل الموردين
    /// </summary>
    public async Task<AdminSuppliersResponse> GetAllSuppliersAsync(AdminSuppliersRequest request)
    {
        var query = _context.SupplierProfiles
            .Where(s => s.DeletedAt == null);

        // البحث
        if (!string.IsNullOrEmpty(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(s =>
                (s.FullName != null && s.FullName.ToLower().Contains(search)) ||
                (s.Phone != null && s.Phone.Contains(search)) ||
                (s.Email != null && s.Email.ToLower().Contains(search)) ||
                (s.BusinessNameAr != null && s.BusinessNameAr.ToLower().Contains(search)) ||
                (s.BusinessNameEn != null && s.BusinessNameEn.ToLower().Contains(search)));
        }

        // فلترة بالحالة
        if (!string.IsNullOrEmpty(request.Status))
        {
            query = query.Where(s => s.Status == request.Status);
        }

        // فلترة بحالة التوثيق
        if (!string.IsNullOrEmpty(request.VerificationStatus))
        {
            query = query.Where(s => s.VerificationStatus == request.VerificationStatus);
        }

        // فلترة بالمدينة
        if (!string.IsNullOrEmpty(request.City))
        {
            query = query.Where(s => s.City == request.City);
        }

        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize);

        var suppliers = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        // جلب عدد القطع والاشتراكات
        var supplierIds = suppliers.Select(s => s.Id).ToList();

        var partsCounts = await _context.Parts
            .Where(p => supplierIds.Contains(p.SupplierId) && p.DeletedAt == null)
            .GroupBy(p => p.SupplierId)
            .Select(g => new { SupplierId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.SupplierId, x => x.Count);
        var subscriptions = await _context.Subscriptions
            .Include(s => s.Plan)
            .Where(s => supplierIds.Contains(s.SupplierId) && s.Status == "active")
            .GroupBy(s => s.SupplierId)
            .Select(g => g.OrderByDescending(s => s.CreatedAt).First())
            .ToDictionaryAsync(s => s.SupplierId);

        var suppliersDto = suppliers.Select(s => new AdminSupplierDto
        {
            Id = s.Id,
            FullName = s.FullName,
            Phone = s.Phone,
            Email = s.Email,
            BusinessNameAr = s.BusinessNameAr,
            BusinessNameEn = s.BusinessNameEn,
            City = s.City,
            LogoUrl = GetFullUrl(s.LogoUrl),
            IsVerified = s.IsVerified,
            VerificationStatus = s.VerificationStatus,
            Status = s.Status,
            RatingAverage = s.RatingAverage,
            TotalOrders = s.TotalOrders,
            PartsCount = partsCounts.GetValueOrDefault(s.Id, 0),
            CurrentPlan = subscriptions.ContainsKey(s.Id) ? subscriptions[s.Id].Plan.NameAr : null,
            CreatedAt = s.CreatedAt,
            LastLoginAt = s.LastLoginAt
        }).ToList();

        return new AdminSuppliersResponse
        {
            Success = true,
            Suppliers = suppliersDto,
            TotalCount = totalItems,
            Pagination = new PaginationInfo
            {
                CurrentPage = request.Page,
                PageSize = request.PageSize,
                TotalItems = totalItems,
                TotalPages = totalPages,
                HasNext = request.Page < totalPages,
                HasPrevious = request.Page > 1
            }
        };
    }

    /// <summary>
    /// تفاصيل مورد
    /// </summary>
    public async Task<AdminSupplierDetailResponse> GetSupplierByIdAsync(long supplierId)
    {
        var supplier = await _context.SupplierProfiles
            .FirstOrDefaultAsync(s => s.Id == supplierId && s.DeletedAt == null);

        if (supplier == null)
        {
            return new AdminSupplierDetailResponse
            {
                Success = false,
                Message = "Supplier not found",
                MessageAr = "المورد غير موجود"
            };
        }

        var partsCount = await _context.Parts
            .CountAsync(p => p.SupplierId == supplierId && p.DeletedAt == null);

        var subscription = await _context.Subscriptions
            .Include(s => s.Plan)
            .FirstOrDefaultAsync(s => s.SupplierId == supplierId && s.Status == "active");

        var supplierDto = new AdminSupplierDetailDto
        {
            Id = supplier.Id,
            FullName = supplier.FullName,
            Phone = supplier.Phone,
            Email = supplier.Email,
            BusinessNameAr = supplier.BusinessNameAr,
            BusinessNameEn = supplier.BusinessNameEn,
            City = supplier.City,
            District = supplier.District,
            LogoUrl = GetFullUrl(supplier.LogoUrl),
            IsVerified = supplier.IsVerified,
            VerificationStatus = supplier.VerificationStatus,
            Status = supplier.Status,
            RatingAverage = supplier.RatingAverage,
            RatingCount = supplier.RatingCount,
            TotalOrders = supplier.TotalOrders,
            CompletedOrders = supplier.CompletedOrders,
            PartsCount = partsCount,
            CurrentPlan = subscription?.Plan.NameAr,
            CreatedAt = supplier.CreatedAt,
            LastLoginAt = supplier.LastLoginAt,

            // بيانات إضافية
            ManagerName = supplier.ManagerName,
            Description = supplier.Description,
            BusinessType = supplier.BusinessType,

            // المستندات
            IdFrontUrl = GetFullUrl(supplier.IdFrontUrl),
            IdBackUrl = GetFullUrl(supplier.IdBackUrl),
            IdNumber = supplier.IdNumber,
            CommercialRegisterImageUrl = GetFullUrl(supplier.CommercialRegisterImageUrl),
            CommercialRegister = supplier.CommercialRegister,
            CommercialRegisterExpiryDate = supplier.CommercialRegisterExpiryDate,
            LicenseImageUrl = GetFullUrl(supplier.LicenseImageUrl),
            LicenseNumber = supplier.LicenseNumber,
            LicenseExpiryDate = supplier.LicenseExpiryDate,
            TaxCertificateUrl = GetFullUrl(supplier.TaxCertificateUrl),
            TaxNumber = supplier.TaxNumber,

            // التوثيق
            RejectionReason = supplier.RejectionReason,
            AdminNotes = supplier.AdminNotes,
            VerificationSubmittedAt = supplier.VerificationSubmittedAt,
            VerifiedAt = supplier.VerifiedAt,

            // الاشتراك
            Subscription = subscription != null ? new SupplierSubscriptionDto
            {
                Id = subscription.Id,
                PlanName = subscription.Plan.NameAr,
                Status = subscription.Status,
                StartsAt = subscription.StartsAt,
                EndsAt = subscription.EndsAt,
                DaysRemaining = subscription.EndsAt.HasValue
                                  ? (subscription.EndsAt.Value - DateTime.UtcNow).Days
                                  : 0
            } : null
        };

        return new AdminSupplierDetailResponse
        {
            Success = true,
            Supplier = supplierDto
        };
    }

    /// <summary>
    /// تفعيل مورد
    /// </summary>
    public async Task<AdminSupplierActionResponse> ActivateSupplierAsync(long supplierId, AdminSupplierActionRequest request, long adminId)
    {
        var supplier = await _context.SupplierProfiles
            .FirstOrDefaultAsync(s => s.Id == supplierId && s.DeletedAt == null);

        if (supplier == null)
        {
            return new AdminSupplierActionResponse
            {
                Success = false,
                Message = "Supplier not found",
                MessageAr = "المورد غير موجود"
            };
        }

        var oldStatus = supplier.Status;

        supplier.Status = "active";
        supplier.AdminNotes = request.AdminNotes;
        supplier.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // ✅ تسجيل العملية
        await _logService.LogAsync(
            userId: adminId,
            userType: "admin",
            userName: "Admin",
            action: "activate",
            actionAr: "تفعيل",
            entityType: "supplier_profile",
            entityTypeAr: "مورد",
            entityId: supplier.Id,
            oldValues: new { Status = oldStatus },
            newValues: new { Status = supplier.Status },
            description: $"تم تفعيل حساب المورد: {supplier.FullName}"
        );

        return new AdminSupplierActionResponse
        {
            Success = true,
            Message = "Supplier activated successfully",
            MessageAr = "تم تفعيل المورد بنجاح"
        };
    }

    /// <summary>
    /// إيقاف مورد
    /// </summary>
    public async Task<AdminSupplierActionResponse> DeactivateSupplierAsync(long supplierId, AdminSupplierActionRequest request, long adminId)
    {
        var supplier = await _context.SupplierProfiles
            .FirstOrDefaultAsync(s => s.Id == supplierId && s.DeletedAt == null);

        if (supplier == null)
        {
            return new AdminSupplierActionResponse
            {
                Success = false,
                Message = "Supplier not found",
                MessageAr = "المورد غير موجود"
            };
        }

        var oldStatus = supplier.Status;

        supplier.Status = "inactive";
        supplier.AdminNotes = request.AdminNotes;
        supplier.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // ✅ تسجيل العملية
        await _logService.LogAsync(
            userId: adminId,
            userType: "admin",
            userName: "Admin",
            action: "deactivate",
            actionAr: "إيقاف",
            entityType: "supplier_profile",
            entityTypeAr: "مورد",
            entityId: supplier.Id,
            oldValues: new { Status = oldStatus },
            newValues: new { Status = supplier.Status },
            description: $"تم إيقاف حساب المورد: {supplier.FullName}"
        );

        return new AdminSupplierActionResponse
        {
            Success = true,
            Message = "Supplier deactivated successfully",
            MessageAr = "تم إيقاف المورد بنجاح"
        };
    }

    /// <summary>
    /// حذف مورد (Soft Delete)
    /// </summary>
    public async Task<AdminSupplierActionResponse> DeleteSupplierAsync(long supplierId, long adminId)
    {
        var supplier = await _context.SupplierProfiles
            .FirstOrDefaultAsync(s => s.Id == supplierId && s.DeletedAt == null);

        if (supplier == null)
        {
            return new AdminSupplierActionResponse
            {
                Success = false,
                Message = "Supplier not found",
                MessageAr = "المورد غير موجود"
            };
        }

        var oldStatus = supplier.Status;

        supplier.DeletedAt = DateTime.UtcNow;
        supplier.Status = "deleted";

        await _context.SaveChangesAsync();

        // ✅ تسجيل العملية
        await _logService.LogAsync(
            userId: adminId,
            userType: "admin",
            userName: "Admin",
            action: "delete",
            actionAr: "حذف",
            entityType: "supplier_profile",
            entityTypeAr: "مورد",
            entityId: supplier.Id,
            oldValues: new { Status = oldStatus },
            newValues: new { Status = "deleted" },
            description: $"تم حذف حساب المورد: {supplier.FullName}"
        );

        return new AdminSupplierActionResponse
        {
            Success = true,
            Message = "Supplier deleted successfully",
            MessageAr = "تم حذف المورد بنجاح"
        };
    }

    /// <summary>
    /// توثيق المورد (موافقة/رفض)
    /// </summary>
    public async Task<AdminSupplierActionResponse> VerifySupplierAsync(long adminId, VerifySupplierRequest request)
    {
        var supplier = await _context.SupplierProfiles
            .FirstOrDefaultAsync(s => s.Id == request.SupplierId && s.DeletedAt == null);

        if (supplier == null)
        {
            return new AdminSupplierActionResponse
            {
                Success = false,
                Message = "Supplier not found",
                MessageAr = "المورد غير موجود"
            };
        }

        var oldVerificationStatus = supplier.VerificationStatus;
        var oldIsVerified = supplier.IsVerified;

        if (request.IsApproved)
        {
            // الموافقة على التوثيق
            supplier.IsVerified = true;
            supplier.VerificationStatus = "approved";
            supplier.VerifiedAt = DateTime.UtcNow;
            supplier.RejectionReason = null;
            // ✅ إنشاء اشتراك في الباقة المجانية تلقائياً (لو موجودة)
            var hasActiveSubscription = await _context.Subscriptions
                .AnyAsync(s => s.SupplierId == supplier.Id && s.Status == "active");

            if (!hasActiveSubscription)
            {
                // البحث عن أي باقة مجانية نشطة (Price = 0)
                var freePlan = await _context.SubscriptionPlans
                    .Where(p => p.Price == 0 && p.IsActive)
                    .OrderBy(p => p.Id)
                    .FirstOrDefaultAsync();

                // لو فيه باقة مجانية، سجل المورد فيها
                if (freePlan != null)
                {
                    var subscription = new Subscription
                    {
                        SupplierId = supplier.Id,
                        PlanId = freePlan.Id,
                        Status = "active",
                        StartsAt = DateTime.UtcNow,
                        EndsAt = DateTime.UtcNow.AddDays(freePlan.DurationDays),
                        AmountPaid = 0,
                        PaymentMethod = "free",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.Subscriptions.Add(subscription);
                }
                // لو مفيش باقة مجانية، المورد يشترك يدوياً بعدين
            }
        }
        else
        {
            // رفض التوثيق
            supplier.IsVerified = false;
            supplier.VerificationStatus = "rejected";
            supplier.RejectionReason = request.RejectionReason;

            // حفظ المستندات المطلوبة
            if (request.RequiredDocuments != null && request.RequiredDocuments.Any())
            {
                supplier.RequiredDocuments = JsonSerializer.Serialize(request.RequiredDocuments);
            }
            else
            {
                supplier.RequiredDocuments = null;
            }
        }

        supplier.VerificationReviewedAt = DateTime.UtcNow;

        if (!string.IsNullOrEmpty(request.AdminNotes))
            supplier.AdminNotes = request.AdminNotes;

        supplier.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // ✅ تسجيل العملية
        await _logService.LogAsync(
            userId: adminId,
            userType: "admin",
            userName: "Admin",
            action: request.IsApproved ? "approve" : "reject",
            actionAr: request.IsApproved ? "توثيق" : "رفض",
            entityType: "supplier_profile",
            entityTypeAr: "مورد",
            entityId: supplier.Id,
            oldValues: new { VerificationStatus = oldVerificationStatus, IsVerified = oldIsVerified },
            newValues: new { VerificationStatus = supplier.VerificationStatus, IsVerified = supplier.IsVerified },
            description: request.IsApproved
                ? $"تم توثيق المورد: {supplier.FullName}"
                : $"تم رفض توثيق المورد: {supplier.FullName} - السبب: {request.RejectionReason}"
        );

        return new AdminSupplierActionResponse
        {
            Success = true,
            Message = request.IsApproved ? "Supplier verified successfully" : "Verification rejected",
            MessageAr = request.IsApproved ? "تم توثيق المورد بنجاح" : "تم رفض التوثيق"
        };
    }

    #region Helper Methods

    private string? GetFullUrl(string? path)
    {
        if (string.IsNullOrEmpty(path)) return null;
        if (path.StartsWith("http")) return path;

        var baseUrl = _configuration["AppSettings:BaseUrl"] ?? "";
        return $"{baseUrl}/{path.TrimStart('/')}";
    }

    #endregion
}