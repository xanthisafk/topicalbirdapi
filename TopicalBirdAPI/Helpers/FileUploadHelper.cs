using Microsoft.AspNetCore.Mvc;

namespace TopicalBirdAPI.Helpers
{
    public static class FileUploadHelper
    {
        private readonly static string[] ALLOWED_EXTENSIONS = { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".heic", ".avif", ".bmp" };
        private readonly static int MAX_SIZE_MB = 10;
        private readonly static long MAX_SIZE_BYTES = MAX_SIZE_MB * 1024 * 1024;

        public async static Task<string> SaveFile(IFormFile file, string folderPath, string filePrefix)
        {
            if (file == null || file.Length == 0)
            {
                return string.Empty;
            }
            
            var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            if (!file.ContentType.Contains("image/") || !ALLOWED_EXTENSIONS.Contains(extension))
            {
                throw new InvalidDataException("File was not an image");
            }

            if (file.Length > MAX_SIZE_BYTES) {
                throw new InvalidDataException("File size too big");
            }

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var fileName = $"{filePrefix}_{DateTime.UtcNow.Ticks}{extension}";
            var filePath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return filePath.Replace("\\", "/").Replace("wwwroot", ""); // for Windows path handling
        }

        public static bool DeleteFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                return false;
            }

            File.Delete(filePath);
            return true;
        }
    }
}
