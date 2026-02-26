using Microsoft.EntityFrameworkCore;
using Tashlih.Application.DTOs.Parts;
using Tashlih.Application.Interfaces;
using Tashlih.Core.Entities;
using Tashlih.Infrastructure.Models;

namespace Tashlih.Infrastructure.Services
{
    public class PartsService : IPartsService
    {
        private readonly TashlihContext _context;
        private readonly IFileService _fileService;
        private readonly ILogService _logService;
        public PartsService(TashlihContext context, IFileService fileService, ILogService logService)
        {
            _context = context;
            _fileService = fileService;
            _logService = logService;
        }

        #region Supplier Methods

        /// <summary>
        /// إضافة قطعة جديدة
        /// </summary>
        public async Task<PartResponse> CreatePartAsync(long supplierId, CreatePartRequest request)
        {
            // التحقق من وجود المورد وأنه موثق
            var supplier = await _context.SupplierProfiles
                .FirstOrDefaultAsync(s => s.Id == supplierId && s.IsVerified && s.Status == "active");
            if (supplier == null)
            {
                return new PartResponse
                {
                    Success = false,
                    Message = "Supplier not found or not verified",
                    MessageAr = "المورد غير موجود أو غير موثق"
                };
            }

            // ✅ التحقق من الاشتراك وحدود الباقة
            var subscription = await _context.Subscriptions
                .Include(s => s.Plan)
                .FirstOrDefaultAsync(s => s.SupplierId == supplierId && s.Status == "active");

            if (subscription == null)
            {
                return new PartResponse
                {
                    Success = false,
                    Message = "No active subscription. Please subscribe to a plan first",
                    MessageAr = "لا يوجد اشتراك فعال. يرجى الاشتراك في باقة أولاً"
                };
            }

            // التحقق من عدد القطع
            var currentPartsCount = await _context.Parts
                .CountAsync(p => p.SupplierId == supplierId && p.DeletedAt == null);

            var maxParts = subscription.Plan.MaxParts;

            if (maxParts.HasValue && currentPartsCount >= maxParts.Value)
            {
                return new PartResponse
                {
                    Success = false,
                    Message = $"You have reached the maximum limit of {maxParts} parts. Please upgrade your plan",
                    MessageAr = $"وصلت للحد الأقصى ({maxParts} قطعة). يرجى ترقية باقتك"
                };
            }

            // التحقق من عدد الصور
            var maxImages = subscription.Plan.MaxImagesPerPart;
            if (request.Images != null && request.Images.Count > maxImages)
            {
                return new PartResponse
                {
                    Success = false,
                    Message = $"Maximum {maxImages} images allowed per part. Please upgrade your plan",
                    MessageAr = $"الحد الأقصى {maxImages} صور لكل قطعة. يرجى ترقية باقتك"
                };
            }

            // جلب بيانات الحالة والضمان من الجداول
            var condition = await _context.PartConditions.FindAsync(request.ConditionId);
            if (condition == null)
            {
                return new PartResponse
                {
                    Success = false,
                    Message = "Invalid condition",
                    MessageAr = "حالة القطعة غير صحيحة"
                };
            }

            WarrantyType? warranty = null;
            if (request.WarrantyTypeId.HasValue)
            {
                warranty = await _context.WarrantyTypes.FindAsync(request.WarrantyTypeId.Value);
            }

            // إنشاء القطعة
            var part = new Part
            {
                SupplierId = supplierId,
                NameAr = request.NameAr,
                NameEn = request.NameEn,
                Description = request.Description,
                PartNumber = request.PartNumber,
                OemNumber = request.OemNumber,
                Condition = condition.Key,
                ConditionId = request.ConditionId,
                ConditionDetails = request.ConditionDetails,
                WarrantyType = warranty?.Key,
                WarrantyTypeId = request.WarrantyTypeId,
                WarrantyDays = warranty?.Days,
                Price = request.Price,
                OriginalPrice = request.OriginalPrice,
                Currency = "SAR",
                Quantity = request.Quantity,
                Status = "available",
                CategoryId = request.CategoryId,
                CustomCategory = request.CustomCategory,
                VehicleTypeId = request.VehicleTypeId,
                CustomVehicleType = request.CustomVehicleType,
                VehicleSubcategoryId = request.VehicleSubcategoryId,
                CustomSubcategory = request.CustomSubcategory,
                MakeId = request.MakeId,
                CustomMake = request.CustomMake,
                ModelId = request.ModelId,
                CustomModel = request.CustomModel,
                YearFrom = request.YearFrom,
                YearTo = request.YearTo,
                VinNumber = request.VinNumber,
                DeliveryAvailable = request.DeliveryAvailable,
                DeliveryByShop = request.DeliveryByShop,
                DeliveryNotes = request.DeliveryNotes,
                CreatedAt = DateTime.UtcNow
            };

            _context.Parts.Add(part);
            await _context.SaveChangesAsync();

            // رفع الصور
            if (request.Images != null && request.Images.Count > 0)
            {
                byte order = 0;
                foreach (var image in request.Images)
                {
                    var imageUrl = await _fileService.UploadFileAsync(image, "parts");
                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        var partImage = new PartImage
                        {
                            PartId = part.Id,
                            ImageUrl = imageUrl,
                            IsPrimary = order == 0,
                            DisplayOrder = order,
                            FileSize = (int)image.Length,
                            MimeType = image.ContentType,
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.PartImages.Add(partImage);
                        order++;
                    }
                }
                await _context.SaveChangesAsync();
            }

            // جلب القطعة من الـ View
            var partView = await _context.VPartsDetaileds
                .FirstOrDefaultAsync(p => p.Id == part.Id);
            var images = await _context.PartImages
                .Where(i => i.PartId == part.Id)
                .OrderBy(i => i.DisplayOrder)
                .ToListAsync();

            return new PartResponse
            {
                Success = true,
                Message = "Part created successfully",
                MessageAr = "تم إضافة القطعة بنجاح",
                Part = MapViewToDto(partView!, images)
            };
        }

