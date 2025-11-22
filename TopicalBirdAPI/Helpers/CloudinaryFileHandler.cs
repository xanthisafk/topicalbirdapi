using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using TopicalBirdAPI.Interface;

namespace TopicalBirdAPI.Helpers
{
    public class CloudinaryFileHandler : IFileHandler
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryFileHandler(IConfiguration config)
        {
            var cloud = config["CLOUDINARY_CLOUD"];
            var apiKey = config["CLOUDINARY_KEY"];
            var apiSecret = config["CLOUDINARY_SECRET"];

            _cloudinary = new Cloudinary(new Account(cloud, apiKey, apiSecret));
        }

        public async Task<string> SaveFile(IFormFile file, string folderPath, string filePrefix)
        {
            using var stream = file.OpenReadStream();

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = folderPath,
                PublicId = $"{filePrefix}_{DateTime.UtcNow.Ticks}",
                Transformation = new Transformation().Quality("auto")
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.Error != null)
                throw new Exception(uploadResult.Error.Message);

            return uploadResult.SecureUrl.ToString();
        }

        public bool DeleteFile(string publicUrlOrId)
        {
            // You probably store the public_id; adjust as needed
            var id = Path.GetFileNameWithoutExtension(publicUrlOrId);

            var deletion = _cloudinary.Destroy(new DeletionParams(id));
            return deletion.Result == "ok";
        }
    }
}
