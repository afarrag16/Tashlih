using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Tashlih.Application.Interfaces;

namespace Tashlih.Infrastructure.Services
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;
        private readonly string[] _allowedImageExtensions = { ".jpg", ".jpeg", ".png" };
        private readonly string[] _allowedDocumentExtensions = { ".jpg", ".jpeg", ".png", ".pdf" };
        private readonly string[] _allowedMediaExtensions = { ".jpg", ".jpeg", ".png", ".mp4", ".mov", ".mkv", ".avi" };
        private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB

        public FileService(IWebHostEnvironment environment, IConfiguration configuration)
        {
            _environment = environment;
            _configuration = configuration;
        }
        public async Task<string> UploadFileAsync(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("الملف فارغ أو غير موجود");

            if (!IsValidFileType(file, _allowedDocumentExtensions))
                throw new ArgumentException("نوع الملف غير مسموح");

            if (!IsValidFileSize(file, MaxFileSize))
                throw new ArgumentException("حجم الملف يتجاوز الحد المسموح (5 ميجابايت)");

            return await SaveFileAsync(file, folder);
        }
        // للمستندات (التسجيل - صور + PDF)
        public async Task<string> UploadDocumentAsync(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("الملف فارغ أو غير موجود");

            if (!IsValidFileType(file, _allowedDocumentExtensions))
                throw new ArgumentException("نوع الملف غير مسموح. الأنواع المسموحة: jpg, jpeg, png, pdf");

            if (!IsValidFileSize(file, MaxFileSize))
                throw new ArgumentException("حجم الملف يتجاوز الحد المسموح (5 ميجابايت)");

            return await SaveFileAsync(file, folder);
        }

        // للصور فقط (اللوجو)
        public async Task<string> UploadImageAsync(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("الملف فارغ أو غير موجود");

            if (!IsValidFileType(file, _allowedImageExtensions))
                throw new ArgumentException("نوع الملف غير مسموح. الأنواع المسموحة: jpg, jpeg, png");

            if (!IsValidFileSize(file, MaxFileSize))
                throw new ArgumentException("حجم الملف يتجاوز الحد المسموح (5 ميجابايت)");

            return await SaveFileAsync(file, folder);
        }

        // للميديا (المنتجات - صور + فيديو)
        public async Task<string> UploadMediaAsync(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("الملف فارغ أو غير موجود");

            if (!IsValidFileType(file, _allowedMediaExtensions))
                throw new ArgumentException("نوع الملف غير مسموح. الأنواع المسموحة: jpg, jpeg, png, mp4, mov, mkv, avi");

            if (!IsValidFileSize(file, MaxFileSize))
                throw new ArgumentException("حجم الملف يتجاوز الحد المسموح (5 ميجابايت)");

            return await SaveFileAsync(file, folder);
        }

        // Method مشتركة لحفظ الملف
        private async Task<string> SaveFileAsync(IFormFile file, string folder)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";

            var uploadsFolder = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, "uploads", folder);

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/{folder}/{uniqueFileName}";
        }

        public async Task<bool> DeleteFileAsync(string fileUrl)
        {
            if (string.IsNullOrEmpty(fileUrl))
                return false;

            try
            {
                // إزالة الـ Base URL لو موجود
                var baseUrl = _configuration["AppSettings:BaseUrl"]?.TrimEnd('/') ?? "";
                var relativePath = fileUrl.Replace(baseUrl, "").TrimStart('/');

                var fullPath = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, relativePath);

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public bool IsValidFileType(IFormFile file, string[] allowedExtensions)
        {
            if (file == null)
                return false;

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            return allowedExtensions.Contains(extension);
        }

        public bool IsValidFileSize(IFormFile file, long maxSizeInBytes)
        {
            if (file == null)
                return false;

            return file.Length <= maxSizeInBytes;
        }
    }
}