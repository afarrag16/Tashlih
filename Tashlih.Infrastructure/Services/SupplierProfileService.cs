using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tashlih.Application.DTOs.SupplierProfile;
using Tashlih.Application.Interfaces;
using Tashlih.Infrastructure.Models;
using Tashlih.Core.Entities;

namespace Tashlih.Infrastructure.Services
{
    public class SupplierProfileService : ISupplierProfileService
    {
        private readonly TashlihContext _context;
        private readonly IFileService _fileService;

        public SupplierProfileService(TashlihContext context, IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        /// <summary>
        /// الحصول على ملف المورد الحالي
        /// </summary>
        public async Task<SupplierProfileResponse> GetMyProfileAsync(long supplierId)
        {
            var profile = await _context.SupplierProfiles
                .Include(s => s.Shops)
                .FirstOrDefaultAsync(s => s.Id == supplierId && s.DeletedAt == null);

            if (profile == null)
            {
                return new SupplierProfileResponse
                {
                    Success = false,
                    Message = "Supplier profile not found",
                    MessageAr = "لم يتم العثور على ملف المورد"
                };
            }

            var partsCount = await _context.Parts
                .Where(p => p.Shop.SupplierId == profile.Id && p.DeletedAt == null)
                .CountAsync();

            return new SupplierProfileResponse
            {
                Success = true,
                Message = "Success",
                MessageAr = "تم بنجاح",
                Profile = MapToDto(profile, partsCount)
            };
        }

        /// <summary>
        /// الحصول على ملف مورد بالـ ID (للعرض العام)
        /// </summary>
        public async Task<SupplierProfileResponse> GetProfileByIdAsync(long supplierId)
        {
            var profile = await _context.SupplierProfiles
                .Include(s => s.Shops)
                .FirstOrDefaultAsync(s => s.Id == supplierId && s.Status == "active" && s.DeletedAt == null);

            if (profile == null)
            {
                return new SupplierProfileResponse
                {
                    Success = false,
                    Message = "Supplier not found",
                    MessageAr = "المورد غير موجود"
                };
            }

            var partsCount = await _context.Parts
                .Where(p => p.Shop.SupplierId == profile.Id && p.DeletedAt == null && p.Status == "available")
                .CountAsync();

            return new SupplierProfileResponse
            {
                Success = true,
                Message = "Success",
                MessageAr = "تم بنجاح",
                Profile = MapToDto(profile, partsCount)
            };
        }

        /// <summary>
        /// تحديث ملف المورد
        /// </summary>
        public async Task<SupplierProfileResponse> UpdateProfileAsync(long supplierId, UpdateSupplierProfileRequest request)
        {
            var profile = await _context.SupplierProfiles
                .FirstOrDefaultAsync(s => s.Id == supplierId && s.DeletedAt == null);

            if (profile == null)
            {
                return new SupplierProfileResponse
                {
                    Success = false,
                    Message = "Supplier profile not found",
                    MessageAr = "لم يتم العثور على ملف المورد"
                };
            }

            // تحديث البيانات الشخصية
            if (!string.IsNullOrEmpty(request.FullName))
                profile.FullName = request.FullName;

            if (request.Email != null)
                profile.Email = request.Email;

            // تحديث بيانات النشاط
            if (!string.IsNullOrEmpty(request.BusinessNameAr))
                profile.BusinessNameAr = request.BusinessNameAr;

            if (request.BusinessNameEn != null)
                profile.BusinessNameEn = request.BusinessNameEn;

            if (request.BusinessType != null)
                profile.BusinessType = request.BusinessType;

            if (request.ManagerName != null)
                profile.ManagerName = request.ManagerName;

            if (request.Description != null)
                profile.Description = request.Description;

            if (request.City != null)
                profile.City = request.City;

            if (request.District != null)
                profile.District = request.District;

            // تحديث اللغة
            if (request.PreferredLanguage != null)
                profile.PreferredLanguage = request.PreferredLanguage;

            profile.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var partsCount = await _context.Parts
                .Where(p => p.Shop.SupplierId == profile.Id && p.DeletedAt == null)
                .CountAsync();

            return new SupplierProfileResponse
            {
                Success = true,
                Message = "Profile updated successfully",
                MessageAr = "تم تحديث الملف بنجاح",
                Profile = MapToDto(profile, partsCount)
            };
        }

        /// <summary>
        /// رفع مستند للتوثيق
        /// </summary>
        public async Task<VerificationResponse> UploadVerificationDocumentAsync(long supplierId, UploadVerificationDocumentRequest request)
        {
            var profile = await _context.SupplierProfiles
                .FirstOrDefaultAsync(s => s.Id == supplierId && s.DeletedAt == null);

            if (profile == null)
            {
                return new VerificationResponse
                {
                    Success = false,
                    Message = "Supplier profile not found",
                    MessageAr = "لم يتم العثور على ملف المورد"
                };
            }

            try
            {
                // رفع الملف
                var documentUrl = await _fileService.UploadFileAsync(request.Document, $"suppliers/{supplierId}/documents");

                // حفظ الـ URL حسب نوع المستند
                switch (request.DocumentType.ToLower())
                {
                    case "id_front":
                        profile.IdFrontUrl = documentUrl;
                        break;
                    case "id_back":
                        profile.IdBackUrl = documentUrl;
                        break;
                    case "commercial_register":
                    case "cr_image":
                        profile.CommercialRegisterImageUrl = documentUrl;
                        break;
                    case "license":
                    case "license_image":
                        profile.LicenseImageUrl = documentUrl;
                        break;
                    case "tax_certificate":
                        profile.TaxCertificateUrl = documentUrl;
                        break;
                    default:
                        return new VerificationResponse
                        {
                            Success = false,
                            Message = "Invalid document type. Valid types: id_front, id_back, commercial_register, license, tax_certificate",
                            MessageAr = "نوع المستند غير صحيح. الأنواع المتاحة: id_front, id_back, commercial_register, license, tax_certificate"
                        };
                }

                profile.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return new VerificationResponse
                {
                    Success = true,
                    Message = "Document uploaded successfully",
                    MessageAr = "تم رفع المستند بنجاح",
                    VerificationStatus = profile.VerificationStatus,
                    DocumentUrl = documentUrl
                };
            }
            catch (ArgumentException ex)
            {
                return new VerificationResponse
                {
                    Success = false,
                    Message = ex.Message,
                    MessageAr = ex.Message
                };
            }
        }

        /// <summary>
        /// تحديث بيانات التوثيق (أرقام، تواريخ)
        /// </summary>
        public async Task<VerificationResponse> UpdateVerificationDataAsync(long supplierId, UpdateVerificationDataRequest request)
        {
            var profile = await _context.SupplierProfiles
                .FirstOrDefaultAsync(s => s.Id == supplierId && s.DeletedAt == null);

            if (profile == null)
            {
                return new VerificationResponse
                {
                    Success = false,
                    Message = "Supplier profile not found",
                    MessageAr = "لم يتم العثور على ملف المورد"
                };
            }

            // رقم الهوية
            if (request.IdNumber != null)
                profile.IdNumber = request.IdNumber;

            // السجل التجاري
            if (request.CrNumber != null)
                profile.CommercialRegister = request.CrNumber;

            if (request.CrExpiryDate.HasValue)
                profile.CommercialRegisterExpiryDate = request.CrExpiryDate;

            // الرخصة
            if (request.LicenseNumber != null)
                profile.LicenseNumber = request.LicenseNumber;

            if (request.LicenseExpiryDate.HasValue)
                profile.LicenseExpiryDate = request.LicenseExpiryDate;

            // الرقم الضريبي
            if (request.TaxNumber != null)
                profile.TaxNumber = request.TaxNumber;

            profile.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return new VerificationResponse
            {
                Success = true,
                Message = "Verification data updated successfully",
                MessageAr = "تم تحديث بيانات التوثيق بنجاح",
                VerificationStatus = profile.VerificationStatus
            };
        }

        /// <summary>
        /// الحصول على حالة التوثيق والمستندات
        /// </summary>
        public async Task<VerificationResponse> GetVerificationStatusAsync(long supplierId)
        {
            var profile = await _context.SupplierProfiles
                .FirstOrDefaultAsync(s => s.Id == supplierId && s.DeletedAt == null);

            if (profile == null)
            {
                return new VerificationResponse
                {
                    Success = false,
                    Message = "Supplier profile not found",
                    MessageAr = "لم يتم العثور على ملف المورد"
                };
            }

            var documents = new List<VerificationDocumentDto>();

            // الهوية - الوجه الأمامي
            documents.Add(new VerificationDocumentDto
            {
                DocumentType = "id_front",
                DocumentTypeAr = "الهوية - الوجه الأمامي",
                DocumentUrl = profile.IdFrontUrl,
                IsUploaded = !string.IsNullOrEmpty(profile.IdFrontUrl),
                IsRequired = true
            });

            // الهوية - الوجه الخلفي
            documents.Add(new VerificationDocumentDto
            {
                DocumentType = "id_back",
                DocumentTypeAr = "الهوية - الوجه الخلفي",
                DocumentUrl = profile.IdBackUrl,
                IsUploaded = !string.IsNullOrEmpty(profile.IdBackUrl),
                IsRequired = false
            });

            // السجل التجاري
            documents.Add(new VerificationDocumentDto
            {
                DocumentType = "commercial_register",
                DocumentTypeAr = "السجل التجاري",
                DocumentUrl = profile.CommercialRegisterImageUrl,
                IsUploaded = !string.IsNullOrEmpty(profile.CommercialRegisterImageUrl),
                IsRequired = true,
                Number = profile.CommercialRegister,
                ExpiryDate = profile.CommercialRegisterExpiryDate
            });

            // الرخصة
            documents.Add(new VerificationDocumentDto
            {
                DocumentType = "license",
                DocumentTypeAr = "رخصة النشاط",
                DocumentUrl = profile.LicenseImageUrl,
                IsUploaded = !string.IsNullOrEmpty(profile.LicenseImageUrl),
                IsRequired = false,
                Number = profile.LicenseNumber,
                ExpiryDate = profile.LicenseExpiryDate
            });

            // شهادة الضريبة
            documents.Add(new VerificationDocumentDto
            {
                DocumentType = "tax_certificate",
                DocumentTypeAr = "شهادة الضريبة",
                DocumentUrl = profile.TaxCertificateUrl,
                IsUploaded = !string.IsNullOrEmpty(profile.TaxCertificateUrl),
                IsRequired = false
            });

            return new VerificationResponse
            {
                Success = true,
                Message = "Success",
                MessageAr = "تم بنجاح",
                IsVerified = profile.IsVerified,
                VerificationStatus = profile.VerificationStatus,
                Documents = documents,
                RejectionReason = profile.RejectionReason,
                AdminNotes = profile.AdminNotes,
                SubmittedAt = profile.VerificationSubmittedAt,
                ReviewedAt = profile.VerificationReviewedAt
            };
        }

        /// <summary>
        /// طلب التوثيق (إرسال للمراجعة)
        /// </summary>
        public async Task<VerificationResponse> RequestVerificationAsync(long supplierId)
        {
            var profile = await _context.SupplierProfiles
                .FirstOrDefaultAsync(s => s.Id == supplierId && s.DeletedAt == null);

            if (profile == null)
            {
                return new VerificationResponse
                {
                    Success = false,
                    Message = "Supplier profile not found",
                    MessageAr = "لم يتم العثور على ملف المورد"
                };
            }

            // التحقق من أن الحساب غير موثق
            if (profile.IsVerified)
            {
                return new VerificationResponse
                {
                    Success = false,
                    Message = "Already verified",
                    MessageAr = "الحساب موثق مسبقاً",
                    IsVerified = true,
                    VerificationStatus = profile.VerificationStatus
                };
            }

            // التحقق من أن الطلب ليس قيد المراجعة
            if (profile.VerificationStatus == "pending_review")
            {
                return new VerificationResponse
                {
                    Success = false,
                    Message = "Verification request already submitted",
                    MessageAr = "طلب التوثيق مقدم مسبقاً وقيد المراجعة",
                    VerificationStatus = profile.VerificationStatus
                };
            }

            // التحقق من المستندات المطلوبة
            var missingDocs = new List<string>();

            if (string.IsNullOrEmpty(profile.IdFrontUrl))
                missingDocs.Add("صورة الهوية (الوجه الأمامي)");

            if (string.IsNullOrEmpty(profile.CommercialRegisterImageUrl))
                missingDocs.Add("صورة السجل التجاري");

            if (string.IsNullOrEmpty(profile.CommercialRegister))
                missingDocs.Add("رقم السجل التجاري");

            if (missingDocs.Any())
            {
                return new VerificationResponse
                {
                    Success = false,
                    Message = "Missing required documents",
                    MessageAr = "المستندات المطلوبة الناقصة: " + string.Join("، ", missingDocs),
                    VerificationStatus = profile.VerificationStatus
                };
            }

            // تحديث حالة التوثيق
            profile.VerificationStatus = "pending_review";
            profile.VerificationSubmittedAt = DateTime.UtcNow;
            profile.RejectionReason = null; // مسح سبب الرفض السابق إن وجد
            profile.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new VerificationResponse
            {
                Success = true,
                Message = "Verification request submitted successfully",
                MessageAr = "تم إرسال طلب التوثيق بنجاح وسيتم مراجعته قريباً",
                VerificationStatus = "pending_review",
                SubmittedAt = profile.VerificationSubmittedAt
            };
        }

        /// <summary>
        /// توثيق المورد (للأدمن فقط)
        /// </summary>
        public async Task<VerificationResponse> VerifySupplierAsync(long adminId, VerifySupplierRequest request)
        {
            var profile = await _context.SupplierProfiles
                .FirstOrDefaultAsync(s => s.Id == request.SupplierId && s.DeletedAt == null);

            if (profile == null)
            {
                return new VerificationResponse
                {
                    Success = false,
                    Message = "Supplier not found",
                    MessageAr = "المورد غير موجود"
                };
            }

            if (request.IsApproved)
            {
                // الموافقة على التوثيق
                profile.IsVerified = true;
                profile.VerificationStatus = "approved";
                profile.VerifiedAt = DateTime.UtcNow;
                profile.VerifiedBy = adminId;
                profile.RejectionReason = null;
            }
            else
            {
                // رفض التوثيق
                profile.IsVerified = false;
                profile.VerificationStatus = "rejected";
                profile.RejectionReason = request.RejectionReason;
            }

            profile.VerificationReviewedAt = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(request.AdminNotes))
                profile.AdminNotes = request.AdminNotes;

            profile.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new VerificationResponse
            {
                Success = true,
                Message = request.IsApproved ? "Supplier verified successfully" : "Verification rejected",
                MessageAr = request.IsApproved ? "تم توثيق المورد بنجاح" : "تم رفض التوثيق",
                IsVerified = profile.IsVerified,
                VerificationStatus = profile.VerificationStatus,
                RejectionReason = profile.RejectionReason,
                ReviewedAt = profile.VerificationReviewedAt
            };
        }

