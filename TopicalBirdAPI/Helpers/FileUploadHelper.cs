using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;

namespace TopicalBirdAPI.Helpers
{
    public static class FileUploadHelper
    {
        // private readonly static string[] ALLOWED_EXTENSIONS = { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".heic", ".avif", ".bmp" };
        private readonly static int MAX_SIZE_MB = 10;
        private readonly static long MAX_SIZE_BYTES = MAX_SIZE_MB * 1024 * 1024;

        public async static Task<string> SaveFile(IFormFile file, string folderPath, string filePrefix)
        {
            if (file == null || file.Length == 0)
            {
                return string.Empty;
            }

            if (file.Length > MAX_SIZE_BYTES) {
                throw new InvalidDataException("File size too big");
            }

            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            var imageInfo = await Image.IdentifyAsync(memoryStream);

            if (imageInfo == null)
            {
                throw new InvalidDataException("File was not a recognized image format.");
            }

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var fileName = $"{filePrefix}_{DateTime.UtcNow.Ticks}.webp";
            var filePath = Path.Combine(folderPath, fileName);

            memoryStream.Position = 0;
            using (var image = await Image.LoadAsync(memoryStream))
            {
                var encoder = new WebpEncoder
                {
                    Quality = 75,
                    FileFormat = WebpFileFormatType.Lossy
                };

                await image.SaveAsync(filePath, encoder);
            }

            return filePath.Replace("\\", "/").Replace("wwwroot", "");
        }

        public static bool DeleteFile(string filePath)
        {
            var fullPath = Path.Combine("wwwroot", filePath.TrimStart('/')); // ???
            bool stringIsNull = string.IsNullOrEmpty(fullPath);
            bool fileExists = File.Exists(fullPath);
            if (stringIsNull || !fileExists)
            {
                return false;
            }

            File.Delete(fullPath);
            return true;
        }
    }
}
