namespace Tashlih.Core.Entities;

public class WarrantyType
{
    public long Id { get; set; }
    public string Key { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public string? NameEn { get; set; }
    public int Days { get; set; }
}