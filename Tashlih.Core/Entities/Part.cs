using System;
using System.Collections.Generic;

namespace Tashlih.Core.Entities;

public partial class Part
{
    public long Id { get; set; }

    public long ShopId { get; set; }

    public long? CategoryId { get; set; }

    public string NameAr { get; set; } = null!;

    public string? NameEn { get; set; }

    public string? Description { get; set; }

    public string? PartNumber { get; set; }

    public string? OemNumber { get; set; }

    public string Condition { get; set; } = null!;

    public string? ConditionDetails { get; set; }

    public int? WarrantyDays { get; set; }

    public decimal Price { get; set; }

    public decimal? OriginalPrice { get; set; }

    public string Currency { get; set; } = null!;

    public int Quantity { get; set; }

    public int ViewsCount { get; set; }

    public int SalesCount { get; set; }

    public int FavoritesCount { get; set; }

    public string Status { get; set; } = null!;

    public bool IsFeatured { get; set; }

    public DateOnly? FeaturedUntil { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public int? VehicleTypeId { get; set; }

    public int? VehicleSubcategoryId { get; set; }

    public int? MakeId { get; set; }

    public int? ModelId { get; set; }

    public short? YearFrom { get; set; }

    public short? YearTo { get; set; }

    public string? VinNumber { get; set; }

    public string? WarrantyType { get; set; }

    public bool DeliveryAvailable { get; set; }

    public bool DeliveryByShop { get; set; }

    public string? DeliveryNotes { get; set; }

    public string? CustomVehicleType { get; set; }

    public string? CustomSubcategory { get; set; }

    public string? CustomMake { get; set; }

    public string? CustomModel { get; set; }

    public string? CustomCategory { get; set; }

    public virtual PartCategory? Category { get; set; }

    public virtual ICollection<ChatThread> ChatThreads { get; set; } = new List<ChatThread>();

    public virtual VehicleMake? Make { get; set; }

    public virtual VehicleModel? Model { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ICollection<PartImage> PartImages { get; set; } = new List<PartImage>();

    public virtual Shop Shop { get; set; } = null!;

    public virtual VehicleSubcategory? VehicleSubcategory { get; set; }

    public virtual VehicleType? VehicleType { get; set; }
}
