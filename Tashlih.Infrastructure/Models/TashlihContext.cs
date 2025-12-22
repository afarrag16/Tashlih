using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Tashlih.Core.Entities;


namespace Tashlih.Infrastructure.Models;

public partial class TashlihContext : DbContext
{
    public TashlihContext()
    {
    }

    public TashlihContext(DbContextOptions<TashlihContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Attachment> Attachments { get; set; }

    public virtual DbSet<ChatAttachment> ChatAttachments { get; set; }

    public virtual DbSet<ChatMessage> ChatMessages { get; set; }

    public virtual DbSet<ChatThread> ChatThreads { get; set; }

    public virtual DbSet<Log> Logs { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }

    public virtual DbSet<Part> Parts { get; set; }

    public virtual DbSet<PartCategory> PartCategories { get; set; }

    public virtual DbSet<PartCategoryMapping> PartCategoryMappings { get; set; }

    public virtual DbSet<PartImage> PartImages { get; set; }

    public virtual DbSet<Review> Reviews { get; set; }

   

    public virtual DbSet<Subscription> Subscriptions { get; set; }

    public virtual DbSet<SubscriptionHistory> SubscriptionHistories { get; set; }

    public virtual DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }

    public virtual DbSet<SupplierProfile> SupplierProfiles { get; set; }

    public virtual DbSet<SupplierSession> SupplierSessions { get; set; } 

    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<City> Cities { get; set; }



    public virtual DbSet<UserSession> UserSessions { get; set; }

    public virtual DbSet<VActiveSupplier> VActiveSuppliers { get; set; }

    public virtual DbSet<VOrdersDetailed> VOrdersDetaileds { get; set; }

    public virtual DbSet<VPartsDetailed> VPartsDetaileds { get; set; }

    public virtual DbSet<VehicleMake> VehicleMakes { get; set; }

    public virtual DbSet<VehicleModel> VehicleModels { get; set; }

    public virtual DbSet<VehicleSubcategory> VehicleSubcategories { get; set; }

