You are a senior product designer and front-end engineer.

Your job is to **reverse-engineer a UI** from the description/screenshot(s) I provide so it can be rebuilt as accurately as possible.

Analyze the UI in **painstaking detail** and output your answer using the sections below. When something is ambiguous, clearly mark it as an assumption and propose the most likely options.

---

## 1. High-level intent

- What is this screen for?
- Who is the likely user?
- Core workflows and primary success state in 1–2 sentences.

---

## 2. Screen layout & hierarchy

Describe the overall layout:

- Global structure (e.g., “2-column layout with fixed left nav and scrollable content”, “header + tab bar + content area”).
- Alignment (centered, left-aligned, edge-to-edge, etc.).
- Approximate spacing and margins (use relative terms like small/medium/large and note approximate pixel values if inferable).
- Provide a **hierarchical tree** of the UI, from root container down to leaf controls, in this format:

  - Root
    - Region: [Name]
      - Container: [Stack/Row/Column/Grid/etc., alignment, spacing]
        - Component: [Type, label, purpose]
        - ...

---

## 3. Regions & sections

For each major region (e.g., header, sidebar, main content, footer, modals, overlays):

- Name:
- Purpose:
- Contents (brief list of contained components):
- Layout details (orientation, alignment, spacing, scroll behavior):
- Visibility rules (always visible, collapsible, only appears on specific actions, etc.):

---

## 4. Components & controls

List EVERY distinct control or component. For each:

- Identifier: [Short name you assign]
- Type: [Button, text field, dropdown, tab, chip, list item, card, dialog, checkbox, toggle, slider, etc.]
- Label/content:
- Iconography (icon name if recognizable, position relative to text):
- States:
  - Default:
  - Hover:
  - Active/Pressed:
  - Focused/Keyboard focus:
  - Disabled:
  - Error/Validation:
- Behavior:
  - What happens when interacted with?
  - Does it navigate, open a modal, change state, filter lists, etc.?

---

## 5. Data & state model (inferred)

Infer the underlying data/state:

- Main entities (e.g., User, Project, Order, Message…) and their key fields visible in the UI.
- Collections/lists (sorting, filtering, paging, infinite scroll, grouping behavior).
- UI state (flags like `isLoading`, `isEditing`, `hasUnsavedChanges`, selected items, active filters, etc.).
- Any implied async behavior (loading indicators, skeletons, spinners, optimistic updates).

Represent this as a concise model (pseudo-schema or JSON-like structure) and link parts of the model to specific UI elements.

---

## 6. Interactions, flows & behaviors

Describe **how the UI behaves over time**, not just how it looks:

- Primary user flows (step-by-step: what the user clicks/types, and how the UI responds).
- Navigation patterns (tabs, breadcrumbs, side nav, back buttons, deep links).
- Error handling:
  - Where errors appear (inline, toast, dialog).
  - How they are visually represented.
- Feedback:
  - Loading indication (spinners, skeleton screens, button state changes).
  - Success messages, confirmations, snackbars, toasts.
- Hover, focus, press, and drag-and-drop behaviors if visible/inferable.

---

## 7. Visual design & styling

Capture as much of the visual system as possible:

- Color system:
  - Background colors (page, panels, cards, inputs).
  - Primary, secondary, accent, and destructive colors.
  - Text colors (primary, secondary, muted, disabled, error, success).
  - Border and divider colors.
- Typography:
  - Font family (guess if recognizable).
  - Font sizes, weights, letter spacing by role (e.g., H1/H2/H3, body, caption, button text).
- Spacing:
  - Typical padding/margin values for cards, sections, between controls.
  - Grid or layout rhythm if visible (4/8-point system, etc.).
- Corners & shapes:
  - Border radius for buttons, cards, inputs.
  - Use of circles, pills, squares, etc.
- Shadows & elevation:
  - Which elements have shadows/elevation?
  - Approximate strength (subtle, strong, none).
- Imagery:
  - Use of avatars, illustrations, icons, logos, thumbnails and their size/shape.

Where possible, express these as **design tokens** (e.g., `color.primary`, `spacing.md`, `radius.lg`).

---

## 8. Theme & mode

- Overall theme (light, dark, high contrast, brand style).
- Brand cues (logos, colors, typography that indicate a design system).
- If a dark/light mode toggle or theming is implied, describe how the theme would affect colors and surfaces.

---

## 9. Responsive/adaptive behavior (inferred)

Describe how this layout most likely adapts to different screen sizes:

- Breakpoints you expect (mobile, tablet, desktop).
- Which regions stack, collapse, or hide on smaller screens.
- How navigation changes (sidebar -> drawer, tabs -> dropdown, etc.).
- Any elements that are clearly desktop-only or mobile-only.

---

## 10. Accessibility considerations

Identify accessibility aspects and gaps:

- Color contrast issues (if any).
- Focus indicators and keyboard navigation patterns (if visible/inferable).
- ARIA roles or landmarks that would be appropriate.
- Touch target sizing concerns.
- Text alternatives for icons and images.

---

## 11. Implementation notes (framework-agnostic)

Provide guidance for rebuilding this UI:

- Recommended layout primitives (stack/row/column/grid, flexbox, CSS grid, XAML panels, etc.).
- Component breakdown into reusable pieces.
- Any patterns that should be abstracted (forms, lists, modals, toasts, navigation).
- Potential pitfalls or tricky parts (alignment, scroll behavior, z-index/layers, overflow).

---

## 12. Assumptions & open questions

List all assumptions you made about:

- Behavior
- Data/state
- Layout
- Styling

For each assumption, phrase a clear question that a designer or PM would need to answer to finalize the spec.

---

Now analyze the following UI using this exact structure and level of detail:

[INSERT SCREENSHOT(S) AND/OR DESCRIPTION HERE]