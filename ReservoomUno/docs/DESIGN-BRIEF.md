# Design Brief

## Theme

- Base: Uno Material (Material Design 3)
- Color mode: Light (default)
- Primary color: Use Material default or hotel-themed warm palette
- Secondary color: From Uno Material palette

## Original WPF Visual Identity

The original Reservoom app has a minimal, functional design:
- White background, dark text
- Simple form layout with labels above inputs
- ListView with GridView columns for reservation listing
- Basic button styling with padding
- Loading spinner for async operations
- Red error text for validation messages

## Migrated Design Direction

- Material Design 3 surfaces and elevation
- Card-based reservation list items (replace flat GridView)
- Material TextBox and CalendarDatePicker with proper labels
- FilledButton for primary actions, OutlinedButton for secondary
- ProgressRing for loading states
- Material NavigationBar for page headers
- Error messages using Material error color tokens

## Typography

- Use Uno Material default typography scale (Roboto)
- Title: TitleLarge style for page headers
- Body: BodyLarge for form labels, BodyMedium for list items

## Spacing and Layout

- Follow Material Design 3 spacing tokens
- Page padding: 16-24px
- Card spacing: 12-16px
- Form field spacing: 16px
- MaxWidth: 600px for content area (matches original)
