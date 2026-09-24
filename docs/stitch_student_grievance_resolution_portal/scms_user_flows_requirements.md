# SCMS User Flows

## Student Flow (Submit → Track → Resolve)
1. **Login**: Secure access via ASP.NET Identity.
2. **Dashboard Overview**: View summary of active and past complaints.
3. **Submit Complaint**: 
   - Fill out form (Category, Subject, Description).
   - Attach supporting documents (PDFs, Images).
   - Submission confirmation.
4. **Track Progress**:
   - List view of complaints with real-time status badges.
   - Detail view showing timeline of actions taken by administrators.
5. **Receive Notifications**: Email/In-app alerts for status changes.
6. **Resolution**: View final response and provide feedback.

## Administrator Flow (Review → Assign → Resolve)
1. **Admin Dashboard**: Overview of incoming complaint queue and department performance.
2. **Review Queue**: Filter complaints by date, category, or priority.
3. **Detail View & Assessment**: Open a complaint to read details and view attachments.
4. **Assign/Route**: Assign the complaint to a specific department or staff member.
5. **Update Status**: Move from "Pending" to "In Review" or "Information Required".
6. **Resolve**: Input the final resolution, attach necessary documents, and notify the student.

## Management Flow (Analyze → Report)
1. **Analytics Dashboard**: High-level charts (Complaints by category, average resolution time).
2. **Reporting**: Export detailed CSV/PDF reports for university board meetings.