        /// <summary>
        /// تعديل قطعة
        /// </summary>
        public async Task<PartResponse> UpdatePartAsync(long supplierId, long partId, UpdatePartRequest request)
        {
            var part = await _context.Parts
                .FirstOrDefaultAsync(p => p.Id == partId && p.SupplierId == supplierId);

            if (part == null)
            {
                return new PartResponse
                {
                    Success = false,
                    Message = "Part not found",
                    MessageAr = "القطعة غير موجودة"
                };
            }

            // حفظ السعر القديم للـ Log
            var oldPrice = part.Price;

            // تحديث الحقول
            if (request.NameAr != null) part.NameAr = request.NameAr;
            if (request.NameEn != null) part.NameEn = request.NameEn;
            if (request.Description != null) part.Description = request.Description;
            if (request.PartNumber != null) part.PartNumber = request.PartNumber;
            if (request.OemNumber != null) part.OemNumber = request.OemNumber;
            if (request.Condition != null) part.Condition = request.Condition;
            if (request.ConditionDetails != null) part.ConditionDetails = request.ConditionDetails;
            if (request.WarrantyType != null) part.WarrantyType = request.WarrantyType;
            if (request.WarrantyDays.HasValue) part.WarrantyDays = request.WarrantyDays;
            if (request.Price.HasValue) part.Price = request.Price.Value;
            if (request.OriginalPrice.HasValue) part.OriginalPrice = request.OriginalPrice;
            if (request.Quantity.HasValue) part.Quantity = request.Quantity.Value;
            if (request.Status != null) part.Status = request.Status;
            if (request.CategoryId.HasValue) part.CategoryId = request.CategoryId;
            if (request.CustomCategory != null) part.CustomCategory = request.CustomCategory;
            if (request.VehicleTypeId.HasValue) part.VehicleTypeId = request.VehicleTypeId;
            if (request.CustomVehicleType != null) part.CustomVehicleType = request.CustomVehicleType;
            if (request.VehicleSubcategoryId.HasValue) part.VehicleSubcategoryId = request.VehicleSubcategoryId;
            if (request.CustomSubcategory != null) part.CustomSubcategory = request.CustomSubcategory;
            if (request.MakeId.HasValue) part.MakeId = request.MakeId;
            if (request.CustomMake != null) part.CustomMake = request.CustomMake;
            if (request.ModelId.HasValue) part.ModelId = request.ModelId;
            if (request.CustomModel != null) part.CustomModel = request.CustomModel;
            if (request.YearFrom.HasValue) part.YearFrom = request.YearFrom;
            if (request.YearTo.HasValue) part.YearTo = request.YearTo;
            if (request.VinNumber != null) part.VinNumber = request.VinNumber;
            if (request.DeliveryAvailable.HasValue) part.DeliveryAvailable = request.DeliveryAvailable.Value;
            if (request.DeliveryByShop.HasValue) part.DeliveryByShop = request.DeliveryByShop.Value;
            if (request.DeliveryNotes != null) part.DeliveryNotes = request.DeliveryNotes;

            part.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // ✅ تسجيل تغيير السعر فقط
            if (request.Price.HasValue && oldPrice != request.Price.Value)
            {
                var supplier = await _context.SupplierProfiles.FindAsync(supplierId);
                await _logService.LogAsync(
                    userId: supplierId,
                    userType: "supplier",
                    userName: supplier?.FullName ?? "مورد",
                    action: "price_change",
                    actionAr: "تغيير سعر",
                    entityType: "part",
                    entityTypeAr: "قطعة",
                    entityId: part.Id,
                    oldValues: new { Price = oldPrice },
                    newValues: new { Price = part.Price },
                    description: $"تم تغيير سعر القطعة '{part.NameAr}' من {oldPrice} إلى {part.Price} ريال"
                );
            }

            var partView = await _context.VPartsDetaileds
                .FirstOrDefaultAsync(p => p.Id == part.Id);

            var images = await _context.PartImages
                .Where(i => i.PartId == part.Id)
                .OrderBy(i => i.DisplayOrder)
                .ToListAsync();

            return new PartResponse
            {
                Success = true,
                Message = "Part updated successfully",
                MessageAr = "تم تعديل القطعة بنجاح",
                Part = MapViewToDto(partView!, images)
            };
        }

