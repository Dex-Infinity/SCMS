namespace SCMS.Domain.Enums;

// NOTE: ComplaintStatus is NOT defined here - it already exists in ComplaintStatus.cs
// (added by Virtus for the complaint-API work). Keeping his enum and values as-is
// to avoid a second breaking change on top of the FK change.

public enum AdminRole
{
    SuperAdmin = 0,
    DepartmentAdmin = 1,
    SupportStaff = 2
}
