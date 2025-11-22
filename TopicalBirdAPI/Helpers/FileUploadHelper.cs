using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using System.Text;
using TopicalBirdAPI.Interface;

namespace TopicalBirdAPI.Helpers
{
    public class LocalFileHandler : IFileHandler
    {
        private readonly int MAX_SIZE_MB = 10;
        private long MaxSizeBytes => MAX_SIZE_MB * 1024 * 1024;

        public async Task<string> SaveFile(IFormFile file, string folderPath, string filePrefix)
        {
            if (file == null || file.Length == 0)
                return string.Empty;

            if (file.Length > MaxSizeBytes)
                throw new InvalidDataException("File size too big");

            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            var imageInfo = await Image.IdentifyAsync(memoryStream);
            if (imageInfo == null)
                throw new InvalidDataException("File was not a recognized image format.");

            if (file.ContentType == "image/webp" && await IsAnimatedWebP(file))
                throw new InvalidDataException("Animated WEBP files are not supported.");

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

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

        public bool DeleteFile(string filePath)
        {
            var fullPath = Path.Combine("wwwroot", filePath.TrimStart('/'));

            if (string.IsNullOrEmpty(fullPath) || !File.Exists(fullPath))
                return false;

            File.Delete(fullPath);
            return true;
        }

        private static async Task<bool> IsAnimatedWebP(IFormFile file)
        {
            byte[] header = new byte[64];
            using var stream = file.OpenReadStream();
            int bytesRead = await stream.ReadAsync(header, 0, header.Length);

            if (bytesRead < 20)
                return false;

            string headerText = Encoding.ASCII.GetString(header);
            return headerText.Contains("ANIM", StringComparison.Ordinal);
        }
    }
}
