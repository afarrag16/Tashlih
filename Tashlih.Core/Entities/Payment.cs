using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tashlih.Core.Entities
{
    public class Payment
    {
        public long PaymentId { get; set; }
        public long UserId { get; set; }
        public string UserType { get; set; } = null!;
        public long? SubscriptionId { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "SAR";
        public string Provider { get; set; } = "MyFatoorah";
        public string? InvoiceId { get; set; }
        public string? PaymentId_External { get; set; }
        public string? PaymentUrl { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime? PaidAt { get; set; }
        public DateTime? FailedAt { get; set; }
        public string? FailureReason { get; set; }
        public string? CallbackData { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Subscription? Subscription { get; set; }
    }
}
