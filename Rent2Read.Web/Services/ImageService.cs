using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Rent2Read.Web.Services
{
    public class ImageService(IWebHostEnvironment _webHostEnvironment) : IImageService
    {
        //private =>encapsulation
        private readonly List<string> _allowedExtensions = new() { ".png", ".jpg", ".jpeg" };// is used to store the allowed file extensions and size
        private readonly int _maxAllowedSize = 2097152;//2MB
        public async Task<(bool isUploaded, string? errorMessage)> UploadAsync(IFormFile image, string imageName, string folderPath, bool hasThumbnail)
        //The tuple is made to return more than one value at the same time from the method.
        {
            var extension = Path.GetExtension(image.FileName);

            if (!_allowedExtensions.Contains(extension))
                return (isUploaded: false, errorMessage: Errors.NotAllowedExtension);

            if (image.Length > _maxAllowedSize)
                return (isUploaded: false, errorMessage: Errors.MaxSize);

            var path = Path.Combine($"{_webHostEnvironment.WebRootPath}{folderPath}", imageName);
            // WebRootPath = This gives you the project's wwwroot folder (the place where I store images and files that you can access from the browser).

            using var stream = File.Create(path);
            await image.CopyToAsync(stream);
            stream.Dispose();

            if (hasThumbnail)
            {
                var thumbPath = Path.Combine($"{_webHostEnvironment.WebRootPath}{folderPath}/thumb", imageName);

                // Image in ImageSharp is the core class that represents an image in memory.
                // You can load, manipulate, and save images with it.
                //The Image class is IDisposable, which means you have to dispose it or put it inside a using block to free up memory.
                using var loadedImage = Image.Load(image.OpenReadStream());
                var ratio = (float)loadedImage.Width / 200;
                var height = loadedImage.Height / ratio;
                loadedImage.Mutate(i => i.Resize(width: 200, height: (int)height));
                loadedImage.Save(thumbPath);
            }

            return (isUploaded: true, errorMessage: null);
        }


        public void Delete(string imagePath, string? imageThumbnailPath = null)
        {
            var oldImagePath = $"{_webHostEnvironment.WebRootPath}{imagePath}";
            //make sure there is actually file with this image in this location.
            //So that it doesn't try to delete something that doesn't exist(cause an exception).

            if (File.Exists(oldImagePath))
                File.Delete(oldImagePath);
            if (!string.IsNullOrEmpty(imageThumbnailPath))
            {
                var oldThumbPath = $"{_webHostEnvironment.WebRootPath}{imageThumbnailPath}";

                if (File.Exists(oldThumbPath))
                    File.Delete(oldThumbPath);
            }


        }

    }
}
