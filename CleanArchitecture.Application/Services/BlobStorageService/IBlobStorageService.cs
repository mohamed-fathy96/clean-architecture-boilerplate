using Microsoft.AspNetCore.Http;

namespace CleanArchitecture.Application.Services.BlobStorageService;

public interface IBlobStorageService
{
    Task<string> UploadFormFile(IFormFile file, string subDirectory, string fileName = "",
        int retryCount = 3);

    Task<bool> DeleteFile(string subDirectory, string fileUrl);

    Task<string> UploadBinary(byte[] bytes, string contentType, string fileName, string subDirectory = "",
        int retryCount = 3);
}