        /// <summary>
        /// الحصول على إحصائيات المورد
        /// </summary>
        public async Task<SupplierStatsResponse> GetSupplierStatsAsync(long supplierId)
        {
            var profile = await _context.SupplierProfiles
                .FirstOrDefaultAsync(s => s.Id == supplierId && s.DeletedAt == null);

            if (profile == null)
            {
                return new SupplierStatsResponse
                {
                    Success = false,
                    Message = "Supplier profile not found",
                    MessageAr = "لم يتم العثور على ملف المورد"
                };
            }

            var shopIds = await _context.Shops
                .Where(s => s.SupplierId == profile.Id && s.DeletedAt == null)
                .Select(s => s.Id)
                .ToListAsync();

            var totalParts = await _context.Parts
                .Where(p => shopIds.Contains(p.ShopId) && p.DeletedAt == null)
                .CountAsync();

            var availableParts = await _context.Parts
                .Where(p => shopIds.Contains(p.ShopId) && p.DeletedAt == null && p.Status == "available")
                .CountAsync();

            var soldParts = await _context.Parts
                .Where(p => shopIds.Contains(p.ShopId) && p.DeletedAt == null && p.Status == "sold")
                .CountAsync();

            var orders = await _context.Orders
                .Where(o => o.SupplierId == profile.Id)
                .ToListAsync();

            var totalViews = await _context.Parts
                .Where(p => shopIds.Contains(p.ShopId) && p.DeletedAt == null)
                .SumAsync(p => (int?)p.ViewsCount ?? 0);

            var totalRevenue = orders
                .Where(o => o.Status == "completed")
                .Sum(o => o.TotalAmount);

            var thisMonthOrders = orders
                .Where(o => o.CreatedAt >= new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1))
                .Count();

