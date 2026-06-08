namespace TwentyOne.Web.API.Services
{
    public class ImageUploadService
    {
        private readonly IWebHostEnvironment _environment;

        public ImageUploadService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string?> UploadImageAsync(IFormFile file)
        {
            // Validate file type
            var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
            if (!allowedTypes.Contains(file.ContentType.ToLower()))
                return null;

            // Validate file size (max 5MB)
            if (file.Length > 5 * 1024 * 1024)
                return null;

            // Generate unique filename
            var extension = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid()}{extension}";

            // Save to wwwroot/uploads/products/
            var uploadPath = Path.Combine(
                _environment.WebRootPath, "uploads", "products");

            // Create directory if it doesn't exist
            Directory.CreateDirectory(uploadPath);

            var filePath = Path.Combine(uploadPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return the URL path
            return $"/uploads/products/{fileName}";
        }

        public async Task<string?> UploadBrandLogoAsync(IFormFile file)
        {
            // Validate file type
            var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
            if (!allowedTypes.Contains(file.ContentType.ToLower()))
                return null;

            // Validate file size (max 2MB for logos)
            if (file.Length > 2 * 1024 * 1024)
                return null;

            // Generate unique filename
            var extension = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid()}{extension}";

            // Save to wwwroot/uploads/brands/
            var uploadPath = Path.Combine(
                _environment.WebRootPath, "uploads", "brands");

            Directory.CreateDirectory(uploadPath);

            var filePath = Path.Combine(uploadPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/brands/{fileName}";
        }

        public void DeleteImage(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl)) return;

            var filePath = Path.Combine(
                _environment.WebRootPath,
                imageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }
}
