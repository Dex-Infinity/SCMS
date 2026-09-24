using SCMS.Domain.Entities;

namespace SCMS.API.Repositories;

public interface IAttachmentRepository
{
    Task<Attachment?> GetByIdAsync(int id);
    Task<IEnumerable<Attachment>> GetByComplaintIdAsync(int complaintId);
    Task<Attachment> AddAsync(Attachment attachment);
    Task<bool> DeleteAsync(int id);
}