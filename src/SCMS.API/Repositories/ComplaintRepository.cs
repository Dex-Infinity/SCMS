using Microsoft.EntityFrameworkCore;
using SCMS.Domain.Entities;
using SCMS.Domain.Enums;
using SCMS.Infrastructure.Data;

namespace SCMS.API.Repositories;

// Repository for handling direct database operations on complaints
public class ComplaintRepository : IComplaintRepository
{
    private readonly ApplicationDbContext _context;

    public ComplaintRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    // Get all complaints ordered by creation date with related entity data
    public async Task<IEnumerable<Complaint>> GetAllAsync()
    {
        return await _context.Complaints
            .Include(c => c.Student)
            .Include(c => c.Department)
            .Include(c => c.AssignedTo)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    // Find complaint by primary key ID with related entity data
    public async Task<Complaint?> GetByIdAsync(int id)
    {
        return await _context.Complaints
            .Include(c => c.Student)
            .Include(c => c.Department)
            .Include(c => c.AssignedTo)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    // Get complaints submitted by a specific student ID
    public async Task<IEnumerable<Complaint>> GetByStudentIdAsync(int studentId)
    {
        return await _context.Complaints
            .Include(c => c.Student)
            .Include(c => c.Department)
            .Include(c => c.AssignedTo)
            .Where(c => c.StudentId == studentId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    // Get complaints assigned to a specific department ID
    public async Task<IEnumerable<Complaint>> GetByDepartmentIdAsync(int departmentId)
    {
        return await _context.Complaints
            .Include(c => c.Student)
            .Include(c => c.Department)
            .Include(c => c.AssignedTo)
            .Where(c => c.DepartmentId == departmentId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    // Insert new complaint into database
    public async Task<Complaint> CreateAsync(Complaint complaint)
    {
        _context.Complaints.Add(complaint);
        await _context.SaveChangesAsync();

        // Reload navigation properties for response
        return (await GetByIdAsync(complaint.Id))!;
    }

    // Update status column for a complaint
    public async Task<Complaint?> UpdateStatusAsync(int id, ComplaintStatus status)
    {
        var complaint = await _context.Complaints.FindAsync(id);
        if (complaint == null) return null;

        complaint.Status = status;
        complaint.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    // Assign complaint to department and optional admin staff
    public async Task<Complaint?> AssignAsync(int id, int departmentId, int? assignedToId)
    {
        var complaint = await _context.Complaints.FindAsync(id);
        if (complaint == null) return null;

        complaint.DepartmentId = departmentId;
        complaint.AssignedToId = assignedToId;
        complaint.Status = ComplaintStatus.Assigned;
        complaint.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    // Fetch chronological status history for a complaint
    public async Task<IEnumerable<StatusHistory>> GetStatusHistoryAsync(int complaintId)
    {
        return await _context.StatusHistories
            .Where(h => h.ComplaintId == complaintId)
            .OrderBy(h => h.ChangedAt)
            .ToListAsync();
    }

    // Record a new status history entry
    public async Task AddStatusHistoryAsync(StatusHistory history)
    {
        _context.StatusHistories.Add(history);
        await _context.SaveChangesAsync();
    }
}
