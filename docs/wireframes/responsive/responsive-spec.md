# SCMS — Admin Dashboard: Responsive Layouts

Responsive versions of the admin dashboard (`../admin-dashboard/admin-dashboard.png` is the desktop reference).

## Figma design

[SCMS — Admin Dashboard](https://www.figma.com/design/LmMBTp0BQW5cm7yZPZCcVX/SCMS-%E2%80%94-Admin-Dashboard)

## Breakpoints

| Breakpoint | Width | Frame size | Navigation | KPI cards | Review queue |
|---|---|---|---|---|---|
| Desktop | 1200 px and up | 1440 × 1024 | 240 px sidebar with labels | 4 in a row | Full table (7 columns) |
| Tablet | 768–1199 px | 768 × 1024 | 72 px icon rail | 4 in a row, 150 px each | Table without the Date column |
| Mobile | below 768 px | 375 × 812 | Hamburger opens a 290 px drawer | 2 × 2 grid | Complaint cards instead of a table |

## Tablet behaviour

- Sidebar collapses to a 72 px icon rail; labels can be revealed on hover/tap in the implemented interface.
- KPI cards stay 4-up but tighten to 150 px wide.
- Table drops the Date column (shown on the detail screen).
- Search gets its own row; the four filters sit in one row below.

## Mobile behaviour

- Top bar (56 px): hamburger, SCMS, notification bell, avatar.
- Filters are a horizontally scrollable chip row; the Date chip is partially clipped at the right edge to indicate additional filters. Search is full width.
- Each complaint is a card: ID and date, student, category, priority and status badges, View action.
- Pagination becomes Prev / Page X of Y / Next, with large touch targets (40 px).

## Design notes

- Desktop starts at 1200 px because the 240 px sidebar would leave too little room for the table at 1024 px.
- The mobile page scrolls; 375 × 812 is the viewport, not a limit on page length.
- Colours, badges and sample data are taken from the corrected desktop design.
- The **Medium** priority badge is grey here (desktop uses light blue) so it is not confused with the blue **In Review** status badge. Update the desktop design to match if the team agrees.
- Minimum touch target is 38 px on tablet and mobile.

## Files

- `admin-dashboard-mobile.png` / `.svg`: mobile dashboard and open menu
- `admin-dashboard-tablet.png` / `.svg`: tablet dashboard

Sample data only; not live system data.
