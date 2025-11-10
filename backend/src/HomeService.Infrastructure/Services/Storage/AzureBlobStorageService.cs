using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using HomeService.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HomeService.Infrastructure.Services.Storage;

public class AzureBlobStorageService : IFileStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly ILogger<AzureBlobStorageService> _logger;
    private readonly string _containerName;

    public AzureBlobStorageService(
        IConfiguration configuration,
        ILogger<AzureBlobStorageService> logger)
    {
        var connectionString = configuration["AzureStorage:ConnectionString"]
            ?? throw new InvalidOperationException("Azure Storage connection string is not configured");

        _containerName = configuration["AzureStorage:ContainerName"] ?? "files";
        _blobServiceClient = new BlobServiceClient(connectionString);
        _logger = logger;

        // Ensure container exists
        EnsureContainerExistsAsync().GetAwaiter().GetResult();
    }

    private async Task EnsureContainerExistsAsync()
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.None);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ensuring blob container exists");
        }
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, string folder = "uploads")
    {
        try
        {
            // Generate unique file name to avoid conflicts
            var uniqueFileName = $"{folder}/{Guid.NewGuid()}_{SanitizeFileName(fileName)}";

            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(uniqueFileName);

            // Upload with metadata
            var blobHttpHeaders = new BlobHttpHeaders
            {
                ContentType = contentType
            };

            await blobClient.UploadAsync(fileStream, new BlobUploadOptions
            {
                HttpHeaders = blobHttpHeaders
            });

            var url = blobClient.Uri.ToString();
            _logger.LogInformation("File uploaded successfully: {FileName} to {Url}", fileName, url);

            return url;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file: {FileName}", fileName);
            throw;
        }
    }

    public async Task<List<string>> UploadFilesAsync(List<(Stream stream, string fileName, string contentType)> files, string folder = "uploads")
    {
        var urls = new List<string>();

        foreach (var file in files)
        {
            try
            {
                var url = await UploadFileAsync(file.stream, file.fileName, file.contentType, folder);
                urls.Add(url);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file: {FileName}", file.fileName);
                // Continue with other files even if one fails
            }
        }

        return urls;
    }

    public async Task<bool> DeleteFileAsync(string fileUrl)
    {
        try
        {
            var blobName = GetBlobNameFromUrl(fileUrl);
            if (string.IsNullOrEmpty(blobName))
                return false;

            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            var result = await blobClient.DeleteIfExistsAsync();

            if (result.Value)
            {
                _logger.LogInformation("File deleted successfully: {FileUrl}", fileUrl);
            }

            return result.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {FileUrl}", fileUrl);
            return false;
        }
    }

    public async Task<bool> DeleteFilesAsync(List<string> fileUrls)
    {
        var allDeleted = true;

        foreach (var url in fileUrls)
        {
            var deleted = await DeleteFileAsync(url);
            if (!deleted)
                allDeleted = false;
        }

        return allDeleted;
    }

    public async Task<string> GetDownloadUrlAsync(string fileUrl, int expiryMinutes = 60)
    {
        try
        {
            var blobName = GetBlobNameFromUrl(fileUrl);
            if (string.IsNullOrEmpty(blobName))
                return fileUrl;

            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            // Check if we can generate SAS token
            if (!blobClient.CanGenerateSasUri)
            {
                _logger.LogWarning("Cannot generate SAS token for blob: {BlobName}", blobName);
                return fileUrl;
            }

            // Generate SAS token
            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = _containerName,
                BlobName = blobName,
                Resource = "b",
                ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(expiryMinutes)
            };

            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            var sasUri = blobClient.GenerateSasUri(sasBuilder);
            return sasUri.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating download URL for: {FileUrl}", fileUrl);
            return fileUrl; // Return original URL if SAS generation fails
        }
    }

    public async Task<bool> FileExistsAsync(string fileUrl)
    {
        try
        {
            var blobName = GetBlobNameFromUrl(fileUrl);
            if (string.IsNullOrEmpty(blobName))
                return false;

            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            return await blobClient.ExistsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if file exists: {FileUrl}", fileUrl);
            return false;
        }
    }

    public async Task<long> GetFileSizeAsync(string fileUrl)
    {
        try
        {
            var blobName = GetBlobNameFromUrl(fileUrl);
            if (string.IsNullOrEmpty(blobName))
                return 0;

            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            var properties = await blobClient.GetPropertiesAsync();
            return properties.Value.ContentLength;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file size: {FileUrl}", fileUrl);
            return 0;
        }
    }

    public async Task<string> CopyFileAsync(string sourceUrl, string destinationFolder)
    {
        try
        {
            var sourceBlobName = GetBlobNameFromUrl(sourceUrl);
            if (string.IsNullOrEmpty(sourceBlobName))
                throw new ArgumentException("Invalid source URL");

            var fileName = Path.GetFileName(sourceBlobName);
            var destinationBlobName = $"{destinationFolder}/{Guid.NewGuid()}_{fileName}";

            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var sourceBlobClient = containerClient.GetBlobClient(sourceBlobName);
            var destinationBlobClient = containerClient.GetBlobClient(destinationBlobName);

            await destinationBlobClient.StartCopyFromUriAsync(sourceBlobClient.Uri);

            _logger.LogInformation("File copied from {Source} to {Destination}", sourceUrl, destinationBlobClient.Uri);

            return destinationBlobClient.Uri.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error copying file: {SourceUrl}", sourceUrl);
            throw;
        }
    }

    public async Task<FileMetadata> GetFileMetadataAsync(string fileUrl)
    {
        try
        {
            var blobName = GetBlobNameFromUrl(fileUrl);
            if (string.IsNullOrEmpty(blobName))
                throw new ArgumentException("Invalid file URL");

            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            var properties = await blobClient.GetPropertiesAsync();

            return new FileMetadata
            {
                FileName = Path.GetFileName(blobName),
                ContentType = properties.Value.ContentType,
                Size = properties.Value.ContentLength,
                CreatedAt = properties.Value.CreatedOn.DateTime,
                ModifiedAt = properties.Value.LastModified.DateTime
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file metadata: {FileUrl}", fileUrl);
            throw;
        }
    }

    private string GetBlobNameFromUrl(string fileUrl)
    {
        try
        {
            if (string.IsNullOrEmpty(fileUrl))
                return string.Empty;

            var uri = new Uri(fileUrl);
            var path = uri.AbsolutePath;

            // Remove leading slash and container name
            var parts = path.TrimStart('/').Split('/', 2);
            return parts.Length > 1 ? parts[1] : parts[0];
        }
        catch
        {
            return string.Empty;
        }
    }

    private string SanitizeFileName(string fileName)
    {
        // Remove invalid characters from file name
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
        return sanitized;
    }
}
