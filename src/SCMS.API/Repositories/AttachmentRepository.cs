using Microsoft.EntityFrameworkCore;
using SCMS.Domain.Entities;
using SCMS.Infrastructure.Data;

namespace SCMS.API.Repositories;

// Repository for handling direct database operations on attachments
public class AttachmentRepository : IAttachmentRepository
{
    private readonly ApplicationDbContext _context;

    // Inject EF Core DbContext
    public AttachmentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    // Find attachment metadata by primary key ID
    public async Task<Attachment?> GetByIdAsync(int id)
    {
        return await _context.Attachments.FindAsync(id);
    }

    // Get all attachments for a specific complaint
    public async Task<IEnumerable<Attachment>> GetByComplaintIdAsync(int complaintId)
    {
        return await _context.Attachments
            .Where(a => a.ComplaintId == complaintId)
            .OrderByDescending(a => a.UploadedAt)
            .ToListAsync();
    }

    // Persist attachment metadata
    public async Task<Attachment> AddAsync(Attachment attachment)
    {
        _context.Attachments.Add(attachment);
        await _context.SaveChangesAsync();
        return attachment;
    }

    // Delete attachment metadata
    public async Task<bool> DeleteAsync(int id)
    {
        var attachment = await _context.Attachments.FindAsync(id);
        if (attachment == null)
        {
            return false;
        }

        _context.Attachments.Remove(attachment);
        await _context.SaveChangesAsync();
        return true;
    }
}