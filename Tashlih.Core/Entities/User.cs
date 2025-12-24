using System;
using System.Collections.Generic;

namespace Tashlih.Core.Entities;


public partial class User
{
    public long Id { get; set; }

    public string FullName { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string? Email { get; set; }

    public string PasswordHash { get; set; } = null!;

    public string? AvatarUrl { get; set; }

    public string UserType { get; set; } = null!;

    public string Status { get; set; } = null!;
    public bool IsPhoneVerified { get; set; }
    public string PreferredLanguage { get; set; } = null!;

    public bool NotificationsEnabled { get; set; }
    public string? OtpCode { get; set; }
    public DateTime? OtpExpiresAt { get; set; }
    public string? Street { get; set; }
    public int? CityId { get; set; }
    public string? PostalCode { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }

    public virtual City? City { get; set; }


    public DateTime? LastLoginAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();

    public virtual ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();

    public virtual ICollection<ChatThread> ChatThreads { get; set; } = new List<ChatThread>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Review> ReviewCustomers { get; set; } = new List<Review>();

    public virtual ICollection<Review> ReviewModeratedByNavigations { get; set; } = new List<Review>();

    public virtual ICollection<SubscriptionHistory> SubscriptionHistories { get; set; } = new List<SubscriptionHistory>();

    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();

    public virtual SupplierProfile? SupplierProfileUser { get; set; }

    public virtual ICollection<SupplierProfile> SupplierProfileVerifiedByNavigations { get; set; } = new List<SupplierProfile>();

    

    public virtual ICollection<UserSession> UserSessions { get; set; } = new List<UserSession>();
}
