namespace Recruitment_Process_Management_System.Services.CloudinaryService
{
    public interface ICloudinaryService
    {
        Task<string> UploadResumeAsync(Stream fileStream, string fileName);
        Task<bool> DeleteResumeAsync(string publicId);
    }
}
