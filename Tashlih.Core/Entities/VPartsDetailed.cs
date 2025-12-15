using System;
using System.Collections.Generic;

namespace Tashlih.Core.Entities;
public partial class VPartsDetailed
{
    public long Id { get; set; }

    public string NameAr { get; set; } = null!;

    public string? NameEn { get; set; }

    public string? Description { get; set; }

    public string? PartNumber { get; set; }

    public string? OemNumber { get; set; }

    public string? VinNumber { get; set; }

    public string Condition { get; set; } = null!;

    public decimal Price { get; set; }

    public decimal? OriginalPrice { get; set; }

    public int Quantity { get; set; }

    public string Status { get; set; } = null!;

    public string? WarrantyType { get; set; }

    public bool DeliveryAvailable { get; set; }

    public bool DeliveryByShop { get; set; }

    public int ViewsCount { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? VehicleTypeAr { get; set; }

    public string? VehicleTypeEn { get; set; }

    public string? SubcategoryAr { get; set; }

    public string? SubcategoryEn { get; set; }

    public string? MakeAr { get; set; }

    public string? MakeEn { get; set; }

    public string? ModelAr { get; set; }

    public string? ModelEn { get; set; }

    public short? YearFrom { get; set; }

    public short? YearTo { get; set; }

    public string? CategoryAr { get; set; }

    public string? CategoryEn { get; set; }

    public long ShopId { get; set; }

    public string ShopNameAr { get; set; } = null!;

    public string ShopCity { get; set; } = null!;

    public long SupplierId { get; set; }

    public string SupplierName { get; set; } = null!;

    public decimal RatingAverage { get; set; }

    public bool IsVerified { get; set; }

    public string? PrimaryImage { get; set; }
}
