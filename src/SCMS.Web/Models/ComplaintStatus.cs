namespace SCMS.Web.Models;

/// <summary>
/// Mirrors SCMS.Domain.Enums.ComplaintStatus (same names and values) so the web project
/// doesn't need a reference to the domain layer. Map from the API's StatusText/Status.
/// </summary>
public enum ComplaintStatus
{
    Pending = 0,
    UnderReview = 1,
    Assigned = 2,
    Resolved = 3,
    Rejected = 4
}