        /// <summary>
        /// حذف قطعة (Soft Delete)
        /// </summary>
        public async Task<PartResponse> DeletePartAsync(long supplierId, long partId)
        {
            var part = await _context.Parts
                .FirstOrDefaultAsync(p => p.Id == partId && p.SupplierId == supplierId);

            if (part == null)
            {
                return new PartResponse
                {
                    Success = false,
                    Message = "Part not found",
                    MessageAr = "القطعة غير موجودة"
                };
            }

            var partName = part.NameAr;
            var oldStatus = part.Status;

            part.DeletedAt = DateTime.UtcNow;
            part.Status = "deleted";

            await _context.SaveChangesAsync();

            // ✅ تسجيل العملية
            var supplier = await _context.SupplierProfiles.FindAsync(supplierId);
            await _logService.LogAsync(
                userId: supplierId,
                userType: "supplier",
                userName: supplier?.FullName ?? "مورد",
                action: "delete",
                actionAr: "حذف",
                entityType: "part",
                entityTypeAr: "قطعة",
                entityId: partId,
                oldValues: new { Status = oldStatus, Name = partName },
                newValues: new { Status = "deleted" },
                description: $"تم حذف القطعة: {partName}"
            );

            return new PartResponse
            {
                Success = true,
                Message = "Part deleted successfully",
                MessageAr = "تم حذف القطعة بنجاح"
            };
        }

        /// <summary>
        /// جلب قطع المورد
        /// </summary>
        public async Task<PartsListResponse> GetSupplierPartsAsync(long supplierId, int page = 1, int pageSize = 20, string? status = null)
        {
            var query = _context.VPartsDetaileds
                .AsNoTracking()
                .Where(p => p.SupplierId == supplierId);

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(p => p.Status == status);
            }

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var parts = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var partIds = parts.Select(p => p.Id).ToList();
            var allImages = await _context.PartImages
                .Where(i => partIds.Contains(i.PartId))
                .OrderBy(i => i.DisplayOrder)
                .ToListAsync();

            var partsDto = parts.Select(p =>
            {
                var images = allImages.Where(i => i.PartId == p.Id).ToList();
                return MapViewToDto(p, images);
            }).ToList();

            return new PartsListResponse
            {
                Success = true,
                Parts = partsDto,
                Pagination = new PaginationInfo
                {
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    TotalPages = totalPages,
                    HasNext = page < totalPages,
                    HasPrevious = page > 1
                }
            };
        }

        /// <summary>
        /// إضافة صورة للقطعة
        /// </summary>
        public async Task<PartResponse> AddPartImageAsync(long supplierId, long partId, AddPartImageRequest request)
        {
            var part = await _context.Parts
                .Include(p => p.PartImages)
                .FirstOrDefaultAsync(p => p.Id == partId && p.SupplierId == supplierId);

            if (part == null)
            {
                return new PartResponse
                {
                    Success = false,
                    Message = "Part not found",
                    MessageAr = "القطعة غير موجودة"
                };
            }

            var imageUrl = await _fileService.UploadFileAsync(request.Image, "parts");
            if (string.IsNullOrEmpty(imageUrl))
            {
                return new PartResponse
                {
                    Success = false,
                    Message = "Failed to upload image",
                    MessageAr = "فشل رفع الصورة"
                };
            }

            // لو هي الصورة الرئيسية، نشيل الـ primary من الباقي
            if (request.IsPrimary)
            {
                foreach (var img in part.PartImages)
                {
                    img.IsPrimary = false;
                }
            }

            var partImage = new PartImage
            {
                PartId = part.Id,
                ImageUrl = imageUrl,
                IsPrimary = request.IsPrimary || !part.PartImages.Any(),
                DisplayOrder = (byte)part.PartImages.Count,
                FileSize = (int)request.Image.Length,
                MimeType = request.Image.ContentType,
                CreatedAt = DateTime.UtcNow
            };

            _context.PartImages.Add(partImage);
            await _context.SaveChangesAsync();

            var partView = await _context.VPartsDetaileds
                .FirstOrDefaultAsync(p => p.Id == part.Id);

            var images = await _context.PartImages
                .Where(i => i.PartId == part.Id)
                .OrderBy(i => i.DisplayOrder)
                .ToListAsync();

            return new PartResponse
            {
                Success = true,
                Message = "Image added successfully",
                MessageAr = "تم إضافة الصورة بنجاح",
                Part = MapViewToDto(partView!, images)
            };
        }

        /// <summary>
        /// حذف صورة من القطعة
        /// </summary>
        public async Task<PartResponse> DeletePartImageAsync(long supplierId, long partId, long imageId)
        {
            var part = await _context.Parts
                .FirstOrDefaultAsync(p => p.Id == partId && p.SupplierId == supplierId);

            if (part == null)
            {
                return new PartResponse
                {
                    Success = false,
                    Message = "Part not found",
                    MessageAr = "القطعة غير موجودة"
                };
            }

            var image = await _context.PartImages
                .FirstOrDefaultAsync(i => i.Id == imageId && i.PartId == partId);

            if (image == null)
            {
                return new PartResponse
                {
                    Success = false,
                    Message = "Image not found",
                    MessageAr = "الصورة غير موجودة"
                };
            }

            // حذف الملف
            await _fileService.DeleteFileAsync(image.ImageUrl);

            var wasPrimary = image.IsPrimary;
            _context.PartImages.Remove(image);
            await _context.SaveChangesAsync();

            // لو كانت الصورة الرئيسية، نعين صورة ثانية
            if (wasPrimary)
            {
                var firstImage = await _context.PartImages
                    .Where(i => i.PartId == partId)
                    .OrderBy(i => i.DisplayOrder)
                    .FirstOrDefaultAsync();

                if (firstImage != null)
                {
                    firstImage.IsPrimary = true;
                    await _context.SaveChangesAsync();
                }
            }

            var partView = await _context.VPartsDetaileds
                .FirstOrDefaultAsync(p => p.Id == part.Id);

            var images = await _context.PartImages
                .Where(i => i.PartId == part.Id)
                .OrderBy(i => i.DisplayOrder)
                .ToListAsync();

            return new PartResponse
            {
                Success = true,
                Message = "Image deleted successfully",
                MessageAr = "تم حذف الصورة بنجاح",
                Part = MapViewToDto(partView!, images)
            };
        }