    public virtual DbSet<VehicleType> VehicleTypes { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Server=.;Database=Tashlih;Trusted_Connection=True;TrustServerCertificate=True;");
    //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //    => optionsBuilder.UseSqlServer("Server=localhost;Database=Tashlih;User Id=sa;Password=Rekj@10170;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Attachment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__attachme__3213E83F8B5EF20A");

            entity.ToTable("attachments");

            entity.HasIndex(e => new { e.AttachableType, e.AttachableId }, "idx_attachments_type");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AttachableId).HasColumnName("attachable_id");
            entity.Property(e => e.AttachableType)
                .HasMaxLength(50)
                .HasColumnName("attachable_type");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.FileName)
                .HasMaxLength(255)
                .HasColumnName("file_name");
            entity.Property(e => e.FilePath)
                .HasMaxLength(500)
                .HasColumnName("file_path");
            entity.Property(e => e.FileSize).HasColumnName("file_size");
            entity.Property(e => e.FileType)
                .HasMaxLength(50)
                .HasColumnName("file_type");
            entity.Property(e => e.FileUrl)
                .HasMaxLength(500)
                .HasColumnName("file_url");
            entity.Property(e => e.MimeType)
                .HasMaxLength(100)
                .HasColumnName("mime_type");
            entity.Property(e => e.Purpose)
                .HasMaxLength(50)
                .HasColumnName("purpose");
            entity.Property(e => e.ThumbnailUrl)
                .HasMaxLength(500)
                .HasColumnName("thumbnail_url");
            entity.Property(e => e.UploadedBy).HasColumnName("uploaded_by");
            entity.Property(e => e.Visibility)
                .HasMaxLength(20)
                .HasDefaultValue("private")
                .HasColumnName("visibility");

            entity.HasOne(d => d.UploadedByNavigation).WithMany(p => p.Attachments)
                .HasForeignKey(d => d.UploadedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_attachments_uploader");
        });

        modelBuilder.Entity<ChatAttachment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__chat_att__3213E83F742633AB");

            entity.ToTable("chat_attachments");

            entity.HasIndex(e => e.MessageId, "idx_chat_attach_msg");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.Duration).HasColumnName("duration");
            entity.Property(e => e.FileName)
                .HasMaxLength(255)
                .HasColumnName("file_name");
            entity.Property(e => e.FileSize).HasColumnName("file_size");
            entity.Property(e => e.FileType)
                .HasMaxLength(20)
                .HasColumnName("file_type");
            entity.Property(e => e.FileUrl)
                .HasMaxLength(500)
                .HasColumnName("file_url");
            entity.Property(e => e.Height).HasColumnName("height");
            entity.Property(e => e.MessageId).HasColumnName("message_id");
            entity.Property(e => e.MimeType)
                .HasMaxLength(100)
                .HasColumnName("mime_type");
            entity.Property(e => e.ThumbnailUrl)
                .HasMaxLength(500)
                .HasColumnName("thumbnail_url");
            entity.Property(e => e.Width).HasColumnName("width");

            entity.HasOne(d => d.Message).WithMany(p => p.ChatAttachments)
                .HasForeignKey(d => d.MessageId)
                .HasConstraintName("FK_chat_attach_msg");
        });

        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__chat_mes__3213E83F0E3694C5");

            entity.ToTable("chat_messages", tb => tb.HasTrigger("trg_message_insert"));

            entity.HasIndex(e => e.ThreadId, "idx_messages_thread");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            entity.Property(e => e.IsRead).HasColumnName("is_read");
            entity.Property(e => e.MessageType)
                .HasMaxLength(20)
                .HasDefaultValue("text")
                .HasColumnName("message_type");
            entity.Property(e => e.Metadata).HasColumnName("metadata");
            entity.Property(e => e.ReadAt).HasColumnName("read_at");
            entity.Property(e => e.SenderId).HasColumnName("sender_id");
            entity.Property(e => e.SenderType)
                .HasMaxLength(20)
                .HasColumnName("sender_type");
            entity.Property(e => e.ThreadId).HasColumnName("thread_id");

            entity.HasOne(d => d.Sender).WithMany(p => p.ChatMessages)
                .HasForeignKey(d => d.SenderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_messages_sender");

            entity.HasOne(d => d.Thread).WithMany(p => p.ChatMessages)
                .HasForeignKey(d => d.ThreadId)
                .HasConstraintName("FK_messages_thread");
        });

        modelBuilder.Entity<ChatThread>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__chat_thr__3213E83FD78A5EF4");

            entity.ToTable("chat_threads");

            entity.HasIndex(e => e.CustomerId, "idx_threads_customer");

            entity.HasIndex(e => e.SupplierId, "idx_threads_supplier");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.CustomerUnreadCount).HasColumnName("customer_unread_count");
            entity.Property(e => e.LastMessage).HasColumnName("last_message");
            entity.Property(e => e.LastMessageAt).HasColumnName("last_message_at");
            entity.Property(e => e.LastMessageBy).HasColumnName("last_message_by");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.PartId).HasColumnName("part_id");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("active")
                .HasColumnName("status");
            entity.Property(e => e.SupplierId).HasColumnName("supplier_id");
            entity.Property(e => e.SupplierUnreadCount).HasColumnName("supplier_unread_count");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Customer).WithMany(p => p.ChatThreads)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("FK_threads_customer");

            entity.HasOne(d => d.Order).WithMany(p => p.ChatThreads)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK_threads_order");

            entity.HasOne(d => d.Part).WithMany(p => p.ChatThreads)
                .HasForeignKey(d => d.PartId)
                .HasConstraintName("FK_threads_part");

            entity.HasOne(d => d.Supplier).WithMany(p => p.ChatThreads)
                .HasForeignKey(d => d.SupplierId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_threads_supplier");
        });

        modelBuilder.Entity<Log>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__logs__3213E83F99C1B5FB");

            entity.ToTable("logs");

            entity.HasIndex(e => e.Action, "idx_logs_action");

            entity.HasIndex(e => e.CreatedAt, "idx_logs_created").IsDescending();

            entity.HasIndex(e => e.UserId, "idx_logs_user");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Action)
                .HasMaxLength(50)
                .HasColumnName("action");
            entity.Property(e => e.Context).HasColumnName("context");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.EntityId).HasColumnName("entity_id");
            entity.Property(e => e.EntityType)
                .HasMaxLength(50)
                .HasColumnName("entity_type");
            entity.Property(e => e.IpAddress)
                .HasMaxLength(45)
                .HasColumnName("ip_address");
            entity.Property(e => e.NewValues).HasColumnName("new_values");
            entity.Property(e => e.OldValues).HasColumnName("old_values");
            entity.Property(e => e.UserAgent).HasColumnName("user_agent");
            entity.Property(e => e.UserId).HasColumnName("user_id");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__notifica__3213E83FF5BE0461");

            entity.ToTable("notifications");

            entity.HasIndex(e => new { e.UserId, e.IsRead }, "idx_notifications_read");

            entity.HasIndex(e => e.UserId, "idx_notifications_user");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Body).HasColumnName("body");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.Data).HasColumnName("data");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(500)
                .HasColumnName("image_url");
            entity.Property(e => e.IsPushSent).HasColumnName("is_push_sent");
            entity.Property(e => e.IsRead).HasColumnName("is_read");
            entity.Property(e => e.Priority)
                .HasMaxLength(20)
                .HasDefaultValue("normal")
                .HasColumnName("priority");
            entity.Property(e => e.PushSentAt).HasColumnName("push_sent_at");
            entity.Property(e => e.ReadAt).HasColumnName("read_at");
            entity.Property(e => e.Title)
                .HasMaxLength(200)
                .HasColumnName("title");
            entity.Property(e => e.Type)
                .HasMaxLength(50)
                .HasColumnName("type");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_notifications_user");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__orders__3213E83FDF81CD1E");

            entity.ToTable("orders", tb => tb.HasTrigger("trg_order_completed"));

            entity.HasIndex(e => e.OrderNumber, "UQ_orders_number").IsUnique();

            entity.HasIndex(e => e.CustomerId, "idx_orders_customer");

            entity.HasIndex(e => e.Status, "idx_orders_status");

            entity.HasIndex(e => e.SupplierId, "idx_orders_supplier");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CancelReason).HasColumnName("cancel_reason");
            entity.Property(e => e.CancelledAt).HasColumnName("cancelled_at");
            entity.Property(e => e.CancelledBy)
                .HasMaxLength(20)
                .HasColumnName("cancelled_by");
            entity.Property(e => e.CompletedAt).HasColumnName("completed_at");
            entity.Property(e => e.ConfirmedAt).HasColumnName("confirmed_at");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.Currency)
                .HasMaxLength(3)
                .HasDefaultValue("SAR")
                .HasColumnName("currency");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.CustomerNotes).HasColumnName("customer_notes");
            entity.Property(e => e.DiscountAmount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("discount_amount");
            entity.Property(e => e.InternalNotes).HasColumnName("internal_notes");
            entity.Property(e => e.IsReviewed).HasColumnName("is_reviewed");
            entity.Property(e => e.OrderNumber)
                .HasMaxLength(30)
                .HasColumnName("order_number");
            entity.Property(e => e.ProcessingAt).HasColumnName("processing_at");
            entity.Property(e => e.ReadyAt).HasColumnName("ready_at");
            entity.Property(e => e.ShopId).HasColumnName("shop_id");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("pending")
                .HasColumnName("status");
            entity.Property(e => e.Subtotal)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("subtotal");
            entity.Property(e => e.SupplierId).HasColumnName("supplier_id");
            entity.Property(e => e.SupplierNotes).HasColumnName("supplier_notes");
            entity.Property(e => e.TotalAmount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("total_amount");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Customer).WithMany(p => p.Orders)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_orders_customer");

          

            entity.HasOne(d => d.Supplier).WithMany(p => p.Orders)
                .HasForeignKey(d => d.SupplierId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_orders_supplier");
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__order_it__3213E83FA3D6A6A2");

            entity.ToTable("order_items");

            entity.HasIndex(e => e.OrderId, "idx_items_order");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ConditionSnapshot)
                .HasMaxLength(20)
                .HasColumnName("condition_snapshot");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.ImageUrlSnapshot)
                .HasMaxLength(500)
                .HasColumnName("image_url_snapshot");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.PartId).HasColumnName("part_id");
            entity.Property(e => e.PartNameSnapshot)
                .HasMaxLength(200)
                .HasColumnName("part_name_snapshot");
            entity.Property(e => e.PartNumberSnapshot)
                .HasMaxLength(50)
                .HasColumnName("part_number_snapshot");
            entity.Property(e => e.Quantity)
                .HasDefaultValue(1)
                .HasColumnName("quantity");
            entity.Property(e => e.TotalPrice)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("total_price");
            entity.Property(e => e.UnitPrice)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("unit_price");
            entity.Property(e => e.WarrantyDaysSnapshot).HasColumnName("warranty_days_snapshot");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK_items_order");

            entity.HasOne(d => d.Part).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.PartId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_items_part");
        });

        modelBuilder.Entity<Part>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__parts__3213E83F47584823");

            entity.ToTable("parts");

            entity.HasIndex(e => e.MakeId, "idx_parts_make");

            entity.HasIndex(e => e.ModelId, "idx_parts_model");

            entity.HasIndex(e => e.Price, "idx_parts_price");

            entity.HasIndex(e => e.SupplierId, "idx_parts_supplier");

            entity.HasIndex(e => e.Status, "idx_parts_status");

            entity.HasIndex(e => e.VehicleTypeId, "idx_parts_vehicle_type");

            entity.HasIndex(e => new { e.YearFrom, e.YearTo }, "idx_parts_year");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.Condition)
                .HasMaxLength(20)
                .HasDefaultValue("used")
                .HasColumnName("condition");
            entity.Property(e => e.ConditionDetails).HasColumnName("condition_details");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.Currency)
                .HasMaxLength(3)
                .HasDefaultValue("SAR")
                .HasColumnName("currency");
            entity.Property(e => e.CustomCategory)
                .HasMaxLength(100)
                .HasColumnName("custom_category");
            entity.Property(e => e.CustomMake)
                .HasMaxLength(100)
                .HasColumnName("custom_make");
            entity.Property(e => e.CustomModel)
                .HasMaxLength(100)
                .HasColumnName("custom_model");
            entity.Property(e => e.CustomSubcategory)
                .HasMaxLength(100)
                .HasColumnName("custom_subcategory");
            entity.Property(e => e.CustomVehicleType)
                .HasMaxLength(100)
                .HasColumnName("custom_vehicle_type");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.DeliveryAvailable).HasColumnName("delivery_available");
            entity.Property(e => e.DeliveryByShop).HasColumnName("delivery_by_shop");
            entity.Property(e => e.DeliveryNotes)
                .HasMaxLength(500)
                .HasColumnName("delivery_notes");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.FavoritesCount).HasColumnName("favorites_count");
            entity.Property(e => e.FeaturedUntil).HasColumnName("featured_until");
            entity.Property(e => e.IsFeatured).HasColumnName("is_featured");
            entity.Property(e => e.MakeId).HasColumnName("make_id");
            entity.Property(e => e.ModelId).HasColumnName("model_id");
            entity.Property(e => e.NameAr)
                .HasMaxLength(200)
                .HasColumnName("name_ar");
            entity.Property(e => e.NameEn)
                .HasMaxLength(200)
                .HasColumnName("name_en");
            entity.Property(e => e.OemNumber)
                .HasMaxLength(50)
                .HasColumnName("oem_number");
            entity.Property(e => e.OriginalPrice)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("original_price");
            entity.Property(e => e.PartNumber)
                .HasMaxLength(50)
                .HasColumnName("part_number");
            entity.Property(e => e.Price)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("price");
            entity.Property(e => e.Quantity)
                .HasDefaultValue(1)
                .HasColumnName("quantity");
            entity.Property(e => e.SalesCount).HasColumnName("sales_count");
            entity.Property(e => e.SupplierId).HasColumnName("supplier_id");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("available")
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("updated_at");
            entity.Property(e => e.VehicleSubcategoryId).HasColumnName("vehicle_subcategory_id");
            entity.Property(e => e.VehicleTypeId).HasColumnName("vehicle_type_id");
            entity.Property(e => e.ViewsCount).HasColumnName("views_count");
            entity.Property(e => e.VinNumber)
                .HasMaxLength(17)
                .HasColumnName("vin_number");
            entity.Property(e => e.WarrantyDays)
                .HasDefaultValue(0)
                .HasColumnName("warranty_days");
            entity.Property(e => e.WarrantyType)
                .HasMaxLength(20)
                .HasDefaultValue("none")
                .HasColumnName("warranty_type");
            entity.Property(e => e.YearFrom).HasColumnName("year_from");
            entity.Property(e => e.YearTo).HasColumnName("year_to");

            entity.HasOne(d => d.Category).WithMany(p => p.Parts)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_parts_category");

            entity.HasOne(d => d.Make).WithMany(p => p.Parts)
                .HasForeignKey(d => d.MakeId)
                .HasConstraintName("FK_parts_make");

            entity.HasOne(d => d.Model).WithMany(p => p.Parts)
                .HasForeignKey(d => d.ModelId)
                .HasConstraintName("FK_parts_model");

            entity.HasOne(d => d.Supplier).WithMany(p => p.Parts)
                .HasForeignKey(d => d.SupplierId)
                .HasConstraintName("fk_parts_supplier");

            entity.HasOne(d => d.VehicleSubcategory).WithMany(p => p.Parts)
                .HasForeignKey(d => d.VehicleSubcategoryId)
                .HasConstraintName("FK_parts_subcategory");

            entity.HasOne(d => d.VehicleType).WithMany(p => p.Parts)
                .HasForeignKey(d => d.VehicleTypeId)
                .HasConstraintName("FK_parts_vehicle_type");
        });

        modelBuilder.Entity<PartCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__part_cat__3213E83F1131DE08");

            entity.ToTable("part_categories");

            entity.HasIndex(e => e.IsActive, "idx_categories_active");

            entity.HasIndex(e => e.ParentId, "idx_categories_parent");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.DescriptionAr).HasColumnName("description_ar");
            entity.Property(e => e.Icon)
                .HasMaxLength(50)
                .HasColumnName("icon");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(500)
                .HasColumnName("image_url");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Level).HasColumnName("level");
            entity.Property(e => e.NameAr)
                .HasMaxLength(100)
                .HasColumnName("name_ar");
            entity.Property(e => e.NameEn)
                .HasMaxLength(100)
                .HasColumnName("name_en");
            entity.Property(e => e.ParentId).HasColumnName("parent_id");
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Parent).WithMany(p => p.InverseParent)
                .HasForeignKey(d => d.ParentId)
                .HasConstraintName("FK_categories_parent");
        });

        modelBuilder.Entity<PartCategoryMapping>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__part_cat__3213E83F80098EA2");

            entity.ToTable("part_category_mapping");

            entity.HasIndex(e => new { e.VehicleTypeId, e.CategoryId }, "UQ_mapping").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.VehicleTypeId).HasColumnName("vehicle_type_id");

            entity.HasOne(d => d.Category).WithMany(p => p.PartCategoryMappings)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK_mapping_category");

            entity.HasOne(d => d.VehicleType).WithMany(p => p.PartCategoryMappings)
                .HasForeignKey(d => d.VehicleTypeId)
                .HasConstraintName("FK_mapping_vehicle");
        });

        modelBuilder.Entity<PartImage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__part_ima__3213E83F7C51C65E");

            entity.ToTable("part_images");

            entity.HasIndex(e => e.PartId, "idx_images_part");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.DisplayOrder).HasColumnName("display_order");
            entity.Property(e => e.FileSize).HasColumnName("file_size");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(500)
                .HasColumnName("image_url");
            entity.Property(e => e.IsPrimary).HasColumnName("is_primary");
            entity.Property(e => e.MimeType)
                .HasMaxLength(50)
                .HasColumnName("mime_type");
            entity.Property(e => e.PartId).HasColumnName("part_id");
            entity.Property(e => e.ThumbnailUrl)
                .HasMaxLength(500)
                .HasColumnName("thumbnail_url");

            entity.HasOne(d => d.Part).WithMany(p => p.PartImages)
                .HasForeignKey(d => d.PartId)
                .HasConstraintName("FK_images_part");
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__reviews__3213E83F1DE91CCF");

            entity.ToTable("reviews", tb => tb.HasTrigger("trg_review_insert"));

            entity.HasIndex(e => e.OrderId, "UQ_reviews_order").IsUnique();

            entity.HasIndex(e => e.SupplierId, "idx_reviews_supplier");

            entity.HasIndex(e => e.IsVisible, "idx_reviews_visible");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Comment).HasColumnName("comment");
            entity.Property(e => e.CommunicationRating).HasColumnName("communication_rating");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.IsReported).HasColumnName("is_reported");
            entity.Property(e => e.IsVerified)
                .HasDefaultValue(true)
                .HasColumnName("is_verified");
            entity.Property(e => e.IsVisible)
                .HasDefaultValue(true)
                .HasColumnName("is_visible");
            entity.Property(e => e.ModeratedAt).HasColumnName("moderated_at");
            entity.Property(e => e.ModeratedBy).HasColumnName("moderated_by");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.OverallRating).HasColumnName("overall_rating");
            entity.Property(e => e.PriceRating).HasColumnName("price_rating");
            entity.Property(e => e.QualityRating).HasColumnName("quality_rating");
            entity.Property(e => e.ReportReason).HasColumnName("report_reason");
            entity.Property(e => e.SpeedRating).HasColumnName("speed_rating");
            entity.Property(e => e.SupplierId).HasColumnName("supplier_id");
            entity.Property(e => e.SupplierReply).HasColumnName("supplier_reply");
            entity.Property(e => e.SupplierReplyAt).HasColumnName("supplier_reply_at");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Customer).WithMany(p => p.ReviewCustomers)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_reviews_customer");

            entity.HasOne(d => d.ModeratedByNavigation).WithMany(p => p.ReviewModeratedByNavigations)
                .HasForeignKey(d => d.ModeratedBy)
                .HasConstraintName("FK_reviews_moderator");

            entity.HasOne(d => d.Order).WithOne(p => p.Review)
                .HasForeignKey<Review>(d => d.OrderId)
                .HasConstraintName("FK_reviews_order");

            entity.HasOne(d => d.Supplier).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.SupplierId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_reviews_supplier");
        });

       

        modelBuilder.Entity<Subscription>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__subscrip__3213E83F2E0A54E4");

            entity.ToTable("subscriptions");

            entity.HasIndex(e => e.Status, "idx_subs_status");

            entity.HasIndex(e => e.SupplierId, "idx_subs_supplier");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AmountPaid)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("amount_paid");
            entity.Property(e => e.ApprovedAt).HasColumnName("approved_at");
            entity.Property(e => e.ApprovedBy).HasColumnName("approved_by");
            entity.Property(e => e.AutoRenew).HasColumnName("auto_renew");
            entity.Property(e => e.CancellationReason).HasColumnName("cancellation_reason");
            entity.Property(e => e.CancelledAt).HasColumnName("cancelled_at");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.DiscountAmount)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("discount_amount");
            entity.Property(e => e.EndsAt).HasColumnName("ends_at");
            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(20)
                .HasColumnName("payment_method");
            entity.Property(e => e.PaymentNotes).HasColumnName("payment_notes");
            entity.Property(e => e.PaymentReference)
                .HasMaxLength(100)
                .HasColumnName("payment_reference");
            entity.Property(e => e.PlanId).HasColumnName("plan_id");
            entity.Property(e => e.RejectionReason).HasColumnName("rejection_reason");
            entity.Property(e => e.StartsAt).HasColumnName("starts_at");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("pending")
                .HasColumnName("status");
            entity.Property(e => e.SupplierId).HasColumnName("supplier_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.ApprovedByNavigation).WithMany(p => p.Subscriptions)
                .HasForeignKey(d => d.ApprovedBy)
                .HasConstraintName("FK_subs_approved");

            entity.HasOne(d => d.Plan).WithMany(p => p.Subscriptions)
                .HasForeignKey(d => d.PlanId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_subs_plan");

            entity.HasOne(d => d.Supplier).WithMany(p => p.Subscriptions)
                .HasForeignKey(d => d.SupplierId)
                .HasConstraintName("FK_subs_supplier");
        });

        modelBuilder.Entity<SubscriptionHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__subscrip__3213E83FA3EC7360");

            entity.ToTable("subscription_history");

            entity.HasIndex(e => e.SubscriptionId, "idx_history_sub");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Action)
                .HasMaxLength(20)
                .HasColumnName("action");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("amount");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.NewPlanId).HasColumnName("new_plan_id");
            entity.Property(e => e.NewStatus)
                .HasMaxLength(20)
                .HasColumnName("new_status");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.OldPlanId).HasColumnName("old_plan_id");
            entity.Property(e => e.OldStatus)
                .HasMaxLength(20)
                .HasColumnName("old_status");
            entity.Property(e => e.PerformedBy).HasColumnName("performed_by");
            entity.Property(e => e.SubscriptionId).HasColumnName("subscription_id");
            entity.Property(e => e.SupplierId).HasColumnName("supplier_id");

            entity.HasOne(d => d.PerformedByNavigation).WithMany(p => p.SubscriptionHistories)
                .HasForeignKey(d => d.PerformedBy)
                .HasConstraintName("FK_history_performer");

            entity.HasOne(d => d.Subscription).WithMany(p => p.SubscriptionHistories)
                .HasForeignKey(d => d.SubscriptionId)
                .HasConstraintName("FK_history_sub");

            entity.HasOne(d => d.Supplier).WithMany(p => p.SubscriptionHistories)
                .HasForeignKey(d => d.SupplierId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_history_supplier");
        });

        modelBuilder.Entity<SubscriptionPlan>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__subscrip__3213E83F3FCCCA19");

            entity.ToTable("subscription_plans");

            entity.HasIndex(e => e.IsActive, "idx_plans_active");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BadgeText)
                .HasMaxLength(50)
                .HasColumnName("badge_text");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.Currency)
                .HasMaxLength(3)
                .HasDefaultValue("SAR")
                .HasColumnName("currency");
            entity.Property(e => e.DescriptionAr).HasColumnName("description_ar");
            entity.Property(e => e.DescriptionEn).HasColumnName("description_en");
            entity.Property(e => e.DurationDays)
                .HasDefaultValue(30)
                .HasColumnName("duration_days");
            entity.Property(e => e.Features).HasColumnName("features");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsPopular).HasColumnName("is_popular");
            entity.Property(e => e.MaxImagesPerPart)
                .HasDefaultValue(5)
                .HasColumnName("max_images_per_part");
            entity.Property(e => e.MaxParts).HasColumnName("max_parts");
            entity.Property(e => e.MaxShops)
                .HasDefaultValue(1)
                .HasColumnName("max_shops");
            entity.Property(e => e.NameAr)
                .HasMaxLength(100)
                .HasColumnName("name_ar");
            entity.Property(e => e.NameEn)
                .HasMaxLength(100)
                .HasColumnName("name_en");
            entity.Property(e => e.Price)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("price");
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<SupplierProfile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__supplier__3213E83F4C7E058B");

            entity.ToTable("supplier_profile");

            entity.HasIndex(e => e.CommercialRegister, "UQ_supplier_cr").IsUnique();

            entity.HasIndex(e => e.UserId, "UQ_supplier_user").IsUnique();

            entity.HasIndex(e => e.City, "idx_supplier_city");

            entity.HasIndex(e => e.Status, "idx_supplier_status");

            entity.HasIndex(e => e.UserId, "idx_supplier_user");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BusinessNameAr)
                .HasMaxLength(150)
                .HasColumnName("business_name_ar");
            entity.Property(e => e.BusinessNameEn)
                .HasMaxLength(150)
                .HasColumnName("business_name_en");
            entity.Property(e => e.City)
                .HasMaxLength(50)
                .HasColumnName("city");
            entity.Property(e => e.CommercialRegister)
                .HasMaxLength(50)
                .HasColumnName("commercial_register");
            entity.Property(e => e.CompletedOrders).HasColumnName("completed_orders");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.District)
                .HasMaxLength(100)
                .HasColumnName("district");
            entity.Property(e => e.IsVerified).HasColumnName("is_verified");
            entity.Property(e => e.LicenseNumber)
                .HasMaxLength(50)
                .HasColumnName("license_number");
            entity.Property(e => e.ManagerName)
                .HasMaxLength(100)
                .HasColumnName("manager_name");
            entity.Property(e => e.RatingAverage)
                .HasColumnType("decimal(3, 2)")
                .HasColumnName("rating_average");
            entity.Property(e => e.RatingCount).HasColumnName("rating_count");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("inactive")
                .HasColumnName("status");
            entity.Property(e => e.LogoUrl)
                  .HasColumnName("logo_url")
                  .HasMaxLength(500);

            entity.Property(e => e.Latitude)
                .HasColumnName("latitude")
                .HasColumnType("decimal(10, 8)");

            entity.Property(e => e.Longitude)
                .HasColumnName("longitude")
                .HasColumnType("decimal(11, 8)");
            entity.Property(e => e.TaxNumber)
                .HasMaxLength(50)
                .HasColumnName("tax_number");
            entity.Property(e => e.TotalOrders).HasColumnName("total_orders");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.VerificationStatus)
                .HasMaxLength(20)
                .HasDefaultValue("not_submitted")
                .HasColumnName("verification_status");
            entity.Property(e => e.VerifiedAt).HasColumnName("verified_at");
            entity.Property(e => e.VerifiedBy).HasColumnName("verified_by");

            entity.HasOne(d => d.User).WithOne(p => p.SupplierProfileUser)
                .HasForeignKey<SupplierProfile>(d => d.UserId)
                .HasConstraintName("FK_supplier_user");

            entity.HasOne(d => d.VerifiedByNavigation).WithMany(p => p.SupplierProfileVerifiedByNavigations)
                .HasForeignKey(d => d.VerifiedBy)
                .HasConstraintName("FK_supplier_verified_by");
        });

        modelBuilder.Entity<SupplierSession>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Token).HasMaxLength(500).IsRequired();
            entity.Property(e => e.DeviceType).HasMaxLength(20);
            entity.Property(e => e.DeviceName).HasMaxLength(100);
            entity.Property(e => e.FcmToken).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasIndex(e => e.SupplierId);
            entity.HasIndex(e => e.Token);

            entity.HasOne(d => d.Supplier)
                .WithMany(p => p.SupplierSessions)
                .HasForeignKey(d => d.SupplierId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__users__3213E83F995BE337");

            entity.ToTable("users");

            entity.HasIndex(e => e.Email, "UQ_users_email").IsUnique();

            entity.HasIndex(e => e.Phone, "UQ_users_phone").IsUnique();

            entity.HasIndex(e => e.Phone, "idx_users_phone");

            entity.HasIndex(e => e.Status, "idx_users_status");

            entity.HasIndex(e => e.UserType, "idx_users_type");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AvatarUrl)
                .HasMaxLength(500)
                .HasColumnName("avatar_url");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .HasColumnName("email")
                .IsRequired(false);
            entity.Property(e => e.FullName)
                .HasMaxLength(100)
                .HasColumnName("full_name");
            entity.Property(e => e.LastLoginAt).HasColumnName("last_login_at");
            entity.Property(e => e.NotificationsEnabled)
                .HasDefaultValue(true)
                .HasColumnName("notifications_enabled");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("password_hash");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.PreferredLanguage)
                .HasMaxLength(2)
                .HasDefaultValue("ar")
                .HasColumnName("preferred_language");
            entity.Property(e => e.Status)
                .HasMaxLength(100)
                .HasDefaultValue("active")
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserType)
                .HasMaxLength(20)
                .HasDefaultValue("customer")
                .HasColumnName("user_type");
            entity.Property(e => e.Street)
                .HasColumnName("street")
                .HasMaxLength(255);

            entity.Property(e => e.CityId)
                .HasColumnName("city_id");

            entity.Property(e => e.PostalCode)
                .HasColumnName("postal_code")
                .HasMaxLength(20);

            entity.Property(e => e.Latitude)
                .HasColumnName("latitude")
                .HasColumnType("decimal(10, 8)");

            entity.Property(e => e.Longitude)
                .HasColumnName("longitude")
                .HasColumnType("decimal(11, 8)");

            entity.HasOne(d => d.City)
                .WithMany()
                .HasForeignKey(d => d.CityId)
                .HasConstraintName("fk_users_city")
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<City>(entity =>
        {
            entity.ToTable("cities");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id");

            entity.Property(e => e.NameAr)
                .HasColumnName("name_ar")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.NameEn)
                .HasColumnName("name_en")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.IsActive)
                .HasColumnName("is_active")
                .HasDefaultValue(true);
        });


        modelBuilder.Entity<UserSession>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__user_ses__3213E83F4FF0898D");

            entity.ToTable("user_sessions");

            entity.HasIndex(e => e.IsActive, "idx_sessions_active");

            entity.HasIndex(e => e.UserId, "idx_sessions_user");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.DeviceInfo).HasColumnName("device_info");
            entity.Property(e => e.DeviceName)
                .HasMaxLength(100)
                .HasColumnName("device_name");
            entity.Property(e => e.DeviceType)
                .HasMaxLength(20)
                .HasColumnName("device_type");
            entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");
            entity.Property(e => e.FcmToken)
                .HasMaxLength(500)
                .HasColumnName("fcm_token");
            entity.Property(e => e.IpAddress)
                .HasMaxLength(45)
                .HasColumnName("ip_address");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.LastActivityAt).HasColumnName("last_activity_at");
            entity.Property(e => e.RefreshToken)
                .HasMaxLength(500)
                .HasColumnName("refresh_token");
            entity.Property(e => e.Token)
                .HasMaxLength(2000)
                .HasColumnName("token");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserAgent).HasColumnName("user_agent");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.UserSessions)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_sessions_user");
        });

        modelBuilder.Entity<VActiveSupplier>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_active_suppliers");

            entity.Property(e => e.BusinessNameAr)
                .HasMaxLength(150)
                .HasColumnName("business_name_ar");
            entity.Property(e => e.BusinessNameEn)
                .HasMaxLength(150)
                .HasColumnName("business_name_en");
            entity.Property(e => e.City)
                .HasMaxLength(50)
                .HasColumnName("city");
            entity.Property(e => e.FullName)
                .HasMaxLength(100)
                .HasColumnName("full_name");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IsVerified).HasColumnName("is_verified");
            entity.Property(e => e.PartsCount).HasColumnName("parts_count");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.RatingAverage)
                .HasColumnType("decimal(3, 2)")
                .HasColumnName("rating_average");
            entity.Property(e => e.RatingCount).HasColumnName("rating_count");
            entity.Property(e => e.ShopsCount).HasColumnName("shops_count");
            entity.Property(e => e.TotalOrders).HasColumnName("total_orders");
        });

        modelBuilder.Entity<VOrdersDetailed>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_orders_detailed");

            entity.Property(e => e.CancelReason).HasColumnName("cancel_reason");
            entity.Property(e => e.CancelledAt).HasColumnName("cancelled_at");
            entity.Property(e => e.CancelledBy)
                .HasMaxLength(20)
                .HasColumnName("cancelled_by");
            entity.Property(e => e.CompletedAt).HasColumnName("completed_at");
            entity.Property(e => e.ConfirmedAt).HasColumnName("confirmed_at");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.Currency)
                .HasMaxLength(3)
                .HasColumnName("currency");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.CustomerName)
                .HasMaxLength(100)
                .HasColumnName("customer_name");
            entity.Property(e => e.CustomerNotes).HasColumnName("customer_notes");
            entity.Property(e => e.CustomerPhone)
                .HasMaxLength(20)
                .HasColumnName("customer_phone");
            entity.Property(e => e.DiscountAmount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("discount_amount");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.InternalNotes).HasColumnName("internal_notes");
            entity.Property(e => e.IsReviewed).HasColumnName("is_reviewed");
            entity.Property(e => e.ItemsCount).HasColumnName("items_count");
            entity.Property(e => e.OrderNumber)
                .HasMaxLength(30)
                .HasColumnName("order_number");
            entity.Property(e => e.ProcessingAt).HasColumnName("processing_at");
            entity.Property(e => e.ReadyAt).HasColumnName("ready_at");
            entity.Property(e => e.ShopCity)
                .HasMaxLength(50)
                .HasColumnName("shop_city");
            entity.Property(e => e.ShopId).HasColumnName("shop_id");
            entity.Property(e => e.ShopName)
                .HasMaxLength(150)
                .HasColumnName("shop_name");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasColumnName("status");
            entity.Property(e => e.Subtotal)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("subtotal");
            entity.Property(e => e.SupplierId).HasColumnName("supplier_id");
            entity.Property(e => e.SupplierName)
                .HasMaxLength(150)
                .HasColumnName("supplier_name");
            entity.Property(e => e.SupplierNotes).HasColumnName("supplier_notes");
            entity.Property(e => e.TotalAmount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("total_amount");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        });

        // استبدل الكود القديم بهذا في TashlihContext.cs

        modelBuilder.Entity<VPartsDetailed>(entity =>
        {
            entity.HasNoKey();
            entity.ToView("v_parts_detailed");

            // معلومات أساسية
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.SupplierId).HasColumnName("supplier_id");
            entity.Property(e => e.NameAr).HasMaxLength(200).HasColumnName("name_ar");
            entity.Property(e => e.NameEn).HasMaxLength(200).HasColumnName("name_en");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.PartNumber).HasMaxLength(50).HasColumnName("part_number");
            entity.Property(e => e.OemNumber).HasMaxLength(50).HasColumnName("oem_number");
            entity.Property(e => e.Condition).HasMaxLength(20).HasColumnName("condition");
            entity.Property(e => e.ConditionDetails).HasColumnName("condition_details");
            entity.Property(e => e.WarrantyType).HasMaxLength(20).HasColumnName("warranty_type");
            entity.Property(e => e.WarrantyDays).HasColumnName("warranty_days");
            entity.Property(e => e.Price).HasColumnType("decimal(10, 2)").HasColumnName("price");
            entity.Property(e => e.OriginalPrice).HasColumnType("decimal(10, 2)").HasColumnName("original_price");
            entity.Property(e => e.Currency).HasMaxLength(3).HasColumnName("currency");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.Status).HasMaxLength(20).HasColumnName("status");
            entity.Property(e => e.IsFeatured).HasColumnName("is_featured");
            entity.Property(e => e.FeaturedUntil).HasColumnName("featured_until");
            entity.Property(e => e.ViewsCount).HasColumnName("views_count");
            entity.Property(e => e.SalesCount).HasColumnName("sales_count");
            entity.Property(e => e.FavoritesCount).HasColumnName("favorites_count");

            // التصنيف
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CategoryNameAr).HasMaxLength(100).HasColumnName("category_name_ar");
            entity.Property(e => e.CategoryNameEn).HasMaxLength(100).HasColumnName("category_name_en");
            entity.Property(e => e.CustomCategory).HasMaxLength(100).HasColumnName("custom_category");

            // نوع المركبة
            entity.Property(e => e.VehicleTypeId).HasColumnName("vehicle_type_id");
            entity.Property(e => e.VehicleTypeNameAr).HasMaxLength(100).HasColumnName("vehicle_type_name_ar");
            entity.Property(e => e.VehicleTypeNameEn).HasMaxLength(100).HasColumnName("vehicle_type_name_en");
            entity.Property(e => e.CustomVehicleType).HasMaxLength(100).HasColumnName("custom_vehicle_type");

            // التصنيف الفرعي
            entity.Property(e => e.VehicleSubcategoryId).HasColumnName("vehicle_subcategory_id");
            entity.Property(e => e.SubcategoryNameAr).HasMaxLength(100).HasColumnName("subcategory_name_ar");
            entity.Property(e => e.SubcategoryNameEn).HasMaxLength(100).HasColumnName("subcategory_name_en");
            entity.Property(e => e.CustomSubcategory).HasMaxLength(100).HasColumnName("custom_subcategory");

            // الشركة المصنعة
            entity.Property(e => e.MakeId).HasColumnName("make_id");
            entity.Property(e => e.MakeNameAr).HasMaxLength(100).HasColumnName("make_name_ar");
            entity.Property(e => e.MakeNameEn).HasMaxLength(100).HasColumnName("make_name_en");
            entity.Property(e => e.MakeLogoUrl).HasMaxLength(500).HasColumnName("make_logo_url");
            entity.Property(e => e.CustomMake).HasMaxLength(100).HasColumnName("custom_make");

            // الموديل
            entity.Property(e => e.ModelId).HasColumnName("model_id");
            entity.Property(e => e.ModelNameAr).HasMaxLength(100).HasColumnName("model_name_ar");
            entity.Property(e => e.ModelNameEn).HasMaxLength(100).HasColumnName("model_name_en");
            entity.Property(e => e.CustomModel).HasMaxLength(100).HasColumnName("custom_model");

            // السنوات
            entity.Property(e => e.YearFrom).HasColumnName("year_from");
            entity.Property(e => e.YearTo).HasColumnName("year_to");
            entity.Property(e => e.VinNumber).HasMaxLength(17).HasColumnName("vin_number");

            // التوصيل
            entity.Property(e => e.DeliveryAvailable).HasColumnName("delivery_available");
            entity.Property(e => e.DeliveryByShop).HasColumnName("delivery_by_shop");
            entity.Property(e => e.DeliveryNotes).HasMaxLength(500).HasColumnName("delivery_notes");

          

            // معلومات المورد
            entity.Property(e => e.SupplierId).HasColumnName("supplier_id");
            entity.Property(e => e.SupplierName).HasMaxLength(150).HasColumnName("supplier_name");
            entity.Property(e => e.BusinessNameAr).HasMaxLength(150).HasColumnName("business_name_ar");
            entity.Property(e => e.BusinessNameEn).HasMaxLength(150).HasColumnName("business_name_en");
            entity.Property(e => e.SupplierCity).HasColumnName("supplier_city");
            entity.Property(e => e.SupplierDistrict).HasColumnName("supplier_district");
            entity.Property(e => e.SupplierPhone).HasColumnName("supplier_phone");
            entity.Property(e => e.SupplierLogoUrl).HasColumnName("supplier_logo_url");
            entity.Property(e => e.SupplierIsVerified).HasColumnName("supplier_is_verified");
            entity.Property(e => e.SupplierVerificationStatus).HasMaxLength(20).HasColumnName("supplier_verification_status");
            entity.Property(e => e.SupplierRating).HasColumnType("decimal(3, 2)").HasColumnName("supplier_rating");
            entity.Property(e => e.SupplierRatingCount).HasColumnName("supplier_rating_count");

            // الصور
            entity.Property(e => e.PrimaryImageUrl).HasMaxLength(500).HasColumnName("primary_image_url");
            entity.Property(e => e.ImagesCount).HasColumnName("images_count");

            // التواريخ
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
        });   

        modelBuilder.Entity<VehicleMake>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__vehicle___3213E83FD5DCE6FD");

            entity.ToTable("vehicle_makes");

            entity.HasIndex(e => e.IsPopular, "idx_makes_popular");

            entity.HasIndex(e => e.VehicleTypeId, "idx_makes_type");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Country)
                .HasMaxLength(50)
                .HasColumnName("country");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsPopular).HasColumnName("is_popular");
            entity.Property(e => e.LogoUrl)
                .HasMaxLength(500)
                .HasColumnName("logo_url");
            entity.Property(e => e.NameAr)
                .HasMaxLength(100)
                .HasColumnName("name_ar");
            entity.Property(e => e.NameEn)
                .HasMaxLength(100)
                .HasColumnName("name_en");
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");
            entity.Property(e => e.VehicleTypeId).HasColumnName("vehicle_type_id");

            entity.HasOne(d => d.VehicleType).WithMany(p => p.VehicleMakes)
                .HasForeignKey(d => d.VehicleTypeId)
                .HasConstraintName("FK_makes_type");
        });

        modelBuilder.Entity<VehicleModel>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__vehicle___3213E83F96BE9C1E");

            entity.ToTable("vehicle_models");

            entity.HasIndex(e => e.MakeId, "idx_models_make");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.MakeId).HasColumnName("make_id");
            entity.Property(e => e.NameAr)
                .HasMaxLength(100)
                .HasColumnName("name_ar");
            entity.Property(e => e.NameEn)
                .HasMaxLength(100)
                .HasColumnName("name_en");
            entity.Property(e => e.YearFrom).HasColumnName("year_from");
            entity.Property(e => e.YearTo).HasColumnName("year_to");

            entity.HasOne(d => d.Make).WithMany(p => p.VehicleModels)
                .HasForeignKey(d => d.MakeId)
                .HasConstraintName("FK_models_make");
        });

        modelBuilder.Entity<VehicleSubcategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__vehicle___3213E83FFAD1E4ED");

            entity.ToTable("vehicle_subcategories");

            entity.HasIndex(e => e.VehicleTypeId, "idx_subcategories_type");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.Icon)
                .HasMaxLength(50)
                .HasColumnName("icon");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.NameAr)
                .HasMaxLength(100)
                .HasColumnName("name_ar");
            entity.Property(e => e.NameEn)
                .HasMaxLength(100)
                .HasColumnName("name_en");
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");
            entity.Property(e => e.VehicleTypeId).HasColumnName("vehicle_type_id");

            entity.HasOne(d => d.VehicleType).WithMany(p => p.VehicleSubcategories)
                .HasForeignKey(d => d.VehicleTypeId)
                .HasConstraintName("FK_subcategories_type");
        });

        modelBuilder.Entity<VehicleType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__vehicle___3213E83F8A30BF0F");

            entity.ToTable("vehicle_types");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.Icon)
                .HasMaxLength(50)
                .HasColumnName("icon");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.NameAr)
                .HasMaxLength(100)
                .HasColumnName("name_ar");
            entity.Property(e => e.NameEn)
                .HasMaxLength(100)
                .HasColumnName("name_en");
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
