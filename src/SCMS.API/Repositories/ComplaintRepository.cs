using Microsoft.EntityFrameworkCore;
using SCMS.Domain.Entities;
using SCMS.Domain.Enums;
using SCMS.Infrastructure.Data;

namespace SCMS.API.Repositories;

// Repository for handling direct database operations on complaints
public class ComplaintRepository : IComplaintRepository
{
    private readonly ApplicationDbContext _context;

    // Inject EF Core DbContext
    public ComplaintRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    // Get all complaints ordered by creation date
    public async Task<IEnumerable<Complaint>> GetAllAsync()
    {
        return await _context.Complaints
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    // Find complaint by primary key ID
    public async Task<Complaint?> GetByIdAsync(int id)
    {
        return await _context.Complaints.FindAsync(id);
    }

    // Get complaints submitted by a specific student
    public async Task<IEnumerable<Complaint>> GetByStudentIdAsync(string studentId)
    {
        return await _context.Complaints
            .Where(c => c.StudentId == studentId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    // Get complaints assigned to a specific department
    public async Task<IEnumerable<Complaint>> GetByDepartmentIdAsync(string departmentId)
    {
        return await _context.Complaints
            .Where(c => c.DepartmentId == departmentId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    // Insert new complaint into database
    public async Task<Complaint> CreateAsync(Complaint complaint)
    {
        _context.Complaints.Add(complaint);
        await _context.SaveChangesAsync();
        return complaint;
    }

    // Update status column for a complaint
    public async Task<Complaint?> UpdateStatusAsync(int id, ComplaintStatus status)
    {
        var complaint = await _context.Complaints.FindAsync(id);
        if (complaint == null) return null;

        complaint.Status = status;
        complaint.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return complaint;
    }

    // Assign complaint to department and update status
    public async Task<Complaint?> AssignAsync(int id, string departmentId, string? assignedTo)
    {
        var complaint = await _context.Complaints.FindAsync(id);
        if (complaint == null) return null;

        complaint.DepartmentId = departmentId;
        if (!string.IsNullOrWhiteSpace(assignedTo))
        {
            complaint.AssignedTo = assignedTo;
        }
        complaint.Status = ComplaintStatus.Assigned;
        complaint.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return complaint;
    }
}
