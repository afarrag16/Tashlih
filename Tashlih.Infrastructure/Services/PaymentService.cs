using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using Tashlih.Application.DTOs;
using Tashlih.Application.DTOs.Notification;
using Tashlih.Application.Interfaces;
using Tashlih.Core.Entities;
using Tashlih.Infrastructure.Models;

namespace Tashlih.Infrastructure.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly TashlihContext _context;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly INotificationService _notificationService;

        public PaymentService(TashlihContext context, HttpClient httpClient, IConfiguration configuration, INotificationService notificationService)
        {
            _context = context;
            _httpClient = httpClient;
            _configuration = configuration;
            _notificationService = notificationService;

            var apiKey = _configuration["MyFatoorah:ApiKey"];
            var baseUrl = _configuration["MyFatoorah:BaseUrl"];

            _httpClient.BaseAddress = new Uri(baseUrl!);
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        }

       
        public async Task<PaymentResponse> CreatePaymentAsync(long userId, string userType, CreatePaymentRequest request)
        {
           
            try
            {
                if (string.IsNullOrWhiteSpace(request.MobileCountryCode))
                {
                    return new PaymentResponse
                    {
                        Success = false,
                        Message = "Mobile country code is required",
                        MessageAr = "كود الدولة مطلوب"
                    };
                }

                long? subscriptionId = null;

                // ✅ لو فيه PlanId، اعمل Subscription جديد
                if (request.PlanId.HasValue)
                {
                    var plan = await _context.SubscriptionPlans
                        .FirstOrDefaultAsync(p => p.Id == request.PlanId.Value && p.IsActive);

                    if (plan == null)
                    {
                        return new PaymentResponse
                        {
                            Success = false,
                            Message = "Plan not found or inactive",
                            MessageAr = "الباقة غير موجودة أو غير متاحة"
                        };
                    }

                    // تأكد إن المبلغ صح
                    if (request.Amount != plan.Price)
                    {
                        return new PaymentResponse
                        {
                            Success = false,
                            Message = "Amount does not match plan price",
                            MessageAr = "المبلغ لا يتطابق مع سعر الباقة"
                        };
                    }
                   

                    // شوف لو عنده اشتراك pending قبل كده لنفس الباقة
                    var existingSubscription = await _context.Subscriptions
                        .FirstOrDefaultAsync(s => s.SupplierId == userId
                            && s.PlanId == plan.Id
                            && s.Status == "pending");

                    if (existingSubscription != null)
                    {
                        subscriptionId = existingSubscription.Id;
                        Console.WriteLine($"Using existing pending Subscription: Id={subscriptionId}");
                    }
                    else
                    {
                        // اعمل Subscription جديد
                        var subscription = new Subscription
                        {
                            SupplierId = userId,
                            PlanId = plan.Id,
                            Status = "pending",
                            AutoRenew = false,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };

                        _context.Subscriptions.Add(subscription);
                        await _context.SaveChangesAsync();

                        subscriptionId = subscription.Id;
                        Console.WriteLine($"Created new Subscription: Id={subscriptionId}, PlanId={plan.Id}");
                    }
                }

                // إنشاء سجل الدفع
                var payment = new Payment
                {
                    UserId = userId,
                    UserType = userType,
                    SubscriptionId = subscriptionId,
                    Amount = request.Amount,
                    Currency = "KWD",
                    Provider = "MyFatoorah",
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow
                };

                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();

                // تأمين الإيميل
                var customerEmail = request.CustomerEmail;
                if (string.IsNullOrWhiteSpace(customerEmail) || !customerEmail.Contains("@"))
                {
                    customerEmail = "test@example.com";
                }

                var payload = new
                {
                    PaymentMethodId = 2,
                    NotificationOption = "SMS",
                    CustomerName = request.CustomerName ?? "Customer",
                    DisplayCurrencyIso = "KWD",
                    MobileCountryCode = request.MobileCountryCode,
                    CustomerMobile = request.CustomerPhone ?? "50000000",
                    CustomerEmail = customerEmail,
                    InvoiceValue = request.Amount,
                    CallBackUrl = _configuration["MyFatoorah:CallBackUrl"],
                    ErrorUrl = _configuration["MyFatoorah:ErrorUrl"],
                    Language = "ar",
                    CustomerReference = payment.PaymentId.ToString()
                };

                var response = await _httpClient.PostAsJsonAsync("/v2/SendPayment", payload);
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine("MyFatoorah Response: " + responseContent);
                var result = await response.Content.ReadFromJsonAsync<MyFatoorahResponse>();

                if (result?.IsSuccess == true && result.Data != null)
                {
                    payment.InvoiceId = result.Data.InvoiceId.ToString();
                    payment.PaymentUrl = result.Data.InvoiceURL;
                    payment.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    return new PaymentResponse
                    {
                        Success = true,
                        Message = "Payment created successfully",
                        MessageAr = "تم إنشاء الفاتورة بنجاح",
                        PaymentUrl = result.Data.InvoiceURL,
                        InvoiceId = result.Data.InvoiceId.ToString(),
                        PaymentId = payment.PaymentId
                    };
                }

                // فشل إنشاء الفاتورة
                payment.Status = "Failed";
                payment.FailureReason = result?.Message ?? "Unknown error";
                payment.FailedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return new PaymentResponse
                {
                    Success = false,
                    Message = result?.Message ?? "Failed to create payment",
                    MessageAr = "فشل إنشاء الفاتورة"
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CreatePayment Error: {ex.Message}");
                return new PaymentResponse
                {
                    Success = false,
                    Message = ex.Message,
                    MessageAr = "حدث خطأ أثناء إنشاء الفاتورة"
                };
            }
        }


        public async Task<PaymentStatusResponse> VerifyPaymentAsync(string invoiceId)
        {
            try
            {
                var payload = new
                {
                    Key = invoiceId,
                    KeyType = "InvoiceId"
                };

                var response = await _httpClient.PostAsJsonAsync("/v2/GetPaymentStatus", payload);



                var result = await response.Content.ReadFromJsonAsync<MyFatoorahStatusResponse>();
                if (result?.IsSuccess == true && result.Data != null)
                {
                    var isPaid = result.Data.InvoiceStatus == "Paid";

                   

                    // ✅ جيب الـ PaymentId من أول Transaction ناجحة
                    var externalPaymentId = result.Data.InvoiceTransactions?
                        .FirstOrDefault(t => t.TransactionStatus == "Succss")?.PaymentId;

                  

                    // تحديث سجل الدفع
                    var payment = await _context.Payments
                        .FirstOrDefaultAsync(p => p.InvoiceId == invoiceId);

                    if (payment != null)
                    {
                        payment.Status = isPaid ? "Paid" : result.Data.InvoiceStatus ?? "Pending";
                        payment.PaymentId_External = externalPaymentId;
                        payment.UpdatedAt = DateTime.UtcNow;

                        if (isPaid)
                            payment.PaidAt = DateTime.UtcNow;

                        await _context.SaveChangesAsync();
                    }

                    return new PaymentStatusResponse
                    {
                        Success = true,
                        IsPaid = isPaid,
                        Status = result.Data.InvoiceStatus,
                        PaymentIdExternal = externalPaymentId,
                        Message = isPaid ? "Payment successful" : "Payment not completed",
                       
                    };
                }

                return new PaymentStatusResponse
                {
                    Success = false,
                    IsPaid = false,
                    Message = result?.Message ?? "Failed to verify payment"
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"VerifyPayment Error: {ex.Message}");
                return new PaymentStatusResponse
                {
                    Success = false,
                    IsPaid = false,
                    Message = ex.Message
                };
            }
        }

      

        public async Task<PaymentStatusResponse> VerifyPaymentByExternalPaymentIdAsync(string paymentIdExternal)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(paymentIdExternal))
                {
                    return new PaymentStatusResponse
                    {
                        Success = false,
                        IsPaid = false,
                        Message = "paymentId is required"  
                    };
                }

                var payload = new
                {
                    Key = paymentIdExternal,
                    KeyType = "PaymentId"
                };

                var response = await _httpClient.PostAsJsonAsync("/v2/GetPaymentStatus", payload);
                var result = await response.Content.ReadFromJsonAsync<MyFatoorahStatusResponse>();

                if (result?.IsSuccess == true && result.Data != null)
                {
                    var isPaid = result.Data.InvoiceStatus == "Paid";

                    // ✅ نعتمد على PaymentId_External فقط
                    var payment = await _context.Payments
                        .FirstOrDefaultAsync(p => p.PaymentId_External == paymentIdExternal);

                    if (payment != null)
                    {
                        payment.Status = isPaid ? "Paid" : result.Data.InvoiceStatus ?? "Pending";
                        payment.UpdatedAt = DateTime.UtcNow;

                        if (isPaid && payment.PaidAt == null)
                            payment.PaidAt = DateTime.UtcNow;

                        await _context.SaveChangesAsync();
                    }

                    return new PaymentStatusResponse
                    {
                        Success = true,
                        IsPaid = isPaid,
                        Status = result.Data.InvoiceStatus,
                        Message = isPaid ? "Payment successful" : "Payment not completed"
                    };
                }

                return new PaymentStatusResponse
                {
                    Success = false,
                    IsPaid = false,
                    Message = result?.Message ?? "Failed to verify payment"
                };
            }
            catch (Exception ex)
            {
                return new PaymentStatusResponse
                {
                    Success = false,
                    IsPaid = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<bool> ProcessWebhookAsync(MyFatoorahWebhookRequest request)
        {
            try
            {
                var invoiceId = request.InvoiceId.ToString();

                Console.WriteLine($"Processing Webhook for InvoiceId: {invoiceId}");

                // 1. التحقق من MyFatoorah مباشرة
                var verifyResult = await VerifyPaymentAsync(invoiceId);

                if (!verifyResult.IsPaid)
                {
                    Console.WriteLine("Payment not paid yet");
                    return false;
                }

                // 2. جيب الـ Payment من الداتابيز
                var payment = await _context.Payments
                    .FirstOrDefaultAsync(p => p.InvoiceId == invoiceId);

                if (payment == null)
                {
                    Console.WriteLine("Payment not found in database");
                    return false;
                }

                Console.WriteLine($"Payment found: Id={payment.PaymentId}, SubscriptionId={payment.SubscriptionId}, Status={payment.Status}");

                // 3. حدّث الـ Payment لو مش مدفوع
                if (payment.Status != "Paid")
                {
                    payment.Status = "Paid";
                    payment.PaidAt = DateTime.UtcNow;
                    payment.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }

                // 4. فعّل الاشتراك لو موجود (بغض النظر عن حالة الـ Payment)
                if (payment.SubscriptionId.HasValue)
                {
                    Console.WriteLine($"Activating subscription: {payment.SubscriptionId.Value}");
                    await ActivateSubscriptionAsync(payment.SubscriptionId.Value, payment.PaymentId_External, payment.Amount);
                }
                else
                {
                    Console.WriteLine("No SubscriptionId linked to this payment");
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ProcessWebhook Error: {ex.Message}");
                return false;
            }
        }

        private async Task ActivateSubscriptionAsync(long subscriptionId, string? paymentReference, decimal amount)
        {
            var subscription = await _context.Subscriptions
                .Include(s => s.Plan)
                .FirstOrDefaultAsync(s => s.Id == subscriptionId);

            if (subscription == null)
                return;

            // لو الاشتراك مفعّل قبل كده
            if (subscription.Status == "active")
                return;

            // حفظ الحالة القديمة للـ History
            var oldStatus = subscription.Status;

            // تفعيل الاشتراك
            subscription.Status = "active";
            subscription.StartsAt = DateOnly.FromDateTime(DateTime.UtcNow);
            subscription.EndsAt = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(subscription.Plan.DurationDays));
            subscription.AmountPaid = amount;
            subscription.PaymentReference = paymentReference;
            subscription.PaymentMethod = "MyFatoorah";
            subscription.UpdatedAt = DateTime.UtcNow;

            // إضافة سجل في الـ History
            var history = new SubscriptionHistory
            {
                SubscriptionId = subscriptionId,
                SupplierId = subscription.SupplierId,
                Action = "activated",
                OldStatus = oldStatus,
                NewStatus = "active",
                Amount = amount,
                Notes = $"Activated via MyFatoorah payment. Reference: {paymentReference}",
                CreatedAt = DateTime.UtcNow
            };

            _context.SubscriptionHistories.Add(history);
            await _context.SaveChangesAsync();

            // ✅ إرسال إشعار تفعيل الاشتراك
            await SendPaymentNotificationAsync(
                subscription.SupplierId,
                "supplier",
                "subscription_activated",
                subscription.Plan.NameAr,
                amount
                );
        }


        public async Task<bool> ProcessCallbackAsync(string paymentIdExternal, string status)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(paymentIdExternal))
                    return false;

                var payment = await _context.Payments
                    .Include(p => p.Subscription)
                        .ThenInclude(s => s.Plan)
                    .FirstOrDefaultAsync(p => p.PaymentId_External == paymentIdExternal);

                if (payment == null)
                    return false;

                payment.Status = status;
                payment.UpdatedAt = DateTime.UtcNow;

                if (status == "Paid")
                {
                    payment.PaidAt = DateTime.UtcNow;
                }
                else if (status == "Failed")
                {
                    payment.FailedAt = DateTime.UtcNow;

                    // ✅ إشعار الدفع الفاشل
                    if (payment.Subscription?.Plan != null)
                    {
                        await SendPaymentNotificationAsync(
                            payment.UserId,
                            payment.UserType,
                            "payment_failed",
                            payment.Subscription.Plan.NameAr,
                            payment.Amount
                        );
                    }
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task SendPaymentNotificationAsync(long userId, string userType, string type, string planName, decimal amount)
        {
            try
            {
                var notification = new CreateNotificationDto
                {
                    UserId = userId,
                    UserType = userType,
                    Type = type,
                    Priority = "high",
                    SendPush = true,
                    Data = new Dictionary<string, object>
            {
                { "planName", planName },
                { "amount", amount }
            }
                };

                switch (type)
                {
                    case "payment_success":
                        notification.Title = "Payment Successful";
                        notification.TitleAr = "تم الدفع بنجاح";
                        notification.Body = $"Your payment of {amount} for {planName} was successful.";
                        notification.BodyAr = $"تم دفع {amount} لـ {planName} بنجاح.";
                        break;

                    case "payment_failed":
                        notification.Title = "Payment Failed";
                        notification.TitleAr = "فشل الدفع";
                        notification.Body = $"Your payment for {planName} has failed. Please try again.";
                        notification.BodyAr = $"فشل الدفع لـ {planName}. يرجى المحاولة مرة أخرى.";
                        break;

                    case "subscription_activated":
                        notification.Title = "Subscription Activated";
                        notification.TitleAr = "تم تفعيل الاشتراك";
                        notification.Body = $"Your {planName} subscription is now active!";
                        notification.BodyAr = $"تم تفعيل اشتراكك في {planName}!";
                        break;
                }

                await _notificationService.CreateNotificationAsync(notification);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SendPaymentNotification Error: {ex.Message}");
            }
        }

        public async Task<PaymentHistoryResponse> GetPaymentHistoryAsync(long userId, string userType, int page = 1, int pageSize = 20)
        {
            try
            {
                var query = _context.Payments
                    .Where(p => p.UserId == userId && p.UserType == userType)
                    .OrderByDescending(p => p.CreatedAt);

                var totalCount = await query.CountAsync();

                var payments = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(p => new PaymentHistoryDto
                    {
                        PaymentId = p.PaymentId,
                        InvoiceId = p.InvoiceId,
                        Amount = p.Amount,
                        Currency = p.Currency,
                        Status = p.Status,
                        StatusAr = GetStatusArabic(p.Status),
                        PlanName = p.SubscriptionId.HasValue
                            ? _context.Subscriptions
                                .Where(s => s.Id == p.SubscriptionId)
                                .Select(s => s.Plan.NameAr)
                                .FirstOrDefault()
                            : null,
                        PlanNameEn = p.SubscriptionId.HasValue
                            ? _context.Subscriptions
                                .Where(s => s.Id == p.SubscriptionId)
                                .Select(s => s.Plan.NameEn)
                                .FirstOrDefault()
                            : null,
                        PaymentMethod = p.Provider,
                        PaidAt = p.PaidAt,
                        CreatedAt = p.CreatedAt,
                        PaymentUrl = p.PaymentUrl,
                        FailureReason = p.FailureReason
                    })
                    .ToListAsync();

                return new PaymentHistoryResponse
                {
                    Success = true,
                    Payments = payments,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                return new PaymentHistoryResponse
                {
                    Success = false,
                    Message = ex.Message,
                    MessageAr = "حدث خطأ أثناء جلب سجل المدفوعات"
                };
            }
        }

        public async Task<AdminPaymentHistoryResponse> GetAllPaymentsAsync(int page = 1, int pageSize = 20, string? status = null)
        {
            try
            {
                var query = _context.Payments.AsQueryable();

                // فلتر بالحالة
                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(p => p.Status == status);
                }

                var totalCount = await query.CountAsync();

                var payments = await query
                    .OrderByDescending(p => p.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(p => new AdminPaymentHistoryDto
                    {
                        PaymentId = p.PaymentId,
                        InvoiceId = p.InvoiceId,
                        Amount = p.Amount,
                        Currency = p.Currency,
                        Status = p.Status,
                        StatusAr = GetStatusArabic(p.Status),
                        SupplierId = p.UserId,
                        SupplierName = _context.SupplierProfiles
                            .Where(s => s.Id == p.UserId)
                            .Select(s => s.FullName)
                            .FirstOrDefault(),
                        SupplierPhone = _context.SupplierProfiles
                            .Where(s => s.Id == p.UserId)
                            .Select(s => s.Phone)
                            .FirstOrDefault(),
                        PlanName = p.SubscriptionId.HasValue
                            ? _context.Subscriptions
                                .Where(s => s.Id == p.SubscriptionId)
                                .Select(s => s.Plan.NameAr)
                                .FirstOrDefault()
                            : null,
                        PlanNameEn = p.SubscriptionId.HasValue
                            ? _context.Subscriptions
                                .Where(s => s.Id == p.SubscriptionId)
                                .Select(s => s.Plan.NameEn)
                                .FirstOrDefault()
                            : null,
                        PaymentMethod = p.Provider,
                        PaidAt = p.PaidAt,
                        CreatedAt = p.CreatedAt,
                        FailureReason = p.FailureReason
                    })
                    .ToListAsync();

                return new AdminPaymentHistoryResponse
                {
                    Success = true,
                    Payments = payments,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                return new AdminPaymentHistoryResponse
                {
                    Success = false,
                    Message = ex.Message,
                    MessageAr = "حدث خطأ أثناء جلب سجل المدفوعات"
                };
            }
        }

        public async Task<AdminPaymentHistoryResponse> GetSupplierPaymentsAsync(long supplierId, int page = 1, int pageSize = 20)
        {
            try
            {
                var query = _context.Payments
                    .Where(p => p.UserId == supplierId && p.UserType == "supplier");

                var totalCount = await query.CountAsync();

                var payments = await query
                    .OrderByDescending(p => p.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(p => new AdminPaymentHistoryDto
                    {
                        PaymentId = p.PaymentId,
                        InvoiceId = p.InvoiceId,
                        Amount = p.Amount,
                        Currency = p.Currency,
                        Status = p.Status,
                        StatusAr = GetStatusArabic(p.Status),
                        SupplierId = p.UserId,
                        SupplierName = _context.SupplierProfiles
                            .Where(s => s.Id == p.UserId)
                            .Select(s => s.FullName)
                            .FirstOrDefault(),
                        SupplierPhone = _context.SupplierProfiles
                            .Where(s => s.Id == p.UserId)
                            .Select(s => s.Phone)
                            .FirstOrDefault(),
                        PlanName = p.SubscriptionId.HasValue
                            ? _context.Subscriptions
                                .Where(s => s.Id == p.SubscriptionId)
                                .Select(s => s.Plan.NameAr)
                                .FirstOrDefault()
                            : null,
                        PlanNameEn = p.SubscriptionId.HasValue
                            ? _context.Subscriptions
                                .Where(s => s.Id == p.SubscriptionId)
                                .Select(s => s.Plan.NameEn)
                                .FirstOrDefault()
                            : null,
                        PaymentMethod = p.Provider,
                        PaidAt = p.PaidAt,
                        CreatedAt = p.CreatedAt,
                        FailureReason = p.FailureReason
                    })
                    .ToListAsync();

                return new AdminPaymentHistoryResponse
                {
                    Success = true,
                    Payments = payments,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                return new AdminPaymentHistoryResponse
                {
                    Success = false,
                    Message = ex.Message,
                    MessageAr = "حدث خطأ أثناء جلب سجل المدفوعات"
                };
            }
        }

        private static string GetStatusArabic(string? status)
        {
            return status switch
            {
                "Paid" => "مدفوع",
                "Pending" => "قيد الانتظار",
                "Failed" => "فشل",
                "Cancelled" => "ملغي",
                _ => status ?? ""
            };
        }

    }
}