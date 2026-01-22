using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Tashlih.Application.Interfaces
{
    public interface IFileService
    {
        /// <summary>
        /// رفع ملف وإرجاع الـ URL
        /// </summary>
        /// <param name="file">الملف المراد رفعه</param>
        /// <param name="folder">المجلد الفرعي (مثل: suppliers/identity)</param>
        /// <returns>URL الملف</returns>
        Task<string> UploadFileAsync(IFormFile file, string folder);
        Task<string> UploadDocumentAsync(IFormFile file, string folder);
        Task<string> UploadImageAsync(IFormFile file, string folder);
        Task<string> UploadMediaAsync(IFormFile file, string folder);

        /// <summary>
        /// حذف ملف
        /// </summary>
        /// <param name="fileUrl">URL الملف</param>
        /// <returns>نجاح أو فشل</returns>
        Task<bool> DeleteFileAsync(string fileUrl);

        /// <summary>
        /// التحقق من نوع الملف
        /// </summary>
        bool IsValidFileType(IFormFile file, string[] allowedExtensions);

        /// <summary>
        /// التحقق من حجم الملف
        /// </summary>
        bool IsValidFileSize(IFormFile file, long maxSizeInBytes);
    }
}