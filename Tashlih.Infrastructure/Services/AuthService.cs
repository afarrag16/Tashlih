using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;
using Tashlih.Application.DTOs.Auth;
using Tashlih.Application.Interfaces;
using Tashlih.Core.Entities;
using Tashlih.Infrastructure.Models;

namespace Tashlih.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly TashlihContext _context;
        private readonly IJwtService _jwtService;
        private readonly IFileService _fileService;
        private readonly IConfiguration _configuration;
        private readonly IOtpService _otpService;

        public AuthService(
            TashlihContext context,
            IJwtService jwtService,
            IFileService fileService,
            IConfiguration configuration,
            IOtpService otpService)
        {
            _context = context;
            _jwtService = jwtService;
            _fileService = fileService;
            _configuration = configuration;
            _otpService = otpService;
        }

        #region التسجيل

        public async Task<AuthResponse> RegisterCustomerAsync(CustomerRegisterRequest request)
        {
            if (await IsPhoneExistsAsync(request.Phone))
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Phone number already registered",
                    MessageAr = "رقم الجوال مسجل مسبقاً"
                };
            }

            var email = string.IsNullOrWhiteSpace(request.Email) ||
                request.Email == "user@example.com"
                ? null
                : request.Email.Trim().ToLower();

            if (email != null && await IsEmailExistsAsync(email))
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Email already registered",
                    MessageAr = "البريد الإلكتروني مسجل مسبقاً"
                };
            }

            var expiresAt = DateTime.UtcNow.AddMinutes(5);

            var user = new User
            {
                FullName = request.FullName,
                Phone = request.Phone,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                UserType = "customer",
                Status = "active",
                PreferredLanguage = request.PreferredLanguage ?? "ar",
                NotificationsEnabled = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsPhoneVerified = false,
                OtpExpiresAt = expiresAt,
                OtpPurpose = "verify"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // إرسال OTP عبر Twilio
            var smsSent = await _otpService.SendOtpSmsAsync(request.Phone);
            if (!smsSent)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Failed to send OTP",
                    MessageAr = "فشل في إرسال رمز التحقق"
                };
            }

            return new AuthResponse
            {
                Success = true,
                Message = "Registration successful. Please verify your phone",
                MessageAr = "تم التسجيل بنجاح. يرجى تأكيد رقم الجوال"
            };
        }

        public async Task<AuthResponse> RegisterSupplierAsync(SupplierRegisterRequest request)
        {
            if (await IsPhoneExistsAsync(request.Phone))
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Phone number already registered",
                    MessageAr = "رقم الجوال مسجل مسبقاً"
                };
            }

            var email = string.IsNullOrWhiteSpace(request.Email) ||
                     request.Email == "user@example.com"
             ? null
             : request.Email.Trim().ToLower();

            if (email != null && await IsEmailExistsAsync(email))
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Email already registered",
                    MessageAr = "البريد الإلكتروني مسجل مسبقاً"
                };
            }

            // ✅ التحقق من عدم تكرار السجل التجاري (CR)
            if (!string.IsNullOrWhiteSpace(request.CommercialRegisterNumber))
            {
                bool crExists = await _context.SupplierProfiles
                    .AnyAsync(s =>
                        s.CommercialRegister == request.CommercialRegisterNumber &&
                        s.DeletedAt == null);

                if (crExists)
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Commercial register already registered",
                        MessageAr = "السجل التجاري مسجل مسبقاً"
                    };
                }
            }

            try
            {
                var crImageUrl = await _fileService.UploadDocumentAsync(
                                request.CommercialRegisterImage,
                              "suppliers/commercial-register"
                                    );

                var idImageUrl = await _fileService.UploadDocumentAsync(
                    request.IdentityImage,
                    "suppliers/identity"
                );

                string? logoUrl = null;
                if (request.Logo != null)
                {
                    logoUrl = await _fileService.UploadImageAsync(
                        request.Logo,
                        "suppliers/logos"
                    );
                }

                var expiresAt = DateTime.UtcNow.AddMinutes(5);

                var supplier = new SupplierProfile
                {
                    FullName = request.FullName,
                    Phone = request.Phone,
                    Email = email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    PreferredLanguage = request.PreferredLanguage ?? "ar",
                    BusinessNameAr = request.BusinessNameAr,
                    BusinessNameEn = request.BusinessNameEn,
                    BusinessType = request.BusinessType,
                    City = request.City,
                    District = request.District,
                    CommercialRegister = request.CommercialRegisterNumber,
                    CommercialRegisterImageUrl = crImageUrl,
                    IdFrontUrl = idImageUrl,
                    LogoUrl = logoUrl,
                    Latitude = request.Latitude,
                    Longitude = request.Longitude,
                    Status = "active",
                    VerificationStatus = "pending_review",
                    VerificationSubmittedAt = DateTime.UtcNow,
                    IsVerified = false,
                    IsPhoneVerified = false,
                    OtpExpiresAt = expiresAt,
                    OtpPurpose = "verify",
                    RatingAverage = 0,
                    RatingCount = 0,
                    TotalOrders = 0,
                    CompletedOrders = 0,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.SupplierProfiles.Add(supplier);
                await _context.SaveChangesAsync();

                // إرسال OTP عبر Twilio
                var smsSent = await _otpService.SendOtpSmsAsync(request.Phone);
                if (!smsSent)
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Failed to send OTP",
                        MessageAr = "فشل في إرسال رمز التحقق"
                    };
                }

                return new AuthResponse
                {
                    Success = true,
                    Message = "Registration successful. Please verify your phone",
                    MessageAr = "تم التسجيل بنجاح. يرجى تأكيد رقم الجوال"
                };
            }
            catch (ArgumentException ex)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = ex.Message,
                    MessageAr = ex.Message
                };
            }
        }
        #endregion
        #region تسجيل الدخول

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            // البحث في العملاء
            var customer = await _context.Users
                .FirstOrDefaultAsync(u => u.Phone == request.Phone && u.DeletedAt == null);

            if (customer != null)
            {
                if (!BCrypt.Net.BCrypt.Verify(request.Password, customer.PasswordHash))
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Invalid credentials",
                        MessageAr = "بيانات الدخول غير صحيحة"
                    };
                }

                if (customer.Status == "blocked")
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Account is blocked",
                        MessageAr = "الحساب محظور"
                    };
                }
                if (!customer.IsPhoneVerified)
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Phone not verified. Please verify your phone first",
                        MessageAr = "رقم الجوال غير مؤكد. يرجى تأكيد رقم الجوال أولاً",
                        IsPhoneVerified = false
                    };
                }

                customer.LastLoginAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                //return await CreateCustomerAuthResponseAsync(customer, request.DeviceType, request.DeviceName, request.FcmToken);
                var response = await CreateCustomerAuthResponseAsync(customer, request.DeviceType, request.DeviceName, request.FcmToken);
                response.IsPhoneVerified = true;
                return response;
            }

            // البحث في الموردين
            var supplier = await _context.SupplierProfiles
                .FirstOrDefaultAsync(s => s.Phone == request.Phone && s.DeletedAt == null);

            if (supplier != null)
            {
                if (string.IsNullOrEmpty(supplier.PasswordHash) ||
                    !BCrypt.Net.BCrypt.Verify(request.Password, supplier.PasswordHash))
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Invalid credentials",
                        MessageAr = "بيانات الدخول غير صحيحة"
                    };
                }

                if (supplier.Status == "blocked")
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Account is blocked",
                        MessageAr = "الحساب محظور"
                    };
                }
                if (!supplier.IsPhoneVerified)
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Phone not verified. Please verify your phone first",
                        MessageAr = "رقم الجوال غير مؤكد. يرجى تأكيد رقم الجوال أولاً",
                        IsPhoneVerified = false
                    };
                }

                supplier.LastLoginAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                //var supplierResponse = await CreateSupplierAuthResponseAsync(supplier, request.DeviceType, request.DeviceName, request.FcmToken);
                supplier.LastLoginAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                var supplierResponse = await CreateSupplierAuthResponseAsync(supplier, request.DeviceType, request.DeviceName, request.FcmToken);
                supplierResponse.IsPhoneVerified = true;

                return new AuthResponse
                {
                    Success = supplierResponse.Success,
                    Message = supplierResponse.Message,
                    MessageAr = supplierResponse.MessageAr,
                    Token = supplierResponse.Token,
                    ExpiresAt = supplierResponse.ExpiresAt,
                    IsPhoneVerified = true,
                    User = supplierResponse.User != null ? new UserDto
                    {
                        Id = supplierResponse.User.Id,
                        FullName = supplierResponse.User.FullName,
                        Phone = supplierResponse.User.Phone,
                        Email = supplierResponse.User.Email,
                        UserType = "supplier",
                        Status = supplierResponse.User.Status,
                        IsVerified = supplierResponse.User.IsVerified,
                        VerificationStatus = supplierResponse.User.VerificationStatus,
                        RejectionReason = supplierResponse.User.RejectionReason,
                        AdminNotes = supplierResponse.User.AdminNotes,
                        PreferredLanguage = supplierResponse.User.PreferredLanguage,
                        CreatedAt = supplierResponse.User.CreatedAt ?? DateTime.UtcNow
                    } : null
                };
            }

            return new AuthResponse
            {
                Success = false,
                Message = "Invalid credentials",
                MessageAr = "بيانات الدخول غير صحيحة"
            };
        }

        #endregion

        #region التحقق من التكرار

        public async Task<bool> IsPhoneExistsAsync(string phone)
        {
            var inUsers = await _context.Users.AnyAsync(u => u.Phone == phone && u.DeletedAt == null);
            var inSuppliers = await _context.SupplierProfiles.AnyAsync(s => s.Phone == phone && s.DeletedAt == null);
            return inUsers || inSuppliers;
        }

        public async Task<bool> IsEmailExistsAsync(string? email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            email = email.Trim().ToLower();

            var inUsers = await _context.Users
                .AnyAsync(u =>
                    u.Email != null &&
                    u.Email == email &&
                    u.DeletedAt == null);

            if (inUsers)
                return true;

            var inSuppliers = await _context.SupplierProfiles
                .AnyAsync(s =>
                    s.Email != null &&
                    s.Email == email &&
                    s.DeletedAt == null);

            return inSuppliers;
        }


        #endregion

        #region كلمة المرور

        public async Task<AuthResponse> ChangePasswordAsync(long userId, string userType, ChangePasswordRequest request)
        {
            if (userType == "supplier")
            {
                var supplier = await _context.SupplierProfiles.FindAsync(userId);
                if (supplier == null)
                    return new AuthResponse { Success = false, Message = "User not found", MessageAr = "المستخدم غير موجود" };

                if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, supplier.PasswordHash))
                    return new AuthResponse { Success = false, Message = "Current password is incorrect", MessageAr = "كلمة المرور الحالية غير صحيحة" };

                supplier.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                supplier.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                    return new AuthResponse { Success = false, Message = "User not found", MessageAr = "المستخدم غير موجود" };

                if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
                    return new AuthResponse { Success = false, Message = "Current password is incorrect", MessageAr = "كلمة المرور الحالية غير صحيحة" };

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                user.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return new AuthResponse { Success = true, Message = "Password changed successfully", MessageAr = "تم تغيير كلمة المرور بنجاح" };
        }

        public async Task<AuthResponse> ResetPasswordAsync(ResetPasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Phone) ||
                string.IsNullOrWhiteSpace(request.OtpCode) ||
                string.IsNullOrWhiteSpace(request.NewPassword))
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Invalid request",
                    MessageAr = "بيانات غير صحيحة"
                };
            }

            var now = DateTime.UtcNow;
            const string purpose = "reset";

            // 1) هات الحساب + تحقق إن عنده Session صالحة
            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.Phone == request.Phone &&
                u.OtpPurpose == purpose &&
                u.OtpExpiresAt.HasValue &&
                u.OtpExpiresAt > now &&
                u.DeletedAt == null);

            SupplierProfile? supplier = null;

            if (user == null)
            {
                supplier = await _context.SupplierProfiles.FirstOrDefaultAsync(s =>
                    s.Phone == request.Phone &&
                    s.OtpPurpose == purpose &&
                    s.OtpExpiresAt.HasValue &&
                    s.OtpExpiresAt > now &&
                    s.DeletedAt == null);
            }

            if (user == null && supplier == null)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "OTP session not found or expired",
                    MessageAr = "لا يوجد طلب تحقق صالح أو انتهت صلاحيته"
                };
            }

            // 2) Verify من Twilio
            var twilioOk = await _otpService.VerifyOtpAsync(request.Phone, request.OtpCode);
            if (!twilioOk)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Invalid or expired OTP",
                    MessageAr = "رمز التحقق غير صحيح أو منتهي"
                };
            }

            // 3) نجاح - العميل
            if (user != null)
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                user.OtpCode = null;
                user.OtpExpiresAt = null;
                user.OtpPurpose = null;
                user.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return new AuthResponse
                {
                    Success = true,
                    Message = "Password reset successfully",
                    MessageAr = "تم إعادة تعيين كلمة المرور بنجاح"
                };
            }

            // 4) نجاح - المورد
            if (string.IsNullOrEmpty(supplier!.PasswordHash))
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Password reset is not allowed for this account",
                    MessageAr = "لا يمكن إعادة تعيين كلمة المرور لهذا الحساب"
                };
            }

            supplier.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            supplier.OtpCode = null;
            supplier.OtpExpiresAt = null;
            supplier.OtpPurpose = null;
            supplier.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return new AuthResponse
            {
                Success = true,
                Message = "Password reset successfully",
                MessageAr = "تم إعادة تعيين كلمة المرور بنجاح"
            };
        }


        #endregion

        #region تسجيل الخروج

        public async Task<bool> LogoutAsync(long userId, string userType, string token)
        {
            if (userType == "supplier")
            {
                var session = await _context.SupplierSessions
                    .FirstOrDefaultAsync(s => s.SupplierId == userId && s.Token == token && s.IsActive);

                if (session != null)
                {
                    session.IsActive = false;
                    session.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    return true;
                }
            }
            else
            {
                var session = await _context.UserSessions
                    .FirstOrDefaultAsync(s => s.UserId == userId && s.Token == token && s.IsActive);

                if (session != null)
                {
                    session.IsActive = false;
                    session.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    return true;
                }
            }

            return false;
        }

        public async Task<bool> LogoutAllAsync(long userId, string userType)
        {
            if (userType == "supplier")
            {
                var sessions = await _context.SupplierSessions
                    .Where(s => s.SupplierId == userId && s.IsActive)
                    .ToListAsync();

                foreach (var session in sessions)
                {
                    session.IsActive = false;
                    session.UpdatedAt = DateTime.UtcNow;
                }
            }
            else
            {
                var sessions = await _context.UserSessions
                    .Where(s => s.UserId == userId && s.IsActive)
                    .ToListAsync();

                foreach (var session in sessions)
                {
                    session.IsActive = false;
                    session.UpdatedAt = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region OTP

        //public async Task<AuthResponse> SendOtpAsync(SendOtpRequest request)
        //{
        //    if (string.IsNullOrWhiteSpace(request.Phone))
        //    {
        //        return new AuthResponse
        //        {
        //            Success = false,
        //            Message = "Phone is required",
        //            MessageAr = "رقم الجوال مطلوب"
        //        };
        //    }

        //    if (string.IsNullOrWhiteSpace(request.Purpose))
        //    {
        //        return new AuthResponse
        //        {
        //            Success = false,
        //            Message = "OTP purpose is required",
        //            MessageAr = "غرض رمز التحقق مطلوب"
        //        };
        //    }

        //    var purpose = request.Purpose.Trim().ToLowerInvariant();

        //    if (purpose is not ("verify" or "login" or "reset"))
        //    {
        //        return new AuthResponse
        //        {
        //            Success = false,
        //            Message = "Invalid OTP purpose",
        //            MessageAr = "غرض رمز التحقق غير صحيح"
        //        };
        //    }

        //    var otp = GenerateOtp();
        //    var expiresAt = DateTime.UtcNow.AddMinutes(5);

        //    // ====== البحث في المستخدمين ======
        //    var user = await _context.Users
        //        .FirstOrDefaultAsync(u => u.Phone == request.Phone && u.DeletedAt == null);

        //    if (user != null)
        //    {
        //        user.OtpCode = otp;
        //        user.OtpExpiresAt = expiresAt;
        //        user.OtpPurpose = purpose;              // ✅ إضافة الغرض
        //        user.UpdatedAt = DateTime.UtcNow;

        //        await _context.SaveChangesAsync();
        //    }
        //    else
        //    {
        //        // ====== البحث في الموردين ======
        //        var supplier = await _context.SupplierProfiles
        //            .FirstOrDefaultAsync(s => s.Phone == request.Phone && s.DeletedAt == null);

        //        if (supplier == null)
        //        {
        //            return new AuthResponse
        //            {
        //                Success = false,
        //                Message = "User not found",
        //                MessageAr = "المستخدم غير موجود"
        //            };
        //        }

        //        supplier.OtpCode = otp;
        //        supplier.OtpExpiresAt = expiresAt;
        //        supplier.OtpPurpose = purpose;          // ✅ إضافة الغرض
        //        supplier.UpdatedAt = DateTime.UtcNow;

        //        await _context.SaveChangesAsync();
        //    }
        //    // إرسال الـ OTP عبر SMS

        //    var smsSent = await _otpService.SendOtpSmsAsync(request.Phone);

        //    if (!smsSent)
        //    {
        //        return new AuthResponse
        //        {
        //            Success = false,
        //            Message = "Failed to send OTP",
        //            MessageAr = "فشل في إرسال رمز التحقق"
        //        };
        //    }

        //    return new AuthResponse
        //    {
        //        Success = true,
        //        Message = "OTP sent successfully",
        //        MessageAr = "تم إرسال رمز التحقق"
        //    };

        //}
        public async Task<AuthResponse> SendOtpAsync(SendOtpRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Phone))
                return Fail("Phone is required", "رقم الجوال مطلوب");

            if (string.IsNullOrWhiteSpace(request.Purpose))
                return Fail("OTP purpose is required", "غرض رمز التحقق مطلوب");

            var purpose = request.Purpose.Trim().ToLowerInvariant();
            if (purpose is not ("verify" or "login" or "reset"))
                return Fail("Invalid OTP purpose", "غرض رمز التحقق غير صحيح");

            var now = DateTime.UtcNow;
            var expiresAt = now.AddMinutes(5);

            // 1) هات الحساب الأول
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Phone == request.Phone && u.DeletedAt == null);
            SupplierProfile? supplier = null;

            if (user == null)
                supplier = await _context.SupplierProfiles.FirstOrDefaultAsync(s => s.Phone == request.Phone && s.DeletedAt == null);

            if (user == null && supplier == null)
                return Fail("User not found", "المستخدم غير موجود");

            // 2) حدّث session عندك (بدون تخزين OtpCode)
            if (user != null)
            {
                user.OtpPurpose = purpose;
                user.OtpExpiresAt = expiresAt;
                user.UpdatedAt = now;
            }
            else
            {
                supplier!.OtpPurpose = purpose;
                supplier.OtpExpiresAt = expiresAt;
                supplier.UpdatedAt = now;
            }

            await _context.SaveChangesAsync();

            // 3) إرسال Twilio Verify
            var smsSent = await _otpService.SendOtpSmsAsync(request.Phone);
            if (!smsSent)
                return Fail("Failed to send OTP", "فشل في إرسال رمز التحقق");

            return Success("OTP sent successfully", "تم إرسال رمز التحقق");
        }

        private static AuthResponse Fail(string en, string ar) => new AuthResponse { Success = false, Message = en, MessageAr = ar };
        private static AuthResponse Success(string en, string ar) => new AuthResponse { Success = true, Message = en, MessageAr = ar };



        //public async Task<AuthResponse> VerifyOtpAsync(VerifyOtpRequest request)
        //{
        //    if (string.IsNullOrWhiteSpace(request.Phone) || string.IsNullOrWhiteSpace(request.OtpCode))
        //    {
        //        return new AuthResponse
        //        {
        //            Success = false,
        //            Message = "Invalid request",
        //            MessageAr = "بيانات غير صحيحة"
        //        };
        //    }

        //    var now = DateTime.UtcNow;
        //    const string purpose = "verify"; // ✅ هذا endpoint خاص بتأكيد الهاتف فقط

        //    // ====== 1) تحقق في جدول users ======
        //    var user = await _context.Users.FirstOrDefaultAsync(u =>
        //        u.Phone == request.Phone &&
        //        u.OtpCode == request.OtpCode &&
        //        u.OtpPurpose == purpose &&           // ✅ شرط الغرض
        //        u.OtpExpiresAt.HasValue &&
        //        u.OtpExpiresAt > now &&
        //        u.DeletedAt == null);

        //    if (user != null)
        //    {
        //        // مسح OTP بعد الاستخدام
        //        user.OtpCode = null;
        //        user.OtpExpiresAt = null;
        //        user.OtpPurpose = null;              // ✅ مسح الغرض
        //        user.IsPhoneVerified = true;
        //        user.UpdatedAt = DateTime.UtcNow;

        //        await _context.SaveChangesAsync();

        //        // ✅ حسب طلبك: بعد الـ verify يطلع توكن عادي
        //        return await CreateCustomerAuthResponseAsync(user, null, null, null);
        //    }

        //    // ====== 2) تحقق في جدول supplier_profile ======
        //    var supplier = await _context.SupplierProfiles.FirstOrDefaultAsync(s =>
        //        s.Phone == request.Phone &&
        //        s.OtpCode == request.OtpCode &&
        //        s.OtpPurpose == purpose &&           // ✅ شرط الغرض
        //        s.OtpExpiresAt.HasValue &&
        //        s.OtpExpiresAt > now &&
        //        s.DeletedAt == null);

        //    if (supplier != null)
        //    {
        //        // مسح OTP بعد الاستخدام
        //        supplier.OtpCode = null;
        //        supplier.OtpExpiresAt = null;
        //        supplier.OtpPurpose = null;          // ✅ مسح الغرض
        //        supplier.IsPhoneVerified = true;
        //        supplier.UpdatedAt = DateTime.UtcNow;

        //        await _context.SaveChangesAsync();

        //        // المورد: تأكيد جوال فقط + انتظار موافقة الإدارة (زي ما كان عندك)
        //        return new AuthResponse
        //        {
        //            Success = true,
        //            Message = "Phone verified. Waiting for admin approval",
        //            MessageAr = "تم تأكيد رقم الجوال. في انتظار موافقة الإدارة"
        //        };
        //    }

        //    return new AuthResponse
        //    {
        //        Success = false,
        //        Message = "Invalid or expired OTP",
        //        MessageAr = "رمز التحقق غير صحيح أو منتهي"
        //    };
        //}
        public async Task<AuthResponse> VerifyOtpAsync(VerifyOtpRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Phone) || string.IsNullOrWhiteSpace(request.OtpCode))
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Invalid request",
                    MessageAr = "بيانات غير صحيحة"
                };
            }

            var now = DateTime.UtcNow;

            // 1) هات الحساب + تحقق إن عنده Session صالحة
            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.Phone == request.Phone &&
                u.DeletedAt == null &&
                u.OtpPurpose != null &&
                u.OtpExpiresAt.HasValue &&
                u.OtpExpiresAt > now);

            SupplierProfile? supplier = null;

            if (user == null)
            {
                supplier = await _context.SupplierProfiles.FirstOrDefaultAsync(s =>
                    s.Phone == request.Phone &&
                    s.DeletedAt == null &&
                    s.OtpPurpose != null &&
                    s.OtpExpiresAt.HasValue &&
                    s.OtpExpiresAt > now);
            }

            if (user == null && supplier == null)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "OTP session not found or expired",
                    MessageAr = "لا يوجد طلب تحقق صالح أو انتهت صلاحيته"
                };
            }

            // 2) Verify من Twilio
            var twilioOk = await _otpService.VerifyOtpAsync(request.Phone, request.OtpCode);
            if (!twilioOk)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Invalid or expired OTP",
                    MessageAr = "رمز التحقق غير صحيح أو منتهي"
                };
            }

            // 3) نجاح: جيب الـ purpose من الداتابيز
            if (user != null)
            {
                var purpose = user.OtpPurpose;

                user.OtpCode = null;
                user.OtpPurpose = null;
                user.OtpExpiresAt = null;
                user.UpdatedAt = now;

                if (purpose == "verify")
                    user.IsPhoneVerified = true;

                await _context.SaveChangesAsync();

                if (purpose == "reset")
                {
                    return new AuthResponse
                    {
                        Success = true,
                        Message = "OTP verified. You can reset your password.",
                        MessageAr = "تم التحقق. يمكنك إعادة تعيين كلمة المرور."
                    };
                }

                return await CreateCustomerAuthResponseAsync(user, null, null, null);
            }

            // المورد
            var supplierPurpose = supplier!.OtpPurpose;

            supplier.OtpCode = null;
            supplier.OtpPurpose = null;
            supplier.OtpExpiresAt = null;
            supplier.UpdatedAt = now;

            if (supplierPurpose == "verify")
                supplier.IsPhoneVerified = true;

            await _context.SaveChangesAsync();

            if (supplierPurpose == "reset")
            {
                return new AuthResponse
                {
                    Success = true,
                    Message = "OTP verified. You can reset your password.",
                    MessageAr = "تم التحقق. يمكنك إعادة تعيين كلمة المرور."
                };
            }

            return new AuthResponse
            {
                Success = true,
                Message = "Phone verified. Waiting for admin approval",
                MessageAr = "تم تأكيد رقم الجوال. في انتظار موافقة الإدارة"
            };
        }


        public async Task<AuthResponse> LoginWithOtpAsync(OtpLoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Phone) || string.IsNullOrWhiteSpace(request.OtpCode))
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Invalid request",
                    MessageAr = "بيانات غير صحيحة"
                };
            }

            var now = DateTime.UtcNow;
            const string purpose = "login";

            // 1) هات الحساب + تحقق إن عنده Session صالحة
            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.Phone == request.Phone &&
                u.OtpPurpose == purpose &&
                u.OtpExpiresAt.HasValue &&
                u.OtpExpiresAt > now &&
                u.DeletedAt == null);

            SupplierProfile? supplier = null;

            if (user == null)
            {
                supplier = await _context.SupplierProfiles.FirstOrDefaultAsync(s =>
                    s.Phone == request.Phone &&
                    s.OtpPurpose == purpose &&
                    s.OtpExpiresAt.HasValue &&
                    s.OtpExpiresAt > now &&
                    s.DeletedAt == null);
            }

            if (user == null && supplier == null)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "OTP session not found or expired",
                    MessageAr = "لا يوجد طلب تحقق صالح أو انتهت صلاحيته"
                };
            }

            // 2) Verify من Twilio
            var twilioOk = await _otpService.VerifyOtpAsync(request.Phone, request.OtpCode);
            if (!twilioOk)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Invalid or expired OTP",
                    MessageAr = "رمز التحقق غير صحيح أو منتهي"
                };
            }

            // 3) نجاح - العميل
            if (user != null)
            {
                user.OtpCode = null;
                user.OtpExpiresAt = null;
                user.OtpPurpose = null;
                user.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                if (!user.IsPhoneVerified)
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Phone not verified. Please verify your phone first",
                        MessageAr = "رقم الجوال غير مؤكد. يرجى تأكيد رقم الجوال أولاً",
                        IsPhoneVerified = false
                    };
                }

                var response = await CreateCustomerAuthResponseAsync(
                    user,
                    request.DeviceType,
                    request.DeviceName,
                    request.FcmToken
                );
                response.IsPhoneVerified = true;
                return response;
            }

            // 4) نجاح - المورد
            supplier!.OtpCode = null;
            supplier.OtpExpiresAt = null;
            supplier.OtpPurpose = null;
            supplier.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            if (!supplier.IsPhoneVerified)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Phone not verified. Please verify your phone first",
                    MessageAr = "رقم الجوال غير مؤكد. يرجى تأكيد رقم الجوال أولاً",
                    IsPhoneVerified = false
                };
            }

            var supplierResponse = await CreateSupplierAuthResponseAsync(
                supplier,
                request.DeviceType,
                request.DeviceName,
                request.FcmToken
            );
            supplierResponse.IsPhoneVerified = true;
            return supplierResponse;
        }


        #endregion

        #region Helper Methods

        private async Task<AuthResponse> CreateCustomerAuthResponseAsync(User user, string? deviceType, string? deviceName, string? fcmToken)
        {

          
            var accessToken = _jwtService.GenerateAccessToken(user);
            var expiryMinutes = _configuration.GetValue<int>("Jwt:ExpiryMinutes", 1440);
            var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

            var session = new UserSession
            {
                UserId = user.Id,
                Token = accessToken,
                DeviceType = deviceType,
                DeviceName = deviceName,
                FcmToken = IsValidFcmToken(fcmToken) ? fcmToken : null,
                ExpiresAt = expiresAt,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.UserSessions.Add(session);
            await _context.SaveChangesAsync();

            return new AuthResponse
            {
                Success = true,
                Message = "Login successful",
                MessageAr = "تم تسجيل الدخول بنجاح",
                Token = accessToken,
                ExpiresAt = expiresAt,
                User = new UserDto
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Phone = user.Phone,
                    Email = user.Email,
                    AvatarUrl = user.AvatarUrl,
                    UserType = "customer",
                    Status = user.Status,
                    PreferredLanguage = user.PreferredLanguage,
                    NotificationsEnabled = user.NotificationsEnabled,
                    CreatedAt = user.CreatedAt ?? DateTime.UtcNow
                }
            };
        }

        private async Task<AuthResponse> CreateSupplierAuthResponseAsync(
     SupplierProfile supplier,
     string? deviceType,
     string? deviceName,
     string? fcmToken)
        {
            // 1️⃣ Generate JWT
            var accessToken = _jwtService.GenerateSupplierAccessToken(supplier);

            // 2️⃣ Expiry
            var expiryMinutes = _configuration.GetValue<int>("Jwt:ExpiryMinutes", 1440);
            var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

            // 3️⃣ Session
            var session = new SupplierSession
            {
                SupplierId = supplier.Id,
                Token = accessToken,
                DeviceType = deviceType,
                DeviceName = deviceName,
                FcmToken = IsValidFcmToken(fcmToken) ? fcmToken : null,
                ExpiresAt = expiresAt,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.SupplierSessions.Add(session);
            await _context.SaveChangesAsync();

            // 4️⃣ Unified AuthResponse
            return new AuthResponse
            {
                Success = true,
                Message = "Login successful",
                MessageAr = "تم تسجيل الدخول بنجاح",
                Token = accessToken,
                ExpiresAt = expiresAt,
                User = new UserDto
                {
                    Id = supplier.Id,
                    FullName = supplier.FullName,
                    Phone = supplier.Phone,
                    Email = supplier.Email,
                    UserType = "supplier",
                    Status = supplier.Status,
                    IsVerified = supplier.IsVerified,
                    VerificationStatus = supplier.VerificationStatus,
                    RejectionReason = supplier.RejectionReason,
                    AdminNotes = supplier.AdminNotes,
                    PreferredLanguage = supplier.PreferredLanguage,
                    CreatedAt = supplier.CreatedAt
                }
            };
        }



        private string GenerateOtp()
        {
            return Random.Shared.Next(1000, 9999).ToString();
        }


        private bool IsValidFcmToken(string? token)
        {
            return !string.IsNullOrWhiteSpace(token)
                && token != "string"
                && token.Length >= 50;
        }




        #endregion
    }
}
