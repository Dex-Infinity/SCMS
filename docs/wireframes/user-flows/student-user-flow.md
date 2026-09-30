# SCMS Student User Flow

**Role:** UI/UX — Roselyn Francis  
**Flow:** Submit → Track → Resolve

## Purpose

This flow describes the student journey through the Student Complaint Management System (SCMS), from secure login through complaint submission, progress tracking, notifications, and final resolution.

## Primary Flow

```text
[ Login ]
    |
    v
[ Dashboard Overview ]
    |
    +-----------------------------+
    |                             |
    v                             v
[ Submit Complaint ]        [ My Complaints ]
    |                             |
    v                             v
[ Complaint Form ]         [ Complaint List ]
    |                             |
    +-----------------------------+
    |
    v
[ Category ]
    |
    v
[ Subject ]
    |
    v
[ Description ]
    |
    v
[ Supporting Documents ]
    |
    v
[ Submit ]
    |
    v
[ Submission Confirmation ]
    |
    v
[ Complaint Tracking ]
    |
    v
[ Status + Action Timeline ]
    |
    v
[ Status Change ]
    |
    v
[ Email / In-app Notification ]
    |
    v
[ Final Resolution + Response ]
    |
    v
[ Student Feedback ]
```

## Screen / Interaction Requirements

| Step | Student goal | UI requirement |
|---|---|---|
| Login | Access the system securely | Login screen using the system's authentication flow |
| Dashboard Overview | Understand current and past complaints | Summary of active and past complaints |
| Submit Complaint | Start a new complaint | Clear **Submit Complaint** primary action |
| Complaint Form | Provide complaint information | Category, Subject, Description fields |
| Supporting Documents | Provide evidence | File attachment control for PDFs/images |
| Submission Confirmation | Know the complaint was submitted | Clear confirmation and complaint reference |
| Complaint List | Find submitted complaints | List of complaints with status badges |
| Complaint Detail | Understand progress | Complaint details and administrator action timeline |
| Notifications | Know when something changes | Email and/or in-app status-change alerts |
| Final Resolution | Understand the outcome | Final response/resolution displayed clearly |
| Feedback | Respond after resolution | Feedback action after the final response |

## Key UX Principles

1. **The primary action must be obvious.** A student should quickly find **Submit Complaint** from the dashboard.
2. **Status must be visible.** Complaint lists should use clear status badges so students can understand progress at a glance.
3. **Progress must be traceable.** The complaint detail view should show the timeline of administrator actions.
4. **Submission must feel confirmed.** After submitting, the student should receive a clear confirmation and complaint reference.
5. **Updates must be noticeable.** Status changes should be communicated through the available email/in-app notification mechanism.
6. **Resolution should be explicit.** The final response should be clearly separated from earlier status updates, followed by the feedback action.

## Source Basis

This flow follows the requirements in `docs/stitch_student_grievance_resolution_portal/scms_user_flows_requirements.md` and the project proposal. The project requirements specify secure login, dashboard overview, complaint submission with category/subject/description and supporting documents, status tracking with a timeline, email/in-app notifications, and final resolution with feedback.

## Usability Revisions

The student flow was reviewed through a moderated usability check involving three student participants and two administrator-role participants.

Based on the usability findings, the following flow clarifications were identified:

1. **Complaint tracking:** `My Complaints` is the primary location for tracking active complaints. Students select a complaint from the complaint list to view its status and updates.
2. **History:** `History` represents previous activity and is not the primary location for tracking an active complaint.
3. **Complaint updates:** Updates and administrator communication should be presented within the individual complaint detail/tracking view.
4. **Under Review:** The status should have supporting text explaining that the complaint is currently being assessed.
5. **Category field:** The complaint form should provide brief guidance or examples to help students understand what type of issue belongs in the Category field.
6. **Submission distinction:** The dashboard should maintain a clear distinction between **Submit Complaint** for creating a new complaint and **My Complaints** for viewing and tracking existing complaints.

### Revised Student Tracking Path

```text
[ Dashboard ]
      |
      v
[ My Complaints ]
      |
      v
[ Complaint List ]
      |
      v
[ Select Complaint ]
      |
      v
[ Complaint Details ]
      |
      +---------------------------+
      |                           |
      v                           v
[ Status ]              [ Updates / Communication ]
      |
      v
[ Resolution ]
