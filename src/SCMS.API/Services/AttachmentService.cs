using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using SCMS.API.DTOs;
using SCMS.API.Repositories;
using SCMS.Domain.Entities;

namespace SCMS.API.Services;

public class AttachmentOptions
{
    public string StoragePath { get; set; } = "uploads";
    public long MaxFileSizeBytes { get; set; } = 5 * 1024 * 1024;
    public string[] AllowedExtensions { get; set; } = { ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx" };
}

public record AttachmentDownloadResult(AttachmentResponseDto Attachment, FileStream Stream, string ContentType);

public interface IAttachmentService
{
    Task<AttachmentResponseDto?> UploadAsync(int complaintId, IFormFile file, string uploadedBy);
    Task<IEnumerable<AttachmentResponseDto>> GetComplaintAttachmentsAsync(int complaintId);
    Task<AttachmentDownloadResult?> DownloadAsync(int id);
    Task<bool> DeleteAsync(int id);
}

// Service for handling file storage and attachment metadata
public class AttachmentService : IAttachmentService
{
    private readonly IAttachmentRepository _repository;
    private readonly IComplaintRepository _complaintRepository;
    private readonly AttachmentOptions _options;
    private readonly string _rootPath;

    // Inject repository and resolve the storage root directory
    public AttachmentService(
        IAttachmentRepository repository,
        IComplaintRepository complaintRepository,
        IOptions<AttachmentOptions> options,
        IHostEnvironment environment)
    {
        _repository = repository;
        _complaintRepository = complaintRepository;
        _options = options.Value;
        _rootPath = Path.IsPathRooted(_options.StoragePath)
            ? _options.StoragePath
            : Path.Combine(environment.ContentRootPath, _options.StoragePath);
    }

    // Store a supporting document for a complaint and persist its metadata
    public async Task<AttachmentResponseDto?> UploadAsync(int complaintId, IFormFile file, string uploadedBy)
    {
        if (file == null || file.Length == 0)
        {
            throw new InvalidOperationException("No file was provided for upload.");
        }

        if (file.Length > _options.MaxFileSizeBytes)
        {
            throw new InvalidOperationException(
                $"File exceeds the maximum allowed size of {_options.MaxFileSizeBytes / (1024 * 1024)} MB.");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(extension) || !_options.AllowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException(
                $"File type '{extension}' is not allowed. Allowed types: {string.Join(", ", _options.AllowedExtensions)}.");
        }

        var complaint = await _complaintRepository.GetByIdAsync(complaintId);
        if (complaint == null)
        {
            return null;
        }

        var storedFileName = $"{Guid.NewGuid():N}{extension}";
        var storageDirectory = Path.Combine(_rootPath, complaintId.ToString());
        Directory.CreateDirectory(storageDirectory);
        var storagePath = Path.Combine(storageDirectory, storedFileName);

        using (var stream = new FileStream(storagePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        try
        {
            var attachment = await _repository.AddAsync(new Attachment
            {
                ComplaintId = complaintId,
                FileName = Path.GetFileName(file.FileName),
                StoredFileName = storedFileName,
                ContentType = file.ContentType,
                FileSize = file.Length,
                UploadedBy = uploadedBy,
                UploadedAt = DateTime.UtcNow
            });

            return MapToResponseDto(attachment);
        }
        catch
        {
            File.Delete(storagePath);
            throw;
        }
    }

    // List all attachments for a complaint
    public async Task<IEnumerable<AttachmentResponseDto>> GetComplaintAttachmentsAsync(int complaintId)
    {
        var attachments = await _repository.GetByComplaintIdAsync(complaintId);
        return attachments.Select(MapToResponseDto);
    }

    // Stream an attachment back to the client
    public async Task<AttachmentDownloadResult?> DownloadAsync(int id)
    {
        var attachment = await _repository.GetByIdAsync(id);
        if (attachment == null)
        {
            return null;
        }

        var storagePath = Path.Combine(_rootPath, attachment.ComplaintId.ToString(), attachment.StoredFileName);
        if (!File.Exists(storagePath))
        {
            return null;
        }

        var stream = new FileStream(storagePath, FileMode.Open, FileAccess.Read);
        return new AttachmentDownloadResult(MapToResponseDto(attachment), stream, attachment.ContentType);
    }

    // Delete an attachment (file and metadata)
    public async Task<bool> DeleteAsync(int id)
    {
        var attachment = await _repository.GetByIdAsync(id);
        if (attachment == null)
        {
            return false;
        }

        var storagePath = Path.Combine(_rootPath, attachment.ComplaintId.ToString(), attachment.StoredFileName);
        if (File.Exists(storagePath))
        {
            File.Delete(storagePath);
        }

        return await _repository.DeleteAsync(id);
    }

    // Helper: Map Attachment entity to AttachmentResponseDto
    private static AttachmentResponseDto MapToResponseDto(Attachment attachment)
    {
        return new AttachmentResponseDto
        {
            Id = attachment.Id,
            ComplaintId = attachment.ComplaintId,
            FileName = attachment.FileName,
            ContentType = attachment.ContentType,
            FileSize = attachment.FileSize,
            UploadedBy = attachment.UploadedBy,
            UploadedAt = attachment.UploadedAt
        };
    }
}