            var thisMonthRevenue = orders
                .Where(o => o.Status == "completed" && o.CreatedAt >= new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1))
                .Sum(o => o.TotalAmount);

            return new SupplierStatsResponse
            {
                Success = true,
                Message = "Success",
                MessageAr = "تم بنجاح",
                Stats = new SupplierStatsDto
                {
                    TotalShops = shopIds.Count,
                    TotalParts = totalParts,
                    AvailableParts = availableParts,
                    SoldParts = soldParts,
                    TotalOrders = orders.Count,
                    PendingOrders = orders.Count(o => o.Status == "pending"),
                    CompletedOrders = orders.Count(o => o.Status == "completed"),
                    CancelledOrders = orders.Count(o => o.Status == "cancelled"),
                    TotalRevenue = totalRevenue,
                    ThisMonthOrders = thisMonthOrders,
                    ThisMonthRevenue = thisMonthRevenue,
                    RatingAverage = profile.RatingAverage,
                    RatingCount = profile.RatingCount,
                    TotalViews = totalViews,
                    IsVerified = profile.IsVerified,
                    VerificationStatus = profile.VerificationStatus
                }
            };
        }

        /// <summary>
        /// تحويل Entity إلى DTO
        /// </summary>
        private SupplierProfileDto MapToDto(SupplierProfile profile, int partsCount)
        {
            return new SupplierProfileDto
            {
                Id = profile.Id,
                FullName = profile.FullName,
                Phone = profile.Phone,
                Email = profile.Email,
                BusinessNameAr = profile.BusinessNameAr,
                BusinessNameEn = profile.BusinessNameEn,
                BusinessType = profile.BusinessType,
                ManagerName = profile.ManagerName,
                Description = profile.Description,
                City = profile.City,
                District = profile.District,
                CommercialRegister = profile.CommercialRegister,
                LicenseNumber = profile.LicenseNumber,
                TaxNumber = profile.TaxNumber,
                RatingAverage = profile.RatingAverage,
                RatingCount = profile.RatingCount,
                TotalOrders = profile.TotalOrders,
                CompletedOrders = profile.CompletedOrders,
                IsVerified = profile.IsVerified,
                VerificationStatus = profile.VerificationStatus,
                VerifiedAt = profile.VerifiedAt,
                Status = profile.Status,
                PreferredLanguage = profile.PreferredLanguage,
                CreatedAt = profile.CreatedAt,
                ShopsCount = profile.Shops?.Count(s => s.DeletedAt == null) ?? 0,
                PartsCount = partsCount
            };
        }
    }
}