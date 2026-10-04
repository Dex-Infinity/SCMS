namespace SCMS.Web;

/// <summary>
/// Single source of truth for navigation targets used by the layout and pages.
/// Routes marked "(planned)" don't have a page yet and will show the Not Found page.
/// </summary>
public static class AppRoutes
{
    public const string Dashboard = "/dashboard";
    public const string Complaints = "/complaints";            
    public const string SubmitComplaint = "/submit-complaint";
    public const string History = "/history";                  
    public const string Logout = "/logout";                    
    public const string Notifications = "/notifications";      
    public const string Settings = "/settings";                
    public const string Profile = "/profile";                  
    public const string Search = "/search";                    

    public static string ComplaintDetails(int id) => $"{Complaints}/{id}";
}
