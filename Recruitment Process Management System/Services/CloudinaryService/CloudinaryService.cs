using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace Recruitment_Process_Management_System.Services.CloudinaryService
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;
        private readonly IConfiguration _configuration;

        public CloudinaryService(IConfiguration configuration)
        {
            _configuration = configuration;

            var cloudName = _configuration["Cloudinary:CloudName"];
            var apiKey = _configuration["Cloudinary:ApiKey"];
            var apiSecret = _configuration["Cloudinary:ApiSecret"];

            if (string.IsNullOrEmpty(cloudName) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret))
            {
                throw new InvalidOperationException("Cloudinary configuration is missing. Please check appsettings.json");
            }

            var account = new Account(cloudName, apiKey, apiSecret);
            _cloudinary = new Cloudinary(account);
        }

        /// <summary>
        /// Upload resume file to Cloudinary
        /// </summary>
        public async Task<string> UploadResumeAsync(Stream fileStream, string fileName)
        {
            try
            {
                // Create unique public ID with timestamp
                var publicId = $"resumes/{DateTime.UtcNow:yyyy-MM-dd}/{Guid.NewGuid()}_{Path.GetFileNameWithoutExtension(fileName)}";

                var uploadParams = new RawUploadParams
                {
                    File = new FileDescription(fileName, fileStream),
                    PublicId = publicId,
                    Overwrite = true,
                    // Optional: Add tags for organization
                    Tags = "resume,candidate",
                    // Optional: Set access mode (token = requires authentication)
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.Error != null)
                {
                    throw new Exception($"Cloudinary upload failed: {uploadResult.Error.Message}");
                }

                // Return the secure URL
                return uploadResult.SecureUrl.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to upload resume to Cloudinary: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Delete resume file from Cloudinary
        /// </summary>
        public async Task<bool> DeleteResumeAsync(string publicId)
        {
            try
            {
                if (string.IsNullOrEmpty(publicId))
                    return false;

                var deleteParams = new DeletionParams(publicId)
                {
                    ResourceType = ResourceType.Auto
                };

                var deleteResult = await _cloudinary.DestroyAsync(deleteParams);

                return deleteResult.Result == "ok";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to delete resume from Cloudinary: {ex.Message}");
                return false;
            }
        }
    }
}
