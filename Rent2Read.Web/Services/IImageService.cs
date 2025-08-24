namespace Rent2Read.Web.Services
{
    public interface IImageService
    {
        Task<(bool isUploaded, string? errorMessage)> UploadAsync(IFormFile image, string imageName, string folderPath, bool hasThumbnail);
        public void Delete(string imagePath, string? imageThumbnailPath = null);
    }
}
