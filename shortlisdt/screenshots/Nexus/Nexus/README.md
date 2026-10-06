# NEXUS Component Library

> All components extracted from the NEXUS Industrial Control System dashboard as standalone SVG files.

## Color Tokens

```css
--bg-primary: #0a0a0b
--bg-secondary: #111113
--bg-tertiary: #18181b
--border-primary: #27272a
--border-secondary: #3f3f46
--text-primary: #fafafa
--text-secondary: #a1a1aa
--text-tertiary: #71717a
--success: #4ade80
--warning: #fbbf24
--danger: #f87171
--info: #60a5fa
```

## Typography

- **Display:** Space Grotesk, 300 weight
- **System:** IBM Plex Mono, 400 weight
- **Letter spacing:** 1-4px depending on context

---

## Components

### Core UI Elements

| # | Component | File | Used In |
|---|-----------|------|---------|
| 01 | Metric Card | `01-metric-card.svg` | Overview, Analytics |
| 02 | Status Indicators | `02-status-indicators.svg` | All pages |
| 03 | Panel | `03-panel.svg` | All pages |
| 07 | Buttons | `07-buttons.svg` | All pages |
| 08 | Toggle Switch | `08-toggle-switch.svg` | Settings |
| 09 | Form Inputs | `09-form-inputs.svg` | Settings |
| 22 | Badges | `22-badges.svg` | Production, Maintenance, Settings |

### Navigation & Layout

| # | Component | File | Used In |
|---|-----------|------|---------|
| 10 | Navigation Tabs | `10-navigation-tabs.svg` | Header |
| 11 | Logo Mark | `11-logo-mark.svg` | Header |
| 24 | Footer | `24-footer.svg` | All pages |
| 25 | Connection Status | `25-connection-status.svg` | Header |

### Data Display

| # | Component | File | Used In |
|---|-----------|------|---------|
| 04 | Table Row | `04-table-row.svg` | Overview, Production |
| 06 | Alert Item | `06-alert-item.svg` | Overview |
| 21 | Progress Bars | `21-progress-bars.svg` | Production, Overview |
| 26 | Comparison Matrix | `26-comparison-matrix.svg` | Analytics |

### Production Components

| # | Component | File | Used In |
|---|-----------|------|---------|
| 12 | Shift Information | `12-shift-info.svg` | Production |
| 13 | Inventory Bar | `13-inventory-bar.svg` | Production |
| 14 | Line Card | `14-line-card.svg` | Production |

### Analytics Components

| # | Component | File | Used In |
|---|-----------|------|---------|
| 15 | Bar Chart | `15-bar-chart.svg` | Analytics |
| 16 | Line Chart | `16-line-chart.svg` | Analytics |
| 17 | Defect Bar | `17-defect-bar.svg` | Analytics |

### Maintenance Components

| # | Component | File | Used In |
|---|-----------|------|---------|
| 18 | Health Ring | `18-health-ring.svg` | Maintenance |
| 20 | Spare Parts | `20-spare-parts.svg` | Maintenance |

### Settings Components

| # | Component | File | Used In |
|---|-----------|------|---------|
| 19 | User Avatar | `19-user-avatar.svg` | Settings |
| 23 | Network Config | `23-network-config.svg` | Settings |

### Visualizations

| # | Component | File | Used In |
|---|-----------|------|---------|
| 05 | Network Topology | `05-network-topology.svg` | Overview |

---

## Usage Notes

1. **All SVGs include embedded fonts** — Reference `IBM Plex Mono` and `Space Grotesk` via Google Fonts or local installation
2. **Animations included** — Status dots and network nodes have CSS animations embedded
3. **Dark theme only** — All components designed for dark background context
4. **Consistent spacing** — 4px base grid (4, 8, 12, 16, 20, 24, 32)

---

## File Structure

```
nexus-components/
├── 01-metric-card.svg
├── 02-status-indicators.svg
├── 03-panel.svg
├── 04-table-row.svg
├── 05-network-topology.svg
├── 06-alert-item.svg
├── 07-buttons.svg
├── 08-toggle-switch.svg
├── 09-form-inputs.svg
├── 10-navigation-tabs.svg
├── 11-logo-mark.svg
├── 12-shift-info.svg
├── 13-inventory-bar.svg
├── 14-line-card.svg
├── 15-bar-chart.svg
├── 16-line-chart.svg
├── 17-defect-bar.svg
├── 18-health-ring.svg
├── 19-user-avatar.svg
├── 20-spare-parts.svg
├── 21-progress-bars.svg
├── 22-badges.svg
├── 23-network-config.svg
├── 24-footer.svg
├── 25-connection-status.svg
├── 26-comparison-matrix.svg
└── README.md
```
