namespace HomeService.Application.Interfaces;

public interface IFileStorageService
{
    /// <summary>
    /// Upload a file to storage
    /// </summary>
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, string folder = "uploads");

    /// <summary>
    /// Upload multiple files to storage
    /// </summary>
    Task<List<string>> UploadFilesAsync(List<(Stream stream, string fileName, string contentType)> files, string folder = "uploads");

    /// <summary>
    /// Delete a file from storage
    /// </summary>
    Task<bool> DeleteFileAsync(string fileUrl);

    /// <summary>
    /// Delete multiple files from storage
    /// </summary>
    Task<bool> DeleteFilesAsync(List<string> fileUrls);

    /// <summary>
    /// Get a temporary download URL for a file (SAS token)
    /// </summary>
    Task<string> GetDownloadUrlAsync(string fileUrl, int expiryMinutes = 60);

    /// <summary>
    /// Check if a file exists
    /// </summary>
    Task<bool> FileExistsAsync(string fileUrl);

    /// <summary>
    /// Get file size in bytes
    /// </summary>
    Task<long> GetFileSizeAsync(string fileUrl);

    /// <summary>
    /// Copy a file within storage
    /// </summary>
    Task<string> CopyFileAsync(string sourceUrl, string destinationFolder);

    /// <summary>
    /// Get file metadata
    /// </summary>
    Task<FileMetadata> GetFileMetadataAsync(string fileUrl);
}

public class FileMetadata
{
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long Size { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
}
