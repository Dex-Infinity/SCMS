namespace SCMS.API.DTOs;

// Data transfer object returned to client for an uploaded attachment
public class AttachmentResponseDto
{
    public int Id { get; set; }
    public int ComplaintId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string UploadedBy { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
}