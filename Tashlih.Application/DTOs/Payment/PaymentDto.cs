using System.Text.Json.Serialization;

namespace Tashlih.Application.DTOs
{
    public class CreatePaymentRequest
    {
        public decimal Amount { get; set; }
        public long? PlanId { get; set; }
        public string? CustomerName { get; set; }
        public string? MobileCountryCode { get; set; }
        public string? CustomerPhone { get; set; }
        public string? CustomerEmail { get; set; }
    }

    public class PaymentResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? MessageAr { get; set; }
        public string? PaymentUrl { get; set; }
        public string? InvoiceId { get; set; }
        public long? PaymentId { get; set; }
    }

    public class PaymentStatusResponse
    {
        public bool Success { get; set; }
        public bool IsPaid { get; set; }
        public string? Status { get; set; }
        public string? Message { get; set; }
        public string? PaymentIdExternal { get; set; }
       
    }

    // MyFatoorah Response Models
    public class MyFatoorahResponse
    {
        public bool IsSuccess { get; set; }
        public string? Message { get; set; }
        public MyFatoorahData? Data { get; set; }
    }

    public class MyFatoorahData
    {
        public int InvoiceId { get; set; }
        public string? InvoiceURL { get; set; }
    }

    public class MyFatoorahStatusResponse
    {
        public bool IsSuccess { get; set; }
        public string? Message { get; set; }
        public MyFatoorahStatusData? Data { get; set; }
    }

    public class MyFatoorahStatusData
    {
        public string? InvoiceStatus { get; set; }
        public string? PaymentId { get; set; }
        public List<InvoiceTransaction>? InvoiceTransactions { get; set; }

    }
  

public class InvoiceTransaction
    {
        public string? PaymentId { get; set; }
        public string? TransactionStatus { get; set; }
        public string? PaymentGateway { get; set; }

        [JsonPropertyName("PaidCurrency")]
        public string? PaidCurrency { get; set; }

        [JsonPropertyName("PaidCurrencyValue")]
        public string? PaidCurrencyValue { get; set; }
    }

    public class MyFatoorahWebhookRequest
    {
        public long InvoiceId { get; set; }
        public string? InvoiceStatus { get; set; }
        public string? InvoiceReference { get; set; }
        public string? CustomerReference { get; set; }
        public decimal? InvoiceValue { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerEmail { get; set; }
        public string? CustomerMobile { get; set; }
        public string? PaymentMethod { get; set; }
        public string? UserDefinedField { get; set; }
    }

    public class PaymentHistoryResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? MessageAr { get; set; }
        public List<PaymentHistoryDto> Payments { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    public class PaymentHistoryDto
    {
        public long PaymentId { get; set; }
        public string? InvoiceId { get; set; }
        public decimal Amount { get; set; }
        public string? Currency { get; set; }
        public string? Status { get; set; }
        public string? StatusAr { get; set; }
        public string? PlanName { get; set; }
        public string? PlanNameEn { get; set; }
        public string? PaymentMethod { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? PaymentUrl { get; set; }
        public string? FailureReason { get; set; }
    }

    public class AdminPaymentHistoryResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? MessageAr { get; set; }
        public List<AdminPaymentHistoryDto> Payments { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    public class AdminPaymentHistoryDto
    {
        public long PaymentId { get; set; }
        public string? InvoiceId { get; set; }
        public decimal Amount { get; set; }
        public string? Currency { get; set; }
        public string? Status { get; set; }
        public string? StatusAr { get; set; }

        // بيانات المورد
        public long SupplierId { get; set; }
        public string? SupplierName { get; set; }
        public string? SupplierPhone { get; set; }

        // بيانات الباقة
        public string? PlanName { get; set; }
        public string? PlanNameEn { get; set; }

        public string? PaymentMethod { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? FailureReason { get; set; }
    }
}