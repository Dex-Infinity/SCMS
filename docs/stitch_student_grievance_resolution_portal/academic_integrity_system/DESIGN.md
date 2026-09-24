---
name: Academic Integrity System
colors:
  surface: '#f7f9fb'
  surface-dim: '#d8dadc'
  surface-bright: '#f7f9fb'
  surface-container-lowest: '#ffffff'
  surface-container-low: '#f2f4f6'
  surface-container: '#eceef0'
  surface-container-high: '#e6e8ea'
  surface-container-highest: '#e0e3e5'
  on-surface: '#191c1e'
  on-surface-variant: '#444651'
  inverse-surface: '#2d3133'
  inverse-on-surface: '#eff1f3'
  outline: '#757682'
  outline-variant: '#c5c5d3'
  surface-tint: '#4059aa'
  primary: '#00236f'
  on-primary: '#ffffff'
  primary-container: '#1e3a8a'
  on-primary-container: '#90a8ff'
  inverse-primary: '#b6c4ff'
  secondary: '#505f76'
  on-secondary: '#ffffff'
  secondary-container: '#d0e1fb'
  on-secondary-container: '#54647a'
  tertiary: '#4b1c00'
  on-tertiary: '#ffffff'
  tertiary-container: '#6e2c00'
  on-tertiary-container: '#f39461'
  error: '#ba1a1a'
  on-error: '#ffffff'
  error-container: '#ffdad6'
  on-error-container: '#93000a'
  primary-fixed: '#dce1ff'
  primary-fixed-dim: '#b6c4ff'
  on-primary-fixed: '#00164e'
  on-primary-fixed-variant: '#264191'
  secondary-fixed: '#d3e4fe'
  secondary-fixed-dim: '#b7c8e1'
  on-secondary-fixed: '#0b1c30'
  on-secondary-fixed-variant: '#38485d'
  tertiary-fixed: '#ffdbcb'
  tertiary-fixed-dim: '#ffb691'
  on-tertiary-fixed: '#341100'
  on-tertiary-fixed-variant: '#773205'
  background: '#f7f9fb'
  on-background: '#191c1e'
  surface-variant: '#e0e3e5'
typography:
  display-lg:
    fontFamily: Inter
    fontSize: 48px
    fontWeight: '700'
    lineHeight: 56px
    letterSpacing: -0.02em
  headline-lg:
    fontFamily: Inter
    fontSize: 32px
    fontWeight: '600'
    lineHeight: 40px
    letterSpacing: -0.01em
  headline-lg-mobile:
    fontFamily: Inter
    fontSize: 24px
    fontWeight: '600'
    lineHeight: 32px
  title-md:
    fontFamily: Inter
    fontSize: 20px
    fontWeight: '600'
    lineHeight: 28px
  body-lg:
    fontFamily: Inter
    fontSize: 18px
    fontWeight: '400'
    lineHeight: 28px
  body-md:
    fontFamily: Inter
    fontSize: 16px
    fontWeight: '400'
    lineHeight: 24px
  label-sm:
    fontFamily: Inter
    fontSize: 14px
    fontWeight: '500'
    lineHeight: 20px
    letterSpacing: 0.01em
  caption:
    fontFamily: Inter
    fontSize: 12px
    fontWeight: '400'
    lineHeight: 16px
rounded:
  sm: 0.25rem
  DEFAULT: 0.5rem
  md: 0.75rem
  lg: 1rem
  xl: 1.5rem
  full: 9999px
spacing:
  base: 4px
  xs: 4px
  sm: 8px
  md: 16px
  lg: 24px
  xl: 32px
  gutter: 24px
  margin-mobile: 16px
  margin-desktop: 48px
---

## Brand & Style
The design system is engineered for a University Student Complaint Management System (SCMS). It prioritizes **Institutional Authority** and **Functional Transparency**. The personality is organized and efficient, aimed at reducing the anxiety of students while providing administrators with a high-density, high-clarity workspace.

