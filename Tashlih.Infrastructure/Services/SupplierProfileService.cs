using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tashlih.Application.DTOs.SupplierProfile;
using Tashlih.Application.Interfaces;
using Tashlih.Infrastructure.Models;
using Tashlih.Core.Entities;
using System.Text.Json;


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
                .Where(p => p.SupplierId == profile.Id && p.DeletedAt == null)
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
                .Where(p => p.SupplierId == profile.Id && p.DeletedAt == null && p.Status == "available")
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
                .Where(p => p.SupplierId == profile.Id && p.DeletedAt == null)
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
        /// إعادة رفع المستندات بعد الرفض
        /// </summary>
        public async Task<VerificationResponse> ResubmitVerificationAsync(long supplierId, ResubmitVerificationRequest request)
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

            // التحقق إن المورد مرفوض
            if (profile.VerificationStatus != "rejected")
            {
                return new VerificationResponse
                {
                    Success = false,
                    Message = "Only rejected suppliers can resubmit documents",
                    MessageAr = "فقط الموردين المرفوضين يمكنهم إعادة رفع المستندات"
                };
            }

            try
            {
                // رفع المستند الأول
                var document1Url = await _fileService.UploadFileAsync(request.Document1, $"suppliers/{supplierId}/documents");
                if (!SaveDocumentUrl(profile, request.DocumentType1, document1Url))
                {
                    return new VerificationResponse
                    {
                        Success = false,
                        Message = "Invalid document type for Document1",
                        MessageAr = "نوع المستند الأول غير صحيح"
                    };
                }

                // رفع المستند الثاني (لو موجود)
                if (request.Document2 != null && !string.IsNullOrEmpty(request.DocumentType2))
                {
                    var document2Url = await _fileService.UploadFileAsync(request.Document2, $"suppliers/{supplierId}/documents");
                    if (!SaveDocumentUrl(profile, request.DocumentType2, document2Url))
                    {
                        return new VerificationResponse
                        {
                            Success = false,
                            Message = "Invalid document type for Document2",
                            MessageAr = "نوع المستند الثاني غير صحيح"
                        };
                    }
                }

                // تغيير الحالة لـ pending_review
                profile.VerificationStatus = "pending_review";
                profile.RejectionReason = null;
                profile.RequiredDocuments = null;
                profile.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return new VerificationResponse
                {
                    Success = true,
                    Message = "Documents submitted successfully, pending review",
                    MessageAr = "تم رفع المستندات بنجاح، في انتظار المراجعة",
                    VerificationStatus = profile.VerificationStatus
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
        /// حفظ URL المستند حسب النوع
        /// </summary>
        private bool SaveDocumentUrl(SupplierProfile profile, string documentType, string url)
        {
            switch (documentType.ToLower())
            {
                case "id_front":
                    profile.IdFrontUrl = url;
                    return true;
                case "id_back":
                    profile.IdBackUrl = url;
                    return true;
                case "commercial_register":
                    profile.CommercialRegisterImageUrl = url;
                    return true;
                case "license":
                    profile.LicenseImageUrl = url;
                    return true;
                case "tax_certificate":
                    profile.TaxCertificateUrl = url;
                    return true;
                default:
                    return false;
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
                RequiredDocuments = !string.IsNullOrEmpty(profile.RequiredDocuments)
                 ? JsonSerializer.Deserialize<List<string>>(profile.RequiredDocuments)
                 : null, // ✅ الجديد
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

            var totalParts = await _context.Parts
                .Where(p => p.SupplierId == profile.Id && p.DeletedAt == null)
                .CountAsync();

            var availableParts = await _context.Parts
                .Where(p => p.SupplierId == profile.Id && p.DeletedAt == null && p.Status == "available")
                .CountAsync();

            var soldParts = await _context.Parts
                .Where(p => p.SupplierId == profile.Id && p.DeletedAt == null && p.Status == "sold")
                .CountAsync();

            var orders = await _context.Orders
                .Where(o => o.SupplierId == profile.Id)
                .ToListAsync();

            var totalViews = await _context.Parts
                .Where(p => p.SupplierId == profile.Id && p.DeletedAt == null)
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
                    VerificationStatus = profile.VerificationStatus,
                    RejectionReason = profile.RejectionReason
                }
            };
        }

        /// <summary>
        /// حذف الحساب
        /// </summary>
        public async Task<DeleteSupplierAccountResponse> DeleteAccountAsync(long supplierId, DeleteSupplierAccountRequest request)
        {
            var supplier = await _context.SupplierProfiles
                .FirstOrDefaultAsync(s => s.Id == supplierId && s.DeletedAt == null);

            if (supplier == null)
            {
                return new DeleteSupplierAccountResponse
                {
                    Success = false,
                    Message = "Supplier not found",
                    MessageAr = "المورد غير موجود"
                };
            }

            // التحقق من كلمة المرور
            if (!BCrypt.Net.BCrypt.Verify(request.Password, supplier.PasswordHash))
            {
                return new DeleteSupplierAccountResponse
                {
                    Success = false,
                    Message = "Incorrect password",
                    MessageAr = "كلمة المرور غير صحيحة"
                };
            }

            // Soft Delete
            supplier.DeletedAt = DateTime.UtcNow;
            supplier.UpdatedAt = DateTime.UtcNow;

            // إلغاء كل الـ Sessions
            var sessions = await _context.SupplierSessions
                .Where(s => s.SupplierId == supplierId && s.IsActive)
                .ToListAsync();

            foreach (var session in sessions)
            {
                session.IsActive = false;
            }

            await _context.SaveChangesAsync();

            return new DeleteSupplierAccountResponse
            {
                Success = true,
                Message = "Account deleted successfully",
                MessageAr = "تم حذف الحساب بنجاح"
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
                LogoUrl = profile.LogoUrl,
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
               
                PartsCount = partsCount,
                 IsPhoneVerified = profile.IsPhoneVerified
            };
        }
    }
}