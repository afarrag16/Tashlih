using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Tashlih.Application.DTOs;
using Tashlih.Application.Interfaces;

namespace Tashlih.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        /// <summary>
        /// إنشاء فاتورة دفع جديدة
        /// </summary>
        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentRequest request)
        {
            var userId = GetCurrentUserId();
            var userType = GetCurrentUserType();

            if (userId == 0)
                return Unauthorized(new { Success = false, Message = "Unauthorized", MessageAr = "غير مصرح" });

            // بوابة الدفع للموردين فقط
            if (!string.Equals(userType, "supplier", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new PaymentResponse
                {
                    Success = false,
                    Message = "Payment is only available for suppliers",
                    MessageAr = "بوابة الدفع متاحة للموردين فقط"
                });
            }

            var result = await _paymentService.CreatePaymentAsync(userId, userType, request);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// التحقق من حالة الدفع
        /// </summary>
        [HttpGet("verify/{invoiceId}")]
        public async Task<IActionResult> VerifyPayment(string invoiceId)
        {
            var result = await _paymentService.VerifyPaymentAsync(invoiceId);
            return Ok(result);
        }

        /// <summary>
        /// Callback من MyFatoorah بعد الدفع الناجح
        /// </summary>
        [HttpGet("callback")]
        [AllowAnonymous]  // ✅ ضيف ده عشان MyFatoorah يقدر يوصله
        public async Task<IActionResult> PaymentCallback([FromQuery] string paymentId)
        {
            if (string.IsNullOrEmpty(paymentId))
                return BadRequest(new { success = false, message = "Payment ID is required" });

            // ✅ استخدم الـ method الصح - بالـ External PaymentId
            var result = await _paymentService.VerifyPaymentByExternalPaymentIdAsync(paymentId);

            if (result.IsPaid)
            {
                // ✅ رجّع JSON بدل Redirect (أسهل للتست)
                return Ok(new
                {
                    success = true,
                    message = "Payment successful",
                    messageAr = "تم الدفع بنجاح",
                    paymentId = paymentId,
                    status = result.Status
                });

                // أو لو عايز Redirect:
                // return Redirect($"/payment/success?paymentId={paymentId}");
            }

            return Ok(new
            {
                success = false,
                message = "Payment not completed",
                messageAr = "لم يتم إكمال الدفع",
                paymentId = paymentId,
                status = result.Status
            });
        }




        /// <summary>
        /// صفحة الخطأ من MyFatoorah
        /// </summary>
        [HttpGet("error")]
        [AllowAnonymous]
        public async Task<IActionResult> PaymentError([FromQuery] string? paymentId)
        {
            if (!string.IsNullOrEmpty(paymentId))
            {
                var result = await _paymentService.VerifyPaymentByExternalPaymentIdAsync(paymentId);

                // ✅ لو الدفع ناجح فعلاً
                if (result.IsPaid)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "Payment was actually successful",
                        messageAr = "الدفع تم بنجاح",
                        paymentId = paymentId,
                        status = result.Status
                    });
                }

                // الدفع فشل
                return Ok(new
                {
                    success = false,
                    message = "Payment failed or cancelled",
                    messageAr = "فشل الدفع أو تم إلغاؤه",
                    paymentId = paymentId,
                    status = result.Status
                });
            }

            return Ok(new
            {
                success = false,
                message = "Payment cancelled by user",
                messageAr = "تم إلغاء الدفع من قبل المستخدم"
            });
        }

        /// <summary>
        /// Webhook من MyFatoorah (Server-to-Server)
        /// </summary>
        [HttpPost("webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> PaymentWebhook([FromBody] MyFatoorahWebhookRequest request)
        {
            try
            {
                Console.WriteLine($"===== Webhook Received =====");
                Console.WriteLine($"InvoiceId: {request.InvoiceId}");
                Console.WriteLine($"InvoiceStatus: {request.InvoiceStatus}");
                Console.WriteLine($"============================");

                if (request.InvoiceId <= 0)
                    return BadRequest(new { success = false, message = "Invalid InvoiceId" });

                // التحقق من الدفع وتفعيل الاشتراك
                var result = await _paymentService.ProcessWebhookAsync(request);

                if (result)
                {
                    return Ok(new { success = true, message = "Webhook processed successfully" });
                }

                return Ok(new { success = false, message = "Payment not completed" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Webhook Error: {ex.Message}");
                return Ok(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// سجل المدفوعات للمورد
        /// </summary>
        [Authorize]
        [HttpGet("history")]
        public async Task<IActionResult> GetPaymentHistory([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var userId = GetCurrentUserId();
            var userType = GetCurrentUserType();

            if (userId == 0)
                return Unauthorized(new { Success = false, Message = "Unauthorized", MessageAr = "غير مصرح" });

            var result = await _paymentService.GetPaymentHistoryAsync(userId, userType, page, pageSize);
            return Ok(result);
        }



        #region Helper Methods

        private long GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return long.TryParse(userIdClaim, out var id) ? id : 0;
        }

        private string GetCurrentUserType()
        {
            return User.FindFirst("user_type")?.Value ?? "customer";
        }

        #endregion
    }
}