The visual style is **Corporate / Modern**, leaning heavily into structured layouts and clear information hierarchies. It utilizes substantial whitespace and a systematic approach to components to ensure that the process of filing and tracking complaints feels objective and reliable. The emotional response should be one of "procedural trust"—the student should feel their case is being handled by a professional, established institution.

## Colors
The palette is rooted in a deep navy blue, symbolizing the university’s heritage and authority. 

- **Primary**: Deep Navy (#1E3A8A) used for headers, primary actions, and brand identification.
- **Secondary/Typography**: Slate Grays (#64748B) ensure long-form text remains highly legible without the harshness of pure black.
- **Backgrounds**: A clean white base with subtle light gray (#F8FAFC) surface tiers to separate content sections.
- **Semantic Palette**: Distinct colors are assigned to complaint statuses to provide instant visual feedback on progress:
    - **Pending**: Solid Blue (Active/Initial)
    - **In Review**: Amber (Processing)
    - **Resolved**: Emerald (Success/Closed)
    - **Escalated/Rejected**: Crimson (Alert/Action Required)

## Typography
This design system uses **Inter** exclusively to leverage its exceptional legibility and systematic weight distribution. 

Headlines utilize a tighter letter-spacing and heavier weights to maintain a sense of importance and structure. Body copy is set with generous line-height to assist in reading long-form complaint descriptions and legal disclaimers. Labels and captions use a medium weight to ensure they remain distinct from body text even at smaller scales. For mobile interfaces, headlines scale down to prevent text wrapping issues while maintaining a clear hierarchy.

## Layout & Spacing
The layout follows a **Fixed Grid** model for desktop to maintain a professional, document-like appearance, centered within the viewport. 

- **Desktop**: 12-column grid, 1140px max-width, 24px gutters.
- **Tablet**: 8-column fluid grid, 24px margins.
- **Mobile**: 4-column fluid grid, 16px margins.

The spacing rhythm is based on a 4px baseline, but primarily uses 8px (sm) and 16px (md) increments for component alignment. Large sections and card groupings are separated by 32px (xl) to prevent the UI from feeling cluttered during complex data entry.

## Elevation & Depth
Depth is conveyed through **Tonal Layers** supplemented by **Ambient Shadows**. 

The background uses a subtle light gray (#F1F5F9), while primary content containers (cards) are pure white. Shadows are kept extremely soft and diffused (e.g., `0 4px 6px -1px rgba(0, 0, 0, 0.1)`) to avoid a "floating" look; instead, elements should feel like they are resting securely on the surface. Interaction states (like hovering over a complaint record) may increase the shadow depth slightly to signify clickability.

## Shapes
The shape language is consistently **Rounded** (8px corner radius). 

This radius strikes a balance between the "sharp" corporate aesthetic and the "soft" approachable aesthetic. It is applied to buttons, input fields, and cards. Status tags and chips use a "Pill" shape (Full Radius) to distinguish them from interactive buttons and structural containers.

## Components
- **Buttons**: Primary buttons are solid Deep Navy (#1E3A8A) with white text. Secondary buttons use a Slate Gray outline. Labels are always descriptive (e.g., "Submit Complaint" instead of "Submit").
- **Inputs**: Text fields use a 1px border (#CBD5E1) and 8px rounded corners. Active states are indicated by a 2px Primary Blue border.
- **Status Chips**: Used for complaint tracking. They use a light-tinted background of the semantic color with high-contrast text of the same hue (e.g., Pending uses Light Blue bg + Dark Blue text).
- **Cards**: The primary container for complaint summaries. Cards feature a 1px stroke (#E2E8F0) and the standard 8px radius.
- **Progress Steppers**: A horizontal indicator for the filing process, showing steps like "Draft," "Verification," and "Submission" to manage user expectations.
- **Data Tables**: Used for the administrator dashboard, featuring high-density rows, zebra-striping for readability, and fixed headers.