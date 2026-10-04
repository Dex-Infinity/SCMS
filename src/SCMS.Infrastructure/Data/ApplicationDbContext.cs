using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SCMS.Domain.Entities;
using SCMS.Infrastructure.Identity;

namespace SCMS.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Complaint> Complaints => Set<Complaint>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Admin> Admins => Set<Admin>();
    public DbSet<Attachment> Attachments => Set<Attachment>();
    public DbSet<StatusHistory> StatusHistories => Set<StatusHistory>();
    public DbSet<UserSetting> UserSettings => Set<UserSetting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Department Configuration
        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(20);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(150);
            entity.Property(e => e.Description).HasMaxLength(500);
        });

        // Student Configuration
        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.IndexNumber).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.IndexNumber).IsUnique();
            entity.Property(e => e.FullName).IsRequired().HasMaxLength(150);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(150);
            entity.HasIndex(e => e.Email).IsUnique();

            entity.HasOne(e => e.Department)
                .WithMany(d => d.Students)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Admin Configuration
        modelBuilder.Entity<Admin>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.StaffId).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.StaffId).IsUnique();
            entity.Property(e => e.FullName).IsRequired().HasMaxLength(150);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(150);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Role).IsRequired().HasMaxLength(50);

            entity.HasOne(e => e.Department)
                .WithMany(d => d.Admins)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Complaint Configuration
        modelBuilder.Entity<Complaint>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).IsRequired();

            // Indexes for common query patterns and analytics
            entity.HasIndex(e => e.StudentId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.DepartmentId);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => new { e.StudentId, e.Status });
            entity.HasIndex(e => new { e.StudentId, e.CreatedAt });
            entity.HasIndex(e => new { e.DepartmentId, e.Status });

            entity.HasOne(e => e.Student)
                .WithMany(s => s.Complaints)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Department)
                .WithMany(d => d.Complaints)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.AssignedTo)
                .WithMany(a => a.AssignedComplaints)
                .HasForeignKey(e => e.AssignedToId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Attachment Configuration
        modelBuilder.Entity<Attachment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FileName).IsRequired().HasMaxLength(260);
            entity.Property(e => e.StoredFileName).IsRequired().HasMaxLength(260);
            entity.Property(e => e.ContentType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.UploadedBy).IsRequired().HasMaxLength(450);

            // Index for fetching all attachments for a given complaint
            entity.HasIndex(e => e.ComplaintId);

            entity.HasOne<Complaint>()
                .WithMany(complaint => complaint.Attachments)
                .HasForeignKey(e => e.ComplaintId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // StatusHistory Configuration
        modelBuilder.Entity<StatusHistory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ChangedByUserId).IsRequired().HasMaxLength(450);
            entity.Property(e => e.Note).HasMaxLength(500);

            // Index for audit log queries (all history for a complaint)
            entity.HasIndex(e => e.ComplaintId);
            entity.HasIndex(e => e.ChangedAt);

            entity.HasOne(e => e.Complaint)
                .WithMany(c => c.StatusHistories)
                .HasForeignKey(e => e.ComplaintId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Notification Configuration
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Message).IsRequired();
            entity.Property(e => e.UserId).IsRequired().HasMaxLength(450);

            // Indexes for user notifications query performance
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => new { e.UserId, e.IsRead });
            entity.HasIndex(e => e.ComplaintId);
            entity.HasIndex(e => e.CreatedAt);
        });

        // UserSetting Configuration
        modelBuilder.Entity<UserSetting>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired().HasMaxLength(450);
            entity.HasIndex(e => e.UserId).IsUnique();
            entity.Property(e => e.Theme).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Language).IsRequired().HasMaxLength(10);
        });

        // Seed Initial Data
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Department>().HasData(
            new Department { Id = 1, Code = "CS", Name = "Computer Science", Description = "Department of Computer Science & IT" },
            new Department { Id = 2, Code = "EE", Name = "Electrical Engineering", Description = "Department of Electrical & Computer Engineering" },
            new Department { Id = 3, Code = "AA", Name = "Academic Affairs", Description = "University Central Academic Affairs Office" }
        );
    }
}
