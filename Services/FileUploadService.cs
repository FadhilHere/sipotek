using Microsoft.AspNetCore.Components.Forms;

namespace SIPOTEK.Services
{
    public interface IFileUploadService
    {
        Task<string?> UploadImageAsync(IBrowserFile file);
        Task<bool> DeleteImageAsync(string fileName);
        string GetImageUrl(string fileName);
    }

    public class FileUploadService : IFileUploadService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly string _uploadPath;

        public FileUploadService(IWebHostEnvironment environment)
        {
            _environment = environment;
            _uploadPath = Path.Combine(_environment.WebRootPath, "uploads", "obat");

            // Buat direktori jika belum ada
            if (!Directory.Exists(_uploadPath))
            {
                Directory.CreateDirectory(_uploadPath);
            }
        }

        public async Task<string?> UploadImageAsync(IBrowserFile file)
        {
            try
            {
                // Validasi file
                if (file == null || file.Size == 0)
                    return null;

                // Validasi tipe file
                var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
                if (!allowedTypes.Contains(file.ContentType))
                    throw new Exception("Hanya file gambar (JPEG, PNG, GIF) yang diperbolehkan");

                // Validasi ukuran file (max 5MB)
                if (file.Size > 5 * 1024 * 1024)
                    throw new Exception("Ukuran file tidak boleh lebih dari 5MB");

                // Generate nama file unik
                var fileExtension = Path.GetExtension(file.Name);
                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(_uploadPath, fileName);

                // Upload file
                using var stream = new FileStream(filePath, FileMode.Create);
                await file.OpenReadStream(maxAllowedSize: 5 * 1024 * 1024).CopyToAsync(stream);

                return fileName;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error uploading file: {ex.Message}");
            }
        }

        public async Task<bool> DeleteImageAsync(string fileName)
        {
            try
            {
                if (string.IsNullOrEmpty(fileName))
                    return false;

                var filePath = Path.Combine(_uploadPath, fileName);
                if (File.Exists(filePath))
                {
                    await Task.Run(() => File.Delete(filePath));
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public string GetImageUrl(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return "/images/no-image.png"; // Default image

            return $"/uploads/obat/{fileName}";
        }
    }
}