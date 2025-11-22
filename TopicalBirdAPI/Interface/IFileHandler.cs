namespace TopicalBirdAPI.Interface
{
    public interface IFileHandler
    {
        Task<string> SaveFile(IFormFile file, string folderPath, string filePrefix);
        bool DeleteFile(string filePath);
    }

}
