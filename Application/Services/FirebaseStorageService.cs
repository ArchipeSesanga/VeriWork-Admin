using Google.Cloud.Storage.V1;

namespace VeriWork_Admin.Application.Services;

public class FirebaseStorageService
{
    private readonly StorageClient _storageClient;
    private readonly string _bucketName;

    public FirebaseStorageService(string projectId, string bucketName, string credentialsPath)
    {
        Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialsPath);
        _storageClient = StorageClient.Create();
        _bucketName = bucketName;
    }

    public async Task<string> UploadFileAsync(IFormFile file, string fileName)
    {
        using var stream = file.OpenReadStream();
        await _storageClient.UploadObjectAsync(_bucketName, fileName, file.ContentType, stream);

        return $"https://firebasestorage.googleapis.com/v0/b/{_bucketName}/o/{Uri.EscapeDataString(fileName)}?alt=media";
    }
}