        /// <summary>
        /// تعيين صورة كرئيسية
        /// </summary>
        public async Task<PartResponse> SetPrimaryImageAsync(long supplierId, long partId, long imageId)
        {
            var part = await _context.Parts
                .Include(p => p.PartImages)
                .FirstOrDefaultAsync(p => p.Id == partId && p.SupplierId == supplierId);

            if (part == null)
            {
                return new PartResponse
                {
                    Success = false,
                    Message = "Part not found",
                    MessageAr = "القطعة غير موجودة"
                };
            }

            var image = part.PartImages.FirstOrDefault(i => i.Id == imageId);
            if (image == null)
            {
                return new PartResponse
                {
                    Success = false,
                    Message = "Image not found",
                    MessageAr = "الصورة غير موجودة"
                };
            }

            foreach (var img in part.PartImages)
            {
                img.IsPrimary = img.Id == imageId;
            }

            await _context.SaveChangesAsync();

            var partView = await _context.VPartsDetaileds
                .FirstOrDefaultAsync(p => p.Id == part.Id);

            var images = await _context.PartImages
                .Where(i => i.PartId == part.Id)
                .OrderBy(i => i.DisplayOrder)
                .ToListAsync();

            return new PartResponse
            {
                Success = true,
                Message = "Primary image set successfully",
                MessageAr = "تم تعيين الصورة الرئيسية بنجاح",
                Part = MapViewToDto(partView!, images)
            };
        }

        #endregion

        #region Customer Methods

        /// <summary>
        /// جلب كل القطع المتاحة
        /// </summary>
        public async Task<PartsListResponse> GetAllPartsAsync(int page, int pageSize)
        {
            // جيب IDs القطع المسموح بعرضها
            var allowedPartIds = await GetAllowedPartIdsAsync();

            var query = _context.VPartsDetaileds
                .AsNoTracking()
                .Where(p => p.Status == "available")
                .Where(p => allowedPartIds.Contains(p.Id)); // ✅ فقط القطع المسموحة

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var parts = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var partIds = parts.Select(p => p.Id).ToList();
            var allImages = await _context.PartImages
                .Where(i => partIds.Contains(i.PartId))
                .OrderBy(i => i.DisplayOrder)
                .ToListAsync();

            var partsDto = parts.Select(p =>
            {
                var images = allImages.Where(i => i.PartId == p.Id).ToList();
                return MapViewToDto(p, images);
            }).ToList();

            return new PartsListResponse
            {
                Success = true,
                Parts = partsDto,
                Pagination = new PaginationInfo
                {
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    TotalPages = totalPages,
                    HasNext = page < totalPages,
                    HasPrevious = page > 1
                }
            };
        }

        /// <summary>
        /// جلب تفاصيل قطعة
        /// </summary>
        public async Task<PartResponse> GetPartByIdAsync(long partId)
        {
            var part = await _context.VPartsDetaileds
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == partId);

            if (part == null)
            {
                return new PartResponse
                {
                    Success = false,
                    Message = "Part not found",
                    MessageAr = "القطعة غير موجودة"
                };
            }

            var images = await _context.PartImages
                .Where(i => i.PartId == partId)
                .OrderBy(i => i.DisplayOrder)
                .ToListAsync();

