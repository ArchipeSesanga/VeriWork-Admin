using Firebase.Storage;
using Microsoft.AspNetCore.Http;

namespace VeriWork_Admin.Application.Services
{
    public class FirebaseStorageService
    {
        private readonly string _projectId;
        private readonly string _bucketName;
        private readonly string _credentialPath;

        public FirebaseStorageService(string projectId, string bucketName, string credentialPath)
        {
            _projectId = projectId;
            _bucketName = bucketName;
            _credentialPath = credentialPath;
        }

        /// <summary>
        /// Uploads a file to Firebase Storage and returns its public URL.
        /// Automatically organizes files under /uploads/{year}/{month}/
        /// </summary>
        public async Task<string> UploadFileAsync(IFormFile file, string fileName)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("Invalid file.");

            try
            {
                // Organize by year/month folders
                string folderPath = $"uploads/{DateTime.UtcNow:yyyy}/{DateTime.UtcNow:MM}/";
                string fullFilePath = $"{folderPath}{fileName}";

                using var stream = file.OpenReadStream();

                var storage = new FirebaseStorage(
                    _bucketName,
                    new FirebaseStorageOptions
                    {
                        AuthTokenAsyncFactory = () => Task.FromResult<string>(null),
                        ThrowOnCancel = true
                    }
                );

                // Upload file to Firebase
                var task = storage
                    .Child(fullFilePath)
                    .PutAsync(stream);

                string downloadUrl = await task;
                return downloadUrl;
            }
            catch (Exception ex)
            {
                throw new Exception($"File upload failed: {ex.Message}");
            }
        }
        
        public async Task<string> UploadAuditPdfAsync(string filePath, string month)
        {
            var fileName = $"audit-reports/{month}/AuditLogs_{month}.pdf";

            // Read file into a stream, then wrap it into a FormFile to match UploadFileAsync signature
            using var stream = File.OpenRead(filePath);
            var formFile = new FormFile(stream, 0, stream.Length, "file", Path.GetFileName(filePath));

            return await UploadFileAsync(formFile, fileName);
        }


    }
}