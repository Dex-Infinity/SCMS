namespace SCMS.Web;

/// <summary>
/// Single source of truth for navigation targets used by the layout and pages.
/// Routes marked "(planned)" don't have a page yet and will show the Not Found page.
/// </summary>
public static class AppRoutes
{
    public const string Dashboard = "/dashboard";              // (planned)
    public const string Complaints = "/complaints";            // (planned)
    public const string SubmitComplaint = "/submit-complaint";
    public const string Resources = "/resources";              // (planned)
    public const string History = "/history";                  // (planned)
    public const string EmergencySupport = "/emergency-support"; // (planned)
    public const string Logout = "/logout";                    // (planned)
    public const string Notifications = "/notifications";      // (planned)
    public const string Settings = "/settings";                // (planned)
    public const string Profile = "/profile";                  // (planned)
    public const string Search = "/search";                    // (planned)
}