            return new PartResponse
            {
                Success = true,
                Part = MapViewToDto(part, images)
            };
        }

        /// <summary>
        /// البحث عن قطع
        /// </summary>
        public async Task<PartsListResponse> SearchPartsAsync(SearchPartsRequest request)
        {
            // ✅ جلب IDs القطع المسموح بعرضها حسب حد الباقة
            var allowedPartIds = await GetAllowedPartIdsAsync();
            var query = _context.VPartsDetaileds
                .AsNoTracking()
                .Where(p => p.Status == "available")
                .Where(p => allowedPartIds.Contains(p.Id)); // ✅ فقط القطع المسموحة

            // فلترة بالكلمة المفتاحية
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                var keyword = request.Keyword.ToLower();
                query = query.Where(p =>
                    p.NameAr.ToLower().Contains(keyword) ||
                    (p.NameEn != null && p.NameEn.ToLower().Contains(keyword)) ||
                    (p.PartNumber != null && p.PartNumber.ToLower().Contains(keyword)) ||
                    (p.OemNumber != null && p.OemNumber.ToLower().Contains(keyword)));
            }

            // فلترة بالتصنيف
            if (request.CategoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == request.CategoryId);
            }

            // فلترة بنوع المركبة
            if (request.VehicleTypeId.HasValue)
            {
                query = query.Where(p => p.VehicleTypeId == request.VehicleTypeId);
            }

            // فلترة بالتصنيف الفرعي
            if (request.VehicleSubcategoryId.HasValue)
            {
                query = query.Where(p => p.VehicleSubcategoryId == request.VehicleSubcategoryId);
            }

            // فلترة بالشركة
            if (request.MakeId.HasValue)
            {
                query = query.Where(p => p.MakeId == request.MakeId);
            }

            // فلترة بالموديل
            if (request.ModelId.HasValue)
            {
                query = query.Where(p => p.ModelId == request.ModelId);
            }

            // فلترة بالسنة
            if (request.Year.HasValue)
            {
                query = query.Where(p =>
                    (p.YearFrom == null || p.YearFrom <= request.Year) &&
                    (p.YearTo == null || p.YearTo >= request.Year));
            }

            // فلترة بالحالة
            if (request.ConditionId.HasValue)
            {
                var condition = await _context.PartConditions.FindAsync(request.ConditionId.Value);
                if (condition != null)
                {
                    query = query.Where(p => p.Condition == condition.Key);
                }
            }

            // فلترة بالسعر
            if (request.MinPrice.HasValue)
            {
                query = query.Where(p => p.Price >= request.MinPrice);
            }
            if (request.MaxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= request.MaxPrice);
            }

            // فلترة بالمدينة
            if (request.CityId.HasValue)
            {
                var city = await _context.Cities.FindAsync(request.CityId.Value);
                if (city != null)
                {
                    query = query.Where(p => p.SupplierCity == city.NameAr);
                }
            }

            // فلترة بالضمان
            if (request.HasWarranty.HasValue && request.HasWarranty.Value)
            {
                query = query.Where(p => p.WarrantyType != null && p.WarrantyType != "none");
            }

            // فلترة بالتوصيل
            if (request.DeliveryAvailable.HasValue && request.DeliveryAvailable.Value)
            {
                query = query.Where(p => p.DeliveryAvailable);
            }

            // الترتيب
            query = request.SortBy switch
            {
                "price_asc" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                "popular" => query.OrderByDescending(p => p.ViewsCount),
                "newest" => query.OrderByDescending(p => p.CreatedAt),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize);

            var parts = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var partIds = parts.Select(p => p.Id).ToList();
            var allImages = await _context.PartImages
                .Where(i => partIds.Contains(i.PartId))
                .OrderBy(i => i.DisplayOrder)
                .ToListAsync();

            var partsDto = parts.Select(p =>
            {
                var images = allImages.Where(i => i.PartId == p.Id).ToList();
                return MapViewToDto(p, images);
            }).ToList();

            return new PartsListResponse
            {
                Success = true,
                Parts = partsDto,
                Pagination = new PaginationInfo
                {
                    CurrentPage = request.Page,
                    PageSize = request.PageSize,
                    TotalItems = totalItems,
                    TotalPages = totalPages,
                    HasNext = request.Page < totalPages,
                    HasPrevious = request.Page > 1
                }
            };
        }

        /// <summary>
        /// جلب قطع حسب التصنيف
        /// </summary>
        public async Task<PartsListResponse> GetPartsByCategoryAsync(long categoryId, int page, int pageSize)
        {
            var allowedPartIds = await GetAllowedPartIdsAsync();

            var query = _context.VPartsDetaileds
                .AsNoTracking()
                .Where(p => p.Status == "available" && p.CategoryId == categoryId)
                .Where(p => allowedPartIds.Contains(p.Id)); // ✅ فقط القطع المسموحة

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var parts = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var partIds = parts.Select(p => p.Id).ToList();
            var allImages = await _context.PartImages
                .Where(i => partIds.Contains(i.PartId))
                .OrderBy(i => i.DisplayOrder)
                .ToListAsync();

            var partsDto = parts.Select(p =>
            {
                var images = allImages.Where(i => i.PartId == p.Id).ToList();
                return MapViewToDto(p, images);
            }).ToList();

            return new PartsListResponse
            {
                Success = true,
                Parts = partsDto,
                Pagination = new PaginationInfo
                {
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    TotalPages = totalPages,
                    HasNext = page < totalPages,
                    HasPrevious = page > 1
                }
            };
        }

        /// <summary>
        /// جلب قطع حسب المورد
        /// </summary>
        public async Task<PartsListResponse> GetPartsBySupplierAsync(long supplierId, int page, int pageSize)
        {
            var allowedPartIds = await GetAllowedPartIdsAsync();

            var query = _context.VPartsDetaileds
                .AsNoTracking()
                .Where(p => p.Status == "available" && p.SupplierId == supplierId)
                .Where(p => allowedPartIds.Contains(p.Id)); // ✅ فقط القطع المسموحة

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var parts = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var partIds = parts.Select(p => p.Id).ToList();
            var allImages = await _context.PartImages
                .Where(i => partIds.Contains(i.PartId))
                .OrderBy(i => i.DisplayOrder)
                .ToListAsync();

            var partsDto = parts.Select(p =>
            {
                var images = allImages.Where(i => i.PartId == p.Id).ToList();
                return MapViewToDto(p, images);
            }).ToList();

            return new PartsListResponse
            {
                Success = true,
                Parts = partsDto,
                Pagination = new PaginationInfo
                {
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    TotalPages = totalPages,
                    HasNext = page < totalPages,
                    HasPrevious = page > 1
                }
            };
        }

        /// <summary>
        /// جلب القطع المميزة
        /// </summary>
        public async Task<PartsListResponse> GetFeaturedPartsAsync(int count)
        {
            var allowedPartIds = await GetAllowedPartIdsAsync();

            var parts = await _context.VPartsDetaileds
                .AsNoTracking()
                .Where(p => p.Status == "available" && p.IsFeatured)
                .Where(p => allowedPartIds.Contains(p.Id)) // ✅ فقط القطع المسموحة
                .OrderByDescending(p => p.CreatedAt)
                .Take(count)
                .ToListAsync();

            var partIds = parts.Select(p => p.Id).ToList();
            var allImages = await _context.PartImages
                .Where(i => partIds.Contains(i.PartId))
                .OrderBy(i => i.DisplayOrder)
                .ToListAsync();

            var partsDto = parts.Select(p =>
            {
                var images = allImages.Where(i => i.PartId == p.Id).ToList();
                return MapViewToDto(p, images);
            }).ToList();

            return new PartsListResponse
            {
                Success = true,
                Parts = partsDto
            };
        }

        /// <summary>
        /// جلب أحدث القطع
        /// </summary>
        public async Task<PartsListResponse> GetLatestPartsAsync(int count)
        {
            var allowedPartIds = await GetAllowedPartIdsAsync();

            var parts = await _context.VPartsDetaileds
                .AsNoTracking()
                .Where(p => p.Status == "available")
                .Where(p => allowedPartIds.Contains(p.Id)) // ✅ فقط القطع المسموحة
                .OrderByDescending(p => p.CreatedAt)
                .Take(count)
                .ToListAsync();

            var partIds = parts.Select(p => p.Id).ToList();
            var allImages = await _context.PartImages
                .Where(i => partIds.Contains(i.PartId))
                .OrderBy(i => i.DisplayOrder)
                .ToListAsync();

            var partsDto = parts.Select(p =>
            {
                var images = allImages.Where(i => i.PartId == p.Id).ToList();
                return MapViewToDto(p, images);
            }).ToList();

            return new PartsListResponse
            {
                Success = true,
                Parts = partsDto
            };
        }

        /// <summary>
        /// زيادة عداد المشاهدات
        /// </summary>
        public async Task IncrementViewCountAsync(long partId)
        {
            var part = await _context.Parts.FindAsync(partId);
            if (part != null)
            {
                part.ViewsCount++;
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// البحث مع الفلاتر الذكية
        /// </summary>
        public async Task<SearchPartsResponse> SearchWithFiltersAsync(SearchPartsRequest request)
        {
            var allowedPartIds = await GetAllowedPartIdsAsync();

            var query = _context.VPartsDetaileds
                .Where(p => p.Status == "available")
                .Where(p => allowedPartIds.Contains(p.Id)); // ✅ فقط القطع المسموحة

            // فلترة بالكلمة المفتاحية
            if (!string.IsNullOrEmpty(request.Keyword))
            {
                var keyword = request.Keyword.ToLower();
                query = query.Where(p =>
                    p.NameAr.ToLower().Contains(keyword) ||
                    (p.NameEn != null && p.NameEn.ToLower().Contains(keyword)) ||
                    (p.PartNumber != null && p.PartNumber.ToLower().Contains(keyword)) ||
                    (p.OemNumber != null && p.OemNumber.ToLower().Contains(keyword)));
            }

            // فلترة بالتصنيف
            if (request.CategoryId.HasValue)
                query = query.Where(p => p.CategoryId == request.CategoryId);

            // فلترة بنوع المركبة
            if (request.VehicleTypeId.HasValue)
                query = query.Where(p => p.VehicleTypeId == request.VehicleTypeId);

            // فلترة بالتصنيف الفرعي
            if (request.VehicleSubcategoryId.HasValue)
                query = query.Where(p => p.VehicleSubcategoryId == request.VehicleSubcategoryId);

            // فلترة بالشركة
            if (request.MakeId.HasValue)
                query = query.Where(p => p.MakeId == request.MakeId);

            // فلترة بالموديل
            if (request.ModelId.HasValue)
                query = query.Where(p => p.ModelId == request.ModelId);

            // فلترة بالسنة
            if (request.Year.HasValue)
                query = query.Where(p =>
                    (p.YearFrom == null || p.YearFrom <= request.Year) &&
                    (p.YearTo == null || p.YearTo >= request.Year));

            // فلترة بالحالة
            if (request.ConditionId.HasValue)
            {
                var condition = await _context.PartConditions.FindAsync(request.ConditionId.Value);
                if (condition != null)
                {
                    query = query.Where(p => p.Condition == condition.Key);
                }
            }

            // فلترة بالسعر
            if (request.MinPrice.HasValue)
                query = query.Where(p => p.Price >= request.MinPrice);
            if (request.MaxPrice.HasValue)
                query = query.Where(p => p.Price <= request.MaxPrice);

            // فلترة بالمدينة
            if (request.CityId.HasValue)
            {
                var city = await _context.Cities.FindAsync(request.CityId.Value);
                if (city != null)
                {
                    query = query.Where(p => p.SupplierCity == city.NameAr);
                }
            }

            // فلترة بالضمان
            if (request.HasWarranty.HasValue && request.HasWarranty.Value)
                query = query.Where(p => p.WarrantyType != null && p.WarrantyType != "none");

            // فلترة بالتوصيل
            if (request.DeliveryAvailable.HasValue && request.DeliveryAvailable.Value)
                query = query.Where(p => p.DeliveryAvailable);

            // === حساب الفلاتر المتاحة ===
            var allResults = await query.ToListAsync();

            var availableFilters = new AvailableFiltersDto
            {
                // التصنيفات
                Categories = allResults
                    .Where(p => p.CategoryId != null)
                    .GroupBy(p => new { p.CategoryId, p.CategoryNameAr })
                    .Select(g => new FilterItemDto
                    {
                        Id = (int?)g.Key.CategoryId,
                        Name = g.Key.CategoryNameAr,
                        Count = g.Count()
                    })
                    .OrderByDescending(f => f.Count)
                    .ToList(),

                // التصنيفات الفرعية
                Subcategories = allResults
                    .Where(p => p.VehicleSubcategoryId != null)
                    .GroupBy(p => new { p.VehicleSubcategoryId, p.SubcategoryNameAr })
                    .Select(g => new FilterItemDto
                    {
                        Id = g.Key.VehicleSubcategoryId,
                        Name = g.Key.SubcategoryNameAr,
                        Count = g.Count()
                    })
                    .OrderByDescending(f => f.Count)
                    .ToList(),

                // أنواع المركبات
                VehicleTypes = allResults
                    .Where(p => p.VehicleTypeId != null)
                    .GroupBy(p => new { p.VehicleTypeId, p.VehicleTypeNameAr })
                    .Select(g => new FilterItemDto
                    {
                        Id = g.Key.VehicleTypeId,
                        Name = g.Key.VehicleTypeNameAr,
                        Count = g.Count()
                    })
                    .OrderByDescending(f => f.Count)
                    .ToList(),

                // الشركات المصنعة
                Makes = allResults
                    .Where(p => p.MakeId != null)
                    .GroupBy(p => new { p.MakeId, p.MakeNameAr })
                    .Select(g => new FilterItemDto
                    {
                        Id = g.Key.MakeId,
                        Name = g.Key.MakeNameAr,
                        Count = g.Count()
                    })
                    .OrderByDescending(f => f.Count)
                    .ToList(),

                // الموديلات
                Models = allResults
                    .Where(p => p.ModelId != null)
                    .GroupBy(p => new { p.ModelId, p.ModelNameAr })
                    .Select(g => new FilterItemDto
                    {
                        Id = g.Key.ModelId,
                        Name = g.Key.ModelNameAr,
                        Count = g.Count()
                    })
                    .OrderByDescending(f => f.Count)
                    .ToList(),

                // السنوات
                Years = allResults
                    .Where(p => p.YearFrom != null)
                    .SelectMany(p => Enumerable.Range(p.YearFrom ?? 0, (p.YearTo ?? p.YearFrom ?? 0) - (p.YearFrom ?? 0) + 1))
                    .GroupBy(y => y)
                    .Select(g => new FilterItemDto
                    {
                        Value = g.Key.ToString(),
                        Name = g.Key.ToString(),
                        Count = g.Count()
                    })
                    .OrderByDescending(f => int.Parse(f.Value ?? "0"))
                    .Take(20)
                    .ToList(),

                // الحالات
                Conditions = allResults
                    .Where(p => p.Condition != null)
                    .GroupBy(p => p.Condition)
                    .Select(g => new FilterItemDto
                    {
                        Value = g.Key,
                        Name = GetConditionAr(g.Key),
                        Count = g.Count()
                    })
                    .OrderByDescending(f => f.Count)
                    .ToList(),

                // نطاق السعر
                PriceRange = allResults.Any() ? new PriceRangeDto
                {
                    Min = allResults.Min(p => p.Price),
                    Max = allResults.Max(p => p.Price)
                } : null
            };

            // === الترتيب ===
            var sortedQuery = request.SortBy switch
            {
                "price_asc" => allResults.OrderBy(p => p.Price),
                "price_desc" => allResults.OrderByDescending(p => p.Price),
                "popular" => allResults.OrderByDescending(p => p.ViewsCount),
                "newest" => allResults.OrderByDescending(p => p.CreatedAt),
                _ => allResults.OrderByDescending(p => p.CreatedAt)
            };

            // === الصفحات ===
            var totalItems = allResults.Count;
            var totalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize);

            var parts = sortedQuery
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var partIds = parts.Select(p => p.Id).ToList();
            var allImages = await _context.PartImages
                .Where(i => partIds.Contains(i.PartId))
                .OrderBy(i => i.DisplayOrder)
                .ToListAsync();

            var partsDto = parts.Select(p =>
            {
                var images = allImages.Where(i => i.PartId == p.Id).ToList();
                return MapViewToDto(p, images);
            }).ToList();

            return new SearchPartsResponse
            {
                Success = true,
                Parts = partsDto,
                Pagination = new PaginationInfo
                {
                    CurrentPage = request.Page,
                    PageSize = request.PageSize,
                    TotalItems = totalItems,
                    TotalPages = totalPages,
                    HasNext = request.Page < totalPages,
                    HasPrevious = request.Page > 1
                },
                AvailableFilters = availableFilters
            };
        }

        private string GetConditionAr(string? condition) 
        {
            return condition?.ToLower() switch
            {
                "new" => "جديد",
                "used" => "مستعمل",
                "refurbished" => "مجدد",
                _ => condition ?? ""
            };
        }

        #endregion

        #region Helper Methods

        private PartDto MapViewToDto(VPartsDetailed view, List<PartImage> images)
        {
            return new PartDto
            {
                Id = view.Id,
                SupplierId = view.SupplierId,
                SupplierName = view.SupplierName,
                City = view.SupplierCity,

                NameAr = view.NameAr,
                NameEn = view.NameEn,
                Description = view.Description,
                PartNumber = view.PartNumber,
                OemNumber = view.OemNumber,
                VinNumber = view.VinNumber,

                Condition = view.Condition,
                ConditionAr = GetConditionAr(view.Condition),
                ConditionDetails = view.ConditionDetails,
                WarrantyType = view.WarrantyType,
                WarrantyTypeAr = GetWarrantyTypeAr(view.WarrantyType),
                WarrantyDays = view.WarrantyDays,

                Price = view.Price,
                OriginalPrice = view.OriginalPrice,
                Currency = view.Currency,
                DiscountPercent = view.OriginalPrice.HasValue && view.OriginalPrice > 0
                    ? (int)Math.Round((1 - (view.Price / view.OriginalPrice.Value)) * 100)
                    : null,

                Quantity = view.Quantity,
                Status = view.Status,
                IsAvailable = view.Status == "available" && view.Quantity > 0,

                CategoryId = view.CategoryId,
                CategoryNameAr = view.CategoryNameAr,
                CategoryNameEn = view.CategoryNameEn,
                CustomCategory = view.CustomCategory,

                VehicleTypeId = view.VehicleTypeId,
                VehicleTypeNameAr = view.VehicleTypeNameAr,
                VehicleTypeNameEn = view.VehicleTypeNameEn,
                CustomVehicleType = view.CustomVehicleType,

                VehicleSubcategoryId = view.VehicleSubcategoryId,
                SubcategoryNameAr = view.SubcategoryNameAr,
                SubcategoryNameEn = view.SubcategoryNameEn,
                CustomSubcategory = view.CustomSubcategory,

                MakeId = view.MakeId,
                MakeNameAr = view.MakeNameAr,
                MakeNameEn = view.MakeNameEn,
                MakeLogoUrl = view.MakeLogoUrl,
                CustomMake = view.CustomMake,

                ModelId = view.ModelId,
                ModelNameAr = view.ModelNameAr,
                ModelNameEn = view.ModelNameEn,
                CustomModel = view.CustomModel,

                YearFrom = view.YearFrom,
                YearTo = view.YearTo,
                YearRange = GetYearRange(view.YearFrom, view.YearTo),

                DeliveryAvailable = view.DeliveryAvailable,
                DeliveryByShop = view.DeliveryByShop,
                DeliveryNotes = view.DeliveryNotes,

                ViewsCount = view.ViewsCount,
                SalesCount = view.SalesCount,
                FavoritesCount = view.FavoritesCount,

                PrimaryImageUrl = view.PrimaryImageUrl ?? images.FirstOrDefault(i => i.IsPrimary)?.ImageUrl ?? images.FirstOrDefault()?.ImageUrl,
                Images = images.Select(i => new PartImageDto
                {
                    Id = i.Id,
                    ImageUrl = i.ImageUrl,
                    ThumbnailUrl = i.ThumbnailUrl,
                    IsPrimary = i.IsPrimary,
                    DisplayOrder = i.DisplayOrder
                }).ToList(),

                CreatedAt = view.CreatedAt,
                UpdatedAt = view.UpdatedAt
            };
        }

       

        private string? GetWarrantyTypeAr(string? warrantyType)
        {
            return warrantyType switch
            {
                "none" => "بدون ضمان",
                "week" => "أسبوع",
                "two_weeks" => "أسبوعين",
                "month" => "شهر",
                "two_months" => "شهرين",
                "three_months" => "3 أشهر",
                "six_months" => "6 أشهر",
                "year" => "سنة",
                _ => warrantyType
            };
        }

        private string? GetYearRange(short? yearFrom, short? yearTo)
        {
            return (yearFrom, yearTo) switch
            {
                (null, null) => null,
                (short from, null) => $"{from}+",
                (null, short to) => $"حتى {to}",
                (short from, short to) when from == to => from.ToString(),
                (short from, short to) => $"{from} - {to}"
            };
        }

        /// <summary>
        /// جلب IDs القطع المسموح بعرضها لكل مورد حسب حد الباقة
        /// </summary>
        private async Task<List<long>> GetAllowedPartIdsAsync()
        {
            var now = DateTime.UtcNow;

            // جيب كل الموردين اللي عندهم اشتراك فعال مع حد القطع
            var activeSubscriptions = await _context.Subscriptions
                .Include(s => s.Plan)
                .Where(s => s.Status == "active" && s.EndsAt >= now)  // ✅ تم التعديل
                .Select(s => new { s.SupplierId, MaxParts = s.Plan.MaxParts })
                .ToListAsync();

            var allowedPartIds = new List<long>();

            foreach (var sub in activeSubscriptions)
            {
                // جيب أحدث X قطعة لكل مورد
                var supplierPartIds = await _context.Parts
                    .Where(p => p.SupplierId == sub.SupplierId && p.Status == "available")
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(sub.MaxParts ?? 15)
                    .Select(p => p.Id)
                    .ToListAsync();

                allowedPartIds.AddRange(supplierPartIds);
            }

            return allowedPartIds;
        }

        #endregion
    }
}