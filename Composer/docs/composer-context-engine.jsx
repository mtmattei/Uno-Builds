import React, { useState, useMemo, useEffect } from 'react';

// ─── Composition Engine v11 — UX walkthrough improvements ─────────────
// Built on v10's markdown preview/editor in the right column.
//
// v11 implements 10 fixes from a walkthrough usability pass:
//
//   1. UX/Architecture/Interactions/Data canvases now DERIVE content
//      from Intent via deriveContext(): extracts entity noun (habit /
//      recipe / task / job / …), app name, user noun, offline-first
//      and mobile-first flags. UX screen labels, Architecture solution
//      tree + Services description (HTTP module drops on offline-first),
//      Interactions primary-flow label, Data entity name + record code
//      all substitute from this single derivation. "DEMO CONTENT" badge
//      appears wherever the field-service example is still in play.
//
//   2. Intent canvas shows an "Example values · Clear all" banner
//      whenever any field matches the INTENT_EXAMPLE default. Click
//      Clear all wipes intent to empty and marks dirty.
//
//   3. FilesRail flipped — markdown preview at top (Live file), file
//      list and locked-count panel at bottom. Active file content is
//      what the user cares about; the list is reference material.
//
//   4. Composer textarea accepts Cmd/Ctrl+Enter to submit. Suggestion
//      chips moved below the textarea with "Try" eyebrow + right-aligned
//      "⌘↵ to submit" hint. Always visible while suggestions exist.
//
//   5. Acknowledgment line on transition to previewing — italic Inter
//      behind an amber left rule, templated as: "You asked: '<first
//      sentence>' — here's what changes if I apply that."
//
//   6. Generate preview disabled when !aiConfigured, with hint reading
//      "Add an API key in settings to enable previews".
//
//   7. First-layer button reads "Continue →" instead of "Lock and
//      continue →" — drops "lock" jargon for the very first action.
//
//   8. View all mode at Scaffold layer flips the FilesRail preview to
//      show all locked layers concatenated as prompt-context.md.
//
//   9. Scaffold canvas action row now reads Download bundle (actual
//      Blob download of {appName}-bundle.md) + Copy prompt-context.md
//      (single-click clipboard for the full concatenated context).
//
//  10. Locked context cards auto-collapse all but the most recent 2 —
//      keeps the active canvas in viewport as the stack grows. Click
//      the +/− chevron to toggle any card manually.

const C = {
  paper: '#ffffff', paper2: '#fbfbfb', paper3: '#f5f5f5', paper4: '#0a0a0a',
  ink: '#1a1a1a', ink2: '#3a3a3a', ink3: '#737373', ink4: '#a3a3a3', ink5: '#d4d4d4',
  hairline: '#ececec', hairline2: '#f0f0f0', hairlineDk: '#1f1f1f',
  indigo: '#3d3dff', amber: '#c89c3f', amberSoft: '#fdf8ef',
  phaseRed: '#b04534', phaseBlue: '#3d6f9a', phasePurple: '#7d4fa0',
  phasePink: '#b4567d', phaseGreen: '#6f8068', phaseAmber: '#c89c3f',
};

const SANS = "'Inter', system-ui, -apple-system, sans-serif";
const MONO = "'JetBrains Mono', ui-monospace, 'SF Mono', monospace";

const LAYERS = [
  { id: 'intent',   label: 'Intent',         file: 'README.md',              hint: 'what the app is for' },
  { id: 'ux',       label: 'UX',             file: 'ux-flows.md',            hint: 'how users move through it' },
  { id: 'arch',     label: 'Architecture',   file: 'architecture.md',        hint: 'how it is shaped' },
  { id: 'design',   label: 'Design System',  file: 'design-system.md',       hint: 'how it feels' },
  { id: 'interact', label: 'Interactions',   file: 'interaction-spec.md',    hint: 'every state of every flow' },
  { id: 'data',     label: 'Data',           file: 'data-contracts.md',      hint: 'shapes and contracts' },
  { id: 'impl',     label: 'Implementation', file: 'implementation-plan.md', hint: 'phased build plan' },
  { id: 'scaffold', label: 'Scaffold',       file: 'scaffold.command',       hint: 'runnable starting point' },
];

// Conversational bridge sentences — rendered as italic above each canvas title,
// connecting prior locked context to the current question. Skipped on Intent
// (first layer, no prior context). The voice is first-person plural ("we've",
// "let's") so each bridge feels like a beat in an ongoing dialogue, not a header.
const RECAPS = {
  intent:   null,
  ux:       "We've named what we're building — now let's trace how someone uses it.",
  arch:     "With the user's path mapped — let's figure out the shape underneath.",
  design:   "Modules in place — let's give the surface a feel.",
  interact: "The design's settled — now every state of every flow.",
  data:     "Interactions captured — let's nail down the shapes.",
  impl:     "Shapes locked in — let's plan how it gets built.",
  scaffold: "Eight layers locked. Here's what ships.",
};

// Composer state words — varied by layer state to give the system a more
// responsive voice. "Listening" (dirty) reads as the system paying attention
// to in-flight edits. "Proposing" (previewing) reads as the system offering
// a redraw for review. Less mechanical than "Edit pending" / "Preview rendered".
const COMPOSER_STATUS = {
  clean:      'Refining',
  dirty:      'Listening',
  previewing: 'Proposing',
};

// Pre-populated example values shown on first load. Single source of truth
// so both IntentCanvas and the main component agree on what counts as
// "still showing the example" vs "user has personalized it".
const INTENT_EXAMPLE = {
  appType:     'Field-service scheduling',
  primaryUser: 'Mobile-first technicians',
  workflow:    'Receive jobs, schedule, dispatch',
  platforms:   'Web, iOS, Android',
};

// Extract domain context from the user's Intent so downstream canvases can
// reflect it without an LLM in the loop. The matching is deliberately
// pattern-based, not exhaustive — substitution is a stronger signal of
// "the system is listening" than perfect linguistic analysis.
//
// Returns:
//   appName        — PascalCase identifier safe for filenames and projects
//   entityNoun     — primary domain noun, lowercase (e.g. "job", "habit")
//   entityTitle    — same noun, capitalized for prose ("Job", "Habit")
//   entityPlural   — naive pluralization
//   userNoun       — primary user noun (e.g. "technicians", "kids")
//   isOfflineFirst — true if intent mentions offline, local, queue, etc.
//   isMobileFirst  — true if intent mentions mobile, phone, tablet
//   isFieldService — true if the default field-service example is still in play
const DOMAIN_NOUN_RULES = [
  // [pattern → entity noun] — order matters; first match wins
  { match: /habit|streak/i,                            noun: 'habit' },
  { match: /recipe|cook|meal/i,                        noun: 'recipe' },
  { match: /workout|exercise|fitness/i,                noun: 'workout' },
  { match: /trade|portfolio|invest|stock/i,            noun: 'trade' },
  { match: /task|todo|backlog/i,                       noun: 'task' },
  { match: /note|journal|diary/i,                      noun: 'note' },
  { match: /appointment|booking|reserv/i,              noun: 'appointment' },
  { match: /patient|medical|health|clinic/i,           noun: 'patient' },
  { match: /invoice|billing|payment/i,                 noun: 'invoice' },
  { match: /order|purchase|cart/i,                     noun: 'order' },
  { match: /ticket|incident|issue/i,                   noun: 'ticket' },
  { match: /lesson|class|course/i,                     noun: 'lesson' },
  { match: /dispatch|field-service|job|service-call/i, noun: 'job' },
];

function deriveContext(intent) {
  const blob = [intent.appType, intent.workflow, intent.primaryUser].filter(Boolean).join(' ');

  // Entity noun
  const rule = DOMAIN_NOUN_RULES.find((r) => r.match.test(blob));
  const entityNoun = rule?.noun || 'item';
  const entityTitle = entityNoun.charAt(0).toUpperCase() + entityNoun.slice(1);
  const entityPlural = entityNoun.endsWith('s') ? entityNoun : `${entityNoun}s`;

  // App name — strip non-alphanumerics from appType, PascalCase, fallback
  const cleaned = (intent.appType || 'App').replace(/[^a-zA-Z0-9 ]/g, '');
  const appName = cleaned
    .split(/\s+/)
    .filter(Boolean)
    .map((w) => w.charAt(0).toUpperCase() + w.slice(1))
    .join('') || 'App';

  // Primary user noun — last word of primaryUser, lowercased, pluralized
  const userParts = (intent.primaryUser || 'users').toLowerCase().trim().split(/\s+/);
  const userNoun = userParts[userParts.length - 1] || 'users';

  const isOfflineFirst = /offline|local|queue|sync\s+later|no\s+backend/i.test(blob);
  const isMobileFirst  = /mobile|phone|tablet|on-the-go/i.test(blob);
  const isFieldService = intent.appType === INTENT_EXAMPLE.appType;

  return {
    appName, entityNoun, entityTitle, entityPlural,
    userNoun, isOfflineFirst, isMobileFirst, isFieldService,
  };
}

// Markdown derivation per layer. Each function takes the current canvas
// state ({intent, design}) and returns the markdown body that will be
// written when the layer locks. For layers without mutable canvas state
// (UX, Architecture, Interactions, Data, Implementation), the output is
// static template content; for Intent and Design it interpolates live
// values. Overrides in overrideMarkdown[layerId] supersede this output.
const MARKDOWN_GEN = {
  intent: (s) => `# ${s.intent.appType || 'App'}

${s.intent.appType || 'A new app'} for ${s.intent.primaryUser || 'users'}.

## What this app does

${s.intent.workflow || 'TODO'}

## Platforms

${s.intent.platforms || 'TODO'}
`,

  ux: () => `# UX Flows

## Dispatch flow

Five screens in sequence:

1. **Dashboard** — Today's jobs
2. **Job detail** — Status, location, parts
3. **Schedule** — Drag-to-reorder
4. **Dispatch** — Confirm + assign
5. **Confirmation** — Synced or queued

## Why this flow

Five steps maps to a five-second mental model. Confirmation isn't a
modal — it's a real terminal screen so the offline-queued case has
somewhere to live without surprising the user.

## Agent prompt

> Each screen represents a discrete user state. The architecture in §03
> will pick the navigation primitive — for now, treat the flow as the
> canonical mental model regardless of how it gets wired.
`,

  arch: () => `# Architecture

## Pattern

MVUX over MVVM. Reactive feeds with offline-first state.

## Modules

- **Pages** — Route surfaces: Calendar, Jobs, Technicians
- **Navigation** — Region-based routes
- **State (MVUX)** — Feeds, States, Selection
- **Services** — Job, Technician, Schedule
- **HTTP (Kiota)** — Typed clients, generated
- **Storage** — Local cache, offline-first

## Connections

| From          | To              | Relationship |
|---------------|-----------------|--------------|
| Pages         | State (MVUX)    | binds        |
| Pages         | Navigation      | requests     |
| State (MVUX)  | Services        | consumes     |
| Services      | HTTP (Kiota)    | calls        |
| Services      | Storage         | persists     |

## Why this fits

MVUX gives both views identical reactive feeds with offline-first
state — no MVVM ceremony, no Rx plumbing. Navigation regions let
Shell and TabBar animate independently, which the dispatch flow
in §02 depends on.

## Agent prompt

> When generating XAML, use uen:Region.Attached for navigation surfaces
> and bind ItemsSource to JobsModel.Jobs (an IFeed). Never construct
> ViewModels in code-behind.
`,

  design: (s) => `# Design System

## Color tokens

| Token   | Hex        |
|---------|------------|
| Surface | ${s.design.surface} |
| Action  | ${s.design.action} |
| Info    | ${s.design.info} |
| Success | ${s.design.success} |
| Warn    | ${s.design.warn} |
| Panel   | ${s.design.panel} |
| Tag     | ${s.design.tag} |
| Locked  | ${s.design.locked} |

## Typography

**Body font:** ${s.design.bodyFont}

| Level   | Size  | Weight | Tracking  |
|---------|-------|--------|-----------|
| Display | 26px  | 500    | -0.015em  |
| Heading | 15px  | 500    | -0.01em   |
| Body    | 13px  | 400    | 0         |
| Caption | 10px  | 500    | 0.04em    |

## Spacing rhythm

4-point grid: 4, 8, 12, 16, 24, 32, 48.

## Why this palette

Outdoor sun + a phone in a truck mount means low chroma, high
contrast. Amber is the only saturated hue and is reserved for the
next action — never decoration.

## Agent prompt

> Apply tokens via ThemeResource — never hex. Reference SurfaceBrush,
> OnSurfaceBrush, SecondaryBrush. Light and Dark theme dictionaries
> let the same XAML render correctly in both themes.
`,

  interact: () => `# Interaction Spec

## Overview

3 flows × 6 states per flow = 18 state definitions total.

## State contract

Every screen must implement all six states. State names map exactly
to VisualStateManager state names in generated XAML:

- **Default** — baseline / first paint
- **Loading** — async work in progress
- **Empty** — no data yet, but not an error
- **Error** — user or system error, recoverable
- **Success** — confirmation, terminal positive
- **Offline** — connection unavailable, queued or read-only

## Flow: Create job

- Default: Empty calendar; primary CTA visible.
- Loading: Skeleton rows; CTA disabled.
- Empty: No technicians available; suggest filters.
- Error: Validation conflict; inline message + retry.
- Success: Toast confirmation; new row animates in.
- Offline: Queued locally; banner indicates sync pending.

## Why six states aren't optional

Offline-first means six states aren't optional — they're load-bearing.
A technician in a basement with no signal needs the queued state to
feel as resolved as success.

## Agent prompt

> Every screen must implement all six states. Use VisualStateManager
> groups named exactly: Default, Loading, Empty, Error, Success, Offline.
`,

  data: () => `# Data Contracts

## Entities

\`\`\`csharp
public partial record Job(
  string Id,
  string Title,
  JobStatus Status,
  DateTime ScheduledAt,
  string TechnicianId,
  string? Notes);

public partial record Technician(
  string Id,
  string Name,
  string[] Skills,
  bool Available);

public partial record Schedule(
  DateOnly Date,
  Job[] Jobs,
  Conflict[] Conflicts);
\`\`\`

## Why these shapes

Records, not classes — immutability + structural equality is what
MVUX feeds need to detect change cheaply. Nullability is explicit
because a missing Notes field shouldn't be a bug.

## Agent prompt

> Use C# records for all domain types. Mutations go through commands
> on the model, not setters. The "?" in field types is load-bearing —
> preserve it.
`,

  impl: () => `# Implementation Plan

## Phase 1 — Scaffold

Multi-head Uno solution wired with MVUX + Uno.Extensions.

Files: FieldDispatch.sln, Directory.Packages.props, 4 platform heads

## Phase 2 — Shell

Two regions: top-level Shell and inner TabBar. Routes registered.

Files: Views/Shell.xaml, MainPage.xaml, Navigation/RouteMap.cs

## Phase 3 — Domain

WorkOrder, Technician, Conflict. JobsModel with IFeed + IListFeed.

Files: Models/WorkOrder.cs, Models/Technician.cs, Presentation/JobsModel.cs

## Phase 4 — Screens

Today, Week, Map, Job-detail. Tokens from §04 — no hardcoded colors.

Files: Views/TodayPage.xaml, Views/WeekPage.xaml, Views/JobDetailPage.xaml

## Phase 5 — States

All six states from §05 wired into VSM groups per screen.

Files: Per-screen VSM definitions, Views/States/*.xaml

## Phase 6 — Polish

UI tests, contrast verification, performance budgets.

Files: Tests/*.UI.cs, perf harness, A11y audit

## Why this phasing

Linear with one parallel option (Domain + Screens) keeps the agent
from rebuilding scaffolding when a domain change cascades. Each phase
has a verifiable acceptance step before the next is unlocked.
`,

  scaffold: (s) => `# Scaffold

\`\`\`bash
dotnet new unoapp \\
  -n ${(s.intent.appType || 'App').replace(/\s+/g, '')} \\
  --tfm net10.0 \\
  --platforms wasm,ios,android \\
  --markup xaml --presentation mvux --theme material \\
  --features config,http,logging,nav,mvux
\`\`\`

## Bundle contents

Eight composed files plus a synthesized prompt-context.md that
concatenates all prior layers for downstream agent consumption.

The composition is, for now, complete.
`,
};

const E = 'cubic-bezier(0.4, 0, 0.2, 1)';
const E_REVEAL = 'cubic-bezier(0.16, 1, 0.3, 1)';

// ─── Atomic typography helpers ──────────────────────────────────────────

function Eyebrow({ children, color = C.ink3, size = 11, weight = 500, ls = '0.04em', style }) {
  return (
    <span style={{
      fontFamily: SANS, fontSize: size, fontWeight: weight,
      letterSpacing: ls, textTransform: 'uppercase',
      color, ...(style || {}),
    }}>
      {children}
    </span>
  );
}

function Mono({ children, color = C.ink2, size = 12, weight = 400, style }) {
  return (
    <span style={{
      fontFamily: MONO, fontSize: size, fontWeight: weight,
      color, letterSpacing: '-0.01em', ...(style || {}),
    }}>
      {children}
    </span>
  );
}

function Body({ children, color = C.ink2, size = 14, weight = 400, style }) {
  return (
    <span style={{
      fontFamily: SANS, fontSize: size, fontWeight: weight,
      color, lineHeight: 1.55, ...(style || {}),
    }}>
      {children}
    </span>
  );
}

// ─── Buttons ────────────────────────────────────────────────────────────

function PrimaryButton({ children, onClick, disabled, tone = 'ink' }) {
  const [hover, setHover] = useState(false);
  const tones = {
    ink: { bg: C.ink, bgHover: '#000', fg: C.paper, border: C.ink },
    amber: { bg: C.amber, bgHover: '#b08a37', fg: C.paper4, border: C.amber },
  };
  const t = tones[tone];
  return (
    <button onClick={onClick} disabled={disabled}
      onMouseEnter={() => setHover(true)} onMouseLeave={() => setHover(false)}
      style={{
        fontFamily: SANS, fontSize: 13, fontWeight: 500,
        letterSpacing: '-0.005em',
        color: disabled ? C.ink4 : t.fg,
        background: disabled ? 'transparent' : (hover ? t.bgHover : t.bg),
        border: `1px solid ${disabled ? C.hairline : t.border}`,
        padding: '8px 14px', borderRadius: 6,
        cursor: disabled ? 'not-allowed' : 'pointer',
        transition: 'all 140ms ease',
        display: 'inline-flex', alignItems: 'center', gap: 8,
      }}>
      {children}
    </button>
  );
}

function GhostButton({ children, onClick, small }) {
  const [hover, setHover] = useState(false);
  return (
    <button onClick={onClick}
      onMouseEnter={() => setHover(true)} onMouseLeave={() => setHover(false)}
      style={{
        fontFamily: SANS, fontSize: small ? 12 : 13, fontWeight: 500,
        letterSpacing: '-0.005em',
        color: hover ? C.ink : C.ink2, background: hover ? C.paper3 : 'transparent',
        border: `1px solid ${hover ? C.hairline : 'transparent'}`,
        padding: small ? '5px 10px' : '8px 12px', borderRadius: 6,
        cursor: 'pointer', transition: 'all 140ms ease',
        display: 'inline-flex', alignItems: 'center', gap: 6,
      }}>
      {children}
    </button>
  );
}

// ─── Block handle (Notion-style drag affordance) ────────────────────────

function BlockHandle({ visible }) {
  return (
    <span aria-hidden style={{
      position: 'absolute', left: -22, top: '50%',
      transform: 'translateY(-50%)',
      fontFamily: MONO, fontSize: 13, fontWeight: 500,
      color: C.ink5, lineHeight: 1,
      opacity: visible ? 1 : 0,
      transition: 'opacity 120ms ease',
      cursor: 'grab', userSelect: 'none',
    }}>
      ⋮⋮
    </span>
  );
}

// ─── Editorial primitives ───────────────────────────────────────────────

// Section header — eyebrow + title + optional action, separated from
// content by a hairline divider rather than a card frame.
function SectionHeader({ filename, badge, badgeColor = C.amber, action }) {
  return (
    <div style={{
      display: 'flex', alignItems: 'baseline', justifyContent: 'space-between',
      paddingBottom: 12, marginBottom: 16,
      borderBottom: `1px solid ${C.hairline}`,
    }}>
      <div style={{ display: 'flex', alignItems: 'baseline', gap: 12 }}>
        <Mono color={C.ink} size={12} weight={500}>{filename}</Mono>
        {badge && (
          <>
            <span style={{ color: C.ink5, fontFamily: SANS, fontSize: 12 }}>·</span>
            <Eyebrow size={10} color={badgeColor} weight={500}>{badge}</Eyebrow>
          </>
        )}
      </div>
      {action && <div>{action}</div>}
    </div>
  );
}

// Annotation — replaces WhyThis + AgentPrompt panels. Marginalia style:
// italic Inter, indented with thin left rule. Reads as a developer's
// margin note, not a card.
function Annotation({ label, children, voice = 'rationale' }) {
  return (
    <div style={{
      paddingLeft: 16, marginTop: 18, marginBottom: 6,
      borderLeft: `1px solid ${C.hairline}`,
    }}>
      <Eyebrow size={10} color={C.ink3} weight={500} style={{ display: 'block', marginBottom: 4 }}>
        {label}
      </Eyebrow>
      <span style={{
        fontFamily: SANS, fontSize: 13, fontStyle: 'italic',
        color: C.ink3, lineHeight: 1.55, display: 'block',
      }}>
        {voice === 'quote' && '"'}{children}{voice === 'quote' && '"'}
      </span>
    </div>
  );
}

// Code block — supports light + dark themes, xaml + tree languages.
function CodeBlock({ label, source, code, language = 'xaml', theme = 'dark' }) {
  const palette = theme === 'light'
    ? {
        bg: C.paper2, headerBg: C.paper3, border: C.hairline,
        headerText: C.ink, sourceText: C.ink3, codeText: C.ink2,
        elementColor: '#a83a25', attrColor: '#a8841a',
        stringColor: '#5a7a5a', commentColor: C.ink3,
      }
    : {
        bg: C.paper4, headerBg: '#141414', border: C.hairlineDk,
        headerText: '#fafafa', sourceText: '#888', codeText: '#cfcfcf',
        elementColor: '#e87f6d', attrColor: '#d4a959',
        stringColor: '#9bbf9d', commentColor: '#7a8a72',
      };

  let tinted;
  if (language === 'tree') {
    tinted = code.split('\n').map((line, i) => {
      const hashIdx = line.indexOf('#');
      if (hashIdx === -1) return <div key={i}>{line || '\u00A0'}</div>;
      return (
        <div key={i}>
          {line.slice(0, hashIdx)}
          <span style={{ color: palette.commentColor, fontStyle: 'italic' }}>
            {line.slice(hashIdx)}
          </span>
        </div>
      );
    });
  } else {
    tinted = code.split('\n').map((line, i) => {
      const parts = [];
      let remaining = line;
      let inElement = false;
      while (remaining.length > 0) {
        const tagMatch = remaining.match(/^<\/?[\w:.]+/);
        if (tagMatch) {
          parts.push(<span key={parts.length} style={{ color: palette.elementColor }}>{tagMatch[0]}</span>);
          remaining = remaining.slice(tagMatch[0].length);
          inElement = true;
          continue;
        }
        const attrMatch = remaining.match(/^(\s+)([\w:.]+)(=)/);
        if (attrMatch && inElement) {
          parts.push(<span key={parts.length}>{attrMatch[1]}</span>);
          parts.push(<span key={parts.length + 1} style={{ color: palette.attrColor }}>{attrMatch[2]}</span>);
          parts.push(<span key={parts.length + 2}>{attrMatch[3]}</span>);
          remaining = remaining.slice(attrMatch[0].length);
          continue;
        }
        const strMatch = remaining.match(/^"[^"]*"/);
        if (strMatch && inElement) {
          parts.push(<span key={parts.length} style={{ color: palette.stringColor }}>{strMatch[0]}</span>);
          remaining = remaining.slice(strMatch[0].length);
          continue;
        }
        if (remaining[0] === '>' || remaining.startsWith('/>')) {
          const close = remaining.startsWith('/>') ? '/>' : '>';
          parts.push(<span key={parts.length} style={{ color: palette.elementColor }}>{close}</span>);
          remaining = remaining.slice(close.length);
          inElement = false;
          continue;
        }
        parts.push(<span key={parts.length}>{remaining[0]}</span>);
        remaining = remaining.slice(1);
      }
      return <div key={i}>{parts.length === 0 ? '\u00A0' : parts}</div>;
    });
  }

  return (
    <div style={{
      background: palette.bg, borderRadius: 6,
      border: `1px solid ${palette.border}`, overflow: 'hidden',
      marginTop: 14,
    }}>
      <div style={{
        padding: '8px 14px', borderBottom: `1px solid ${palette.border}`,
        background: palette.headerBg,
        display: 'flex', alignItems: 'center', justifyContent: 'space-between',
      }}>
        <Mono color={palette.headerText} size={11} weight={500}>{label}</Mono>
        <Eyebrow size={10} color={palette.sourceText} weight={500}>{source}</Eyebrow>
      </div>
      <pre style={{
        fontFamily: MONO, fontSize: 12, lineHeight: 1.65,
        color: palette.codeText, margin: 0, padding: '14px 18px',
        whiteSpace: 'pre', overflowX: 'auto',
      }}>
        {tinted}
      </pre>
    </div>
  );
}

// ─── MarkdownPreview ─────────────────────────────────────────────────────
//
// Minimal markdown renderer. Handles the subset of syntax that MARKDOWN_GEN
// actually emits: headings (#, ##, ###), bulleted + numbered lists, fenced
// code blocks, blockquotes, paragraphs, and inline **bold** / *italic* /
// `code`. Deliberately small — no library, no AST. Tuned for the narrow
// rail column where reading flow is the priority, not feature completeness.

function renderInline(text, keyPrefix = 'i') {
  // Tokenize **bold**, *italic*, `code` — left-to-right, non-nested.
  // The regex captures whichever marker comes first; whatever didn't match
  // becomes plain text.
  const out = [];
  const re = /(\*\*[^*]+\*\*)|(\*[^*]+\*)|(`[^`]+`)/g;
  let last = 0, m, idx = 0;
  while ((m = re.exec(text)) !== null) {
    if (m.index > last) out.push(text.slice(last, m.index));
    if (m[1]) {
      out.push(<strong key={`${keyPrefix}-${idx++}`} style={{ fontWeight: 600, color: C.ink }}>
        {m[1].slice(2, -2)}
      </strong>);
    } else if (m[2]) {
      out.push(<em key={`${keyPrefix}-${idx++}`} style={{ fontStyle: 'italic' }}>
        {m[2].slice(1, -1)}
      </em>);
    } else if (m[3]) {
      out.push(<code key={`${keyPrefix}-${idx++}`} style={{
        fontFamily: MONO, fontSize: '0.92em',
        background: C.paper3, color: C.ink,
        padding: '1px 5px', borderRadius: 3,
      }}>
        {m[3].slice(1, -1)}
      </code>);
    }
    last = re.lastIndex;
  }
  if (last < text.length) out.push(text.slice(last));
  return out.length === 0 ? [text] : out;
}

function MarkdownPreview({ source }) {
  // Parse into blocks. Split by blank lines, but keep fenced code blocks
  // intact (they may contain blank lines).
  const blocks = [];
  const lines = source.split('\n');
  let i = 0;
  while (i < lines.length) {
    const line = lines[i];
    // Fenced code block
    if (line.startsWith('```')) {
      const lang = line.slice(3).trim();
      const code = [];
      i++;
      while (i < lines.length && !lines[i].startsWith('```')) {
        code.push(lines[i]);
        i++;
      }
      i++; // skip closing fence
      blocks.push({ type: 'code', lang, content: code.join('\n') });
      continue;
    }
    // Heading
    const h = line.match(/^(#{1,3})\s+(.+)$/);
    if (h) {
      blocks.push({ type: `h${h[1].length}`, content: h[2] });
      i++;
      continue;
    }
    // Blockquote — collect consecutive `>` lines
    if (line.startsWith('>')) {
      const quote = [];
      while (i < lines.length && lines[i].startsWith('>')) {
        quote.push(lines[i].replace(/^>\s?/, ''));
        i++;
      }
      blocks.push({ type: 'quote', content: quote.join(' ') });
      continue;
    }
    // Bulleted list — collect consecutive `-` or `*` items
    if (/^[-*]\s+/.test(line)) {
      const items = [];
      while (i < lines.length && /^[-*]\s+/.test(lines[i])) {
        items.push(lines[i].replace(/^[-*]\s+/, ''));
        i++;
      }
      blocks.push({ type: 'ul', content: items });
      continue;
    }
    // Numbered list — collect consecutive `1.` `2.` items
    if (/^\d+\.\s+/.test(line)) {
      const items = [];
      while (i < lines.length && /^\d+\.\s+/.test(lines[i])) {
        items.push(lines[i].replace(/^\d+\.\s+/, ''));
        i++;
      }
      blocks.push({ type: 'ol', content: items });
      continue;
    }
    // Blank — skip
    if (line.trim() === '') { i++; continue; }
    // Paragraph — collect consecutive non-blank, non-special lines
    const para = [];
    while (i < lines.length && lines[i].trim() !== ''
      && !lines[i].startsWith('#') && !lines[i].startsWith('>')
      && !/^[-*]\s+/.test(lines[i]) && !/^\d+\.\s+/.test(lines[i])
      && !lines[i].startsWith('```')) {
      para.push(lines[i]);
      i++;
    }
    blocks.push({ type: 'p', content: para.join(' ') });
  }

  return (
    <div style={{
      background: C.paper2, border: `1px solid ${C.hairline}`,
      borderRadius: 4, padding: '14px 14px 6px',
      maxHeight: 480, overflowY: 'auto',
      fontFamily: SANS, color: C.ink2, fontSize: 13, lineHeight: 1.55,
    }}>
      {blocks.map((b, bi) => {
        if (b.type === 'h1') return (
          <h1 key={bi} style={{
            fontFamily: SANS, fontSize: 18, fontWeight: 600, color: C.ink,
            letterSpacing: '-0.01em', margin: '0 0 12px',
          }}>
            {renderInline(b.content, `h1-${bi}`)}
          </h1>
        );
        if (b.type === 'h2') return (
          <h2 key={bi} style={{
            fontFamily: SANS, fontSize: 14, fontWeight: 600, color: C.ink,
            letterSpacing: '0.02em', textTransform: 'uppercase',
            margin: '16px 0 8px', paddingBottom: 4,
            borderBottom: `1px solid ${C.hairline}`,
          }}>
            {renderInline(b.content, `h2-${bi}`)}
          </h2>
        );
        if (b.type === 'h3') return (
          <h3 key={bi} style={{
            fontFamily: SANS, fontSize: 12, fontWeight: 600, color: C.ink2,
            letterSpacing: '0.04em', textTransform: 'uppercase',
            margin: '14px 0 6px',
          }}>
            {renderInline(b.content, `h3-${bi}`)}
          </h3>
        );
        if (b.type === 'p') return (
          <p key={bi} style={{ margin: '0 0 10px', color: C.ink2, fontSize: 13 }}>
            {renderInline(b.content, `p-${bi}`)}
          </p>
        );
        if (b.type === 'ul') return (
          <ul key={bi} style={{
            margin: '0 0 10px', paddingLeft: 18,
            color: C.ink2, fontSize: 13,
          }}>
            {b.content.map((it, ii) => (
              <li key={ii} style={{ marginBottom: 3 }}>
                {renderInline(it, `ul-${bi}-${ii}`)}
              </li>
            ))}
          </ul>
        );
        if (b.type === 'ol') return (
          <ol key={bi} style={{
            margin: '0 0 10px', paddingLeft: 22,
            color: C.ink2, fontSize: 13,
          }}>
            {b.content.map((it, ii) => (
              <li key={ii} style={{ marginBottom: 3 }}>
                {renderInline(it, `ol-${bi}-${ii}`)}
              </li>
            ))}
          </ol>
        );
        if (b.type === 'quote') return (
          <blockquote key={bi} style={{
            margin: '0 0 10px', padding: '0 0 0 12px',
            borderLeft: `2px solid ${C.hairline}`,
            color: C.ink3, fontStyle: 'italic', fontSize: 12,
          }}>
            {renderInline(b.content, `q-${bi}`)}
          </blockquote>
        );
        if (b.type === 'code') return (
          <pre key={bi} style={{
            margin: '0 0 10px', padding: '8px 10px',
            background: C.paper4, color: '#cfcfcf',
            border: `1px solid ${C.hairlineDk}`, borderRadius: 4,
            fontFamily: MONO, fontSize: 11, lineHeight: 1.5,
            overflowX: 'auto', whiteSpace: 'pre',
          }}>
            {b.content}
          </pre>
        );
        return null;
      })}
    </div>
  );
}

// ─── Progress indicator ──────────────────────────────────────────────────

function ProgressIndicator({ activeIndex, total }) {
  const fraction = (activeIndex + 1) / total;
  return (
    <div style={{ marginBottom: 32 }}>
      <div style={{ position: 'relative', height: 1, background: C.hairline, marginBottom: 10 }}>
        <div style={{
          position: 'absolute', top: 0, left: 0, height: 1, background: C.amber,
          width: `${fraction * 100}%`, transition: `width 480ms ${E}`,
        }} />
      </div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'baseline' }}>
        <Eyebrow size={10} color={C.ink3} weight={500}>{LAYERS[activeIndex].label}</Eyebrow>
        <Mono color={C.ink4} size={11}>
          {String(activeIndex + 1).padStart(2, '0')} / {String(total).padStart(2, '0')}
        </Mono>
      </div>
    </div>
  );
}

// ─── LEFT — Composition Stack ────────────────────────────────────────────

function StackItem({ index, layer, state, summary, onClick }) {
  const isActive = state === 'active';
  const isLocked = state === 'locked';
  const isFuture = state === 'future';
  const clickable = isActive || isLocked;
  const [hover, setHover] = useState(false);
  return (
    <button
      onClick={clickable ? onClick : undefined}
      onMouseEnter={() => setHover(true)} onMouseLeave={() => setHover(false)}
      style={{
        position: 'relative', display: 'block', width: '100%', textAlign: 'left',
        background: isActive ? C.paper3 : (hover && clickable ? C.paper2 : 'transparent'),
        border: 'none', padding: '12px 16px 12px 24px',
        cursor: clickable ? 'pointer' : 'default',
        opacity: isFuture ? 0.5 : 1,
        borderLeft: isActive
          ? `2px solid ${C.amber}`
          : isLocked ? `2px solid ${C.ink2}` : `2px solid transparent`,
        transition: 'opacity 240ms ease, background 140ms ease',
      }}>
      <div style={{ display: 'flex', alignItems: 'baseline', gap: 10, marginBottom: 2 }}>
        <Mono color={isActive ? C.amber : isLocked ? C.ink2 : C.ink4} size={11} weight={500}>
          {String(index).padStart(2, '0')}
        </Mono>
        <Body
          color={isActive ? C.ink : isLocked ? C.ink : C.ink3}
          size={13}
          weight={isActive ? 600 : 500}
        >
          {layer.label}
        </Body>
        {isLocked && (
          <span style={{ marginLeft: 'auto', fontFamily: SANS, fontSize: 11, color: C.ink3 }}>✓</span>
        )}
      </div>
      <div style={{ paddingLeft: 30 }}>
        <Body color={isLocked ? C.ink2 : C.ink3} size={12}>
          {summary || layer.hint}
        </Body>
      </div>
    </button>
  );
}

function CompositionStack({ activeIndex, lockedIds, summaries, onJump, visible }) {
  return (
    <aside style={{
      width: visible ? 260 : 0, flexShrink: 0,
      borderRight: visible ? `1px solid ${C.hairline}` : 'none',
      paddingTop: 8, position: 'sticky', top: 32, alignSelf: 'flex-start',
      maxHeight: 'calc(100vh - 32px)', overflowY: 'auto', overflowX: 'hidden',
      opacity: visible ? 1 : 0,
      transition: `width 480ms ${E_REVEAL}, opacity 320ms ${E} ${visible ? '160ms' : '0ms'}, border-color 320ms ${E}`,
    }}>
      <div style={{ padding: '0 16px 16px 24px', minWidth: 260 }}>
        <Eyebrow size={10} color={C.ink} weight={600} ls="0.04em" style={{ display: 'block', marginBottom: 4 }}>
          Composition Stack
        </Eyebrow>
        <Body color={C.ink3} size={12}>
          A conversation that crystallizes into a build system.
        </Body>
      </div>
      <div style={{ borderTop: `1px solid ${C.hairline}`, minWidth: 260 }} />
      <div style={{ minWidth: 260 }}>
        {LAYERS.map((layer, i) => {
          let state = 'future';
          if (lockedIds.has(layer.id)) state = 'locked';
          if (i === activeIndex) state = 'active';
          return (
            <StackItem key={layer.id} index={i + 1} layer={layer} state={state}
              summary={summaries[layer.id]} onClick={() => onJump(i)} />
          );
        })}
      </div>
    </aside>
  );
}

// ─── Locked context card — flat with hairline rule, BlockHandle on hover

function LockedContextCard({ layer, summary, contents, onEdit, defaultCollapsed = false }) {
  const [hover, setHover] = useState(false);
  const [expanded, setExpanded] = useState(!defaultCollapsed);
  // Sync to defaultCollapsed when it changes (e.g. user advances and the
  // newly-active layer pushes older locked cards down the stack).
  useEffect(() => {
    setExpanded(!defaultCollapsed);
  }, [defaultCollapsed]);
  return (
    <div
      onMouseEnter={() => setHover(true)} onMouseLeave={() => setHover(false)}
      style={{
        position: 'relative', padding: '14px 0',
        borderTop: `1px solid ${C.hairline}`,
        marginBottom: 0,
      }}>
      <BlockHandle visible={hover} />
      <div style={{ display: 'flex', alignItems: 'baseline', gap: 10, marginBottom: expanded ? 6 : 0 }}>
        <Eyebrow size={10} color={C.ink3} weight={600}>✓ {layer.label}</Eyebrow>
        <span style={{ color: C.ink5, fontFamily: SANS, fontSize: 11 }}>·</span>
        <Eyebrow size={10} color={C.ink4} weight={500}>locked</Eyebrow>
        {!expanded && (
          <Body color={C.ink3} size={13} style={{
            fontStyle: 'italic',
            overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap',
            flex: 1, minWidth: 0,
          }}>
            {summary}
          </Body>
        )}
        <div style={{ marginLeft: 'auto', display: 'flex', alignItems: 'baseline', gap: 12, flexShrink: 0 }}>
          <button
            onClick={() => setExpanded(!expanded)}
            title={expanded ? 'Collapse' : 'Expand'}
            style={{
              fontFamily: MONO, fontSize: 11, color: C.ink4,
              background: 'transparent', border: 'none', cursor: 'pointer',
              padding: 0, fontWeight: 500, lineHeight: 1,
            }}>
            {expanded ? '−' : '+'}
          </button>
          {onEdit && (
            <button onClick={onEdit} style={{
              fontFamily: SANS, fontSize: 12, color: C.ink3,
              background: 'transparent', border: 'none', cursor: 'pointer',
              padding: 0, fontWeight: 500,
            }}>
              Revisit ↗
            </button>
          )}
        </div>
      </div>
      {expanded && (
        <>
          <Body color={C.ink} size={14} style={{ display: 'block', marginBottom: 10 }}>
            {summary}
          </Body>
          {contents && contents.length > 0 && (
            <div style={{
              display: 'grid', gridTemplateColumns: 'repeat(2, 1fr)',
              gap: '4px 32px', paddingTop: 8,
              borderTop: `1px solid ${C.hairline2}`,
            }}>
              {contents.map(([k, v]) => (
                <div key={k} style={{ display: 'flex', alignItems: 'baseline', gap: 12 }}>
                  <Eyebrow size={10} color={C.ink3} weight={500} style={{ minWidth: 90 }}>{k}</Eyebrow>
                  <Mono color={C.ink2} size={12}>{v}</Mono>
                </div>
              ))}
            </div>
          )}
        </>
      )}
    </div>
  );
}

function FuturePreviewCard({ layer, distance }) {
  const opacity = Math.max(0.05, 0.40 - (distance * 0.08));
  return (
    <div style={{
      padding: '10px 0', borderTop: `1px solid ${C.hairline}`,
      opacity, transition: `opacity 480ms ${E_REVEAL}`,
      pointerEvents: 'none',
    }}>
      <div style={{ display: 'flex', alignItems: 'baseline', gap: 10, marginBottom: 2 }}>
        <Eyebrow size={10} color={C.ink3} weight={500}>{layer.label}</Eyebrow>
        <span style={{ color: C.ink5, fontFamily: SANS, fontSize: 11 }}>·</span>
        <Eyebrow size={10} color={C.ink4} weight={500}>upcoming</Eyebrow>
      </div>
      <Body color={C.ink3} size={13}>{layer.hint}</Body>
    </div>
  );
}

// ─── Active layer header ────────────────────────────────────────────────

function ActiveLayerHeader({ index, layer, title, subtitle, layerState }) {
  const recap = RECAPS[layer.id];
  return (
    <div style={{ marginBottom: 24 }}>
      {recap && (
        <div style={{
          display: 'flex', alignItems: 'baseline', gap: 8,
          marginBottom: 16, maxWidth: 560,
        }}>
          <span style={{
            fontFamily: MONO, fontSize: 13, color: C.ink4,
            lineHeight: 1, transform: 'translateY(1px)', flexShrink: 0,
          }}>
            ↳
          </span>
          <span style={{
            fontFamily: SANS, fontSize: 13, fontStyle: 'italic',
            color: C.ink3, lineHeight: 1.55,
          }}>
            {recap}
          </span>
        </div>
      )}
      <div style={{ display: 'flex', alignItems: 'baseline', gap: 10, marginBottom: 10 }}>
        <span style={{ color: C.amber, fontSize: 8 }}>●</span>
        <Mono color={C.ink2} size={11} weight={500}>
          {String(index + 1).padStart(2, '0')} · {layer.label.toUpperCase()}
        </Mono>
        {layerState === 'dirty' && (
          <Eyebrow color={C.amber} size={10} weight={600} style={{ marginLeft: 'auto' }}>
            Edited — preview pending
          </Eyebrow>
        )}
        {layerState === 'previewing' && (
          <Eyebrow color={C.amber} size={10} weight={600} style={{ marginLeft: 'auto' }}>
            Preview — review and accept
          </Eyebrow>
        )}
      </div>
      <h2 style={{
        fontFamily: SANS, fontSize: 26, fontWeight: 500,
        color: C.ink, margin: '0 0 6px',
        letterSpacing: '-0.015em', lineHeight: 1.2,
      }}>
        {title}
      </h2>
      <Body color={C.ink3} size={14} style={{ display: 'block', maxWidth: 560 }}>
        {subtitle}
      </Body>
    </div>
  );
}

// ─── INTENT canvas ──────────────────────────────────────────────────────

function IntentCanvas({ intent, setIntent, onDirty, layerState, previewIntent }) {
  const fields = [
    { key: 'appType', label: 'App type', placeholder: 'Field-service scheduling' },
    { key: 'primaryUser', label: 'Primary user', placeholder: 'Mobile-first technicians' },
    { key: 'workflow', label: 'Workflow', placeholder: 'Receive jobs, schedule, dispatch' },
    { key: 'platforms', label: 'Platforms', placeholder: 'Web, iOS, Android' },
  ];
  const isPreviewing = layerState === 'previewing';
  const displayed = isPreviewing ? previewIntent : intent;
  // The example banner is shown whenever ANY field still holds the original
  // field-service example value — a friendly cue that these aren't the user's
  // own answers yet. Clear all wipes the intent to empty strings.
  const examples = INTENT_EXAMPLE;
  const hasAnyExample = fields.some((f) => intent[f.key] === examples[f.key]);
  const clearAll = () => {
    setIntent({ appType: '', primaryUser: '', workflow: '', platforms: '' });
    onDirty();
  };
  return (
    <div>
      <SectionHeader
        filename="intent.md"
        badge={isPreviewing ? 'PREVIEW' : 'EDITING'}
        badgeColor={isPreviewing ? C.amber : C.ink3}
      />
      {hasAnyExample && !isPreviewing && (
        <div style={{
          display: 'flex', alignItems: 'baseline', justifyContent: 'space-between',
          gap: 12, padding: '10px 14px', marginBottom: 4,
          background: C.amberSoft, borderRadius: 5,
          border: `1px solid ${C.hairline}`,
        }}>
          <div style={{ display: 'flex', alignItems: 'baseline', gap: 8 }}>
            <Eyebrow size={9} color={C.amber} weight={600}>Example values</Eyebrow>
            <Body color={C.ink2} size={12} style={{ fontStyle: 'italic' }}>
              Tap any field to start your own — or clear to begin fresh.
            </Body>
          </div>
          <button onClick={clearAll} style={{
            fontFamily: SANS, fontSize: 12, fontWeight: 500,
            color: C.ink, background: 'transparent',
            border: 'none', padding: 0, cursor: 'pointer',
            textDecoration: 'underline', textUnderlineOffset: 2,
            flexShrink: 0,
          }}>
            Clear all
          </button>
        </div>
      )}
      <div>
        {fields.map((f, i) => {
          const isDifferent = isPreviewing && previewIntent[f.key] !== intent[f.key];
          return (
            <div key={f.key} style={{
              display: 'grid', gridTemplateColumns: '120px 1fr 80px',
              gap: 16, alignItems: 'center', padding: '12px 0',
              borderBottom: i < fields.length - 1 ? `1px solid ${C.hairline2}` : 'none',
              background: isDifferent ? C.amberSoft : 'transparent',
              marginLeft: isDifferent ? -10 : 0, paddingLeft: isDifferent ? 10 : 0,
              marginRight: isDifferent ? -10 : 0, paddingRight: isDifferent ? 10 : 0,
              borderRadius: isDifferent ? 4 : 0,
              transition: 'background 220ms ease',
            }}>
              <Eyebrow size={10} color={C.ink3} weight={500}>{f.label}</Eyebrow>
              <input type="text" value={displayed[f.key] || ''}
                disabled={isPreviewing}
                onChange={(e) => { setIntent({ ...intent, [f.key]: e.target.value }); onDirty(); }}
                placeholder={f.placeholder}
                style={{
                  fontFamily: SANS, fontSize: 14, fontWeight: 400,
                  color: !displayed[f.key] ? C.ink4 : C.ink,
                  background: 'transparent',
                  border: 'none', outline: 'none', padding: '2px 0',
                }} />
              {isDifferent && (
                <Eyebrow size={9} color={C.amber} weight={600}>Proposed</Eyebrow>
              )}
            </div>
          );
        })}
      </div>
      <Annotation label="Why this intent">
        Field-service is a focused vertical with clear stakes — jobs, schedules,
        offline conditions. Naming the workflow here means the agent can stop
        guessing and start scaffolding domain types from §06 with confidence.
      </Annotation>
      <Annotation label="Agent prompt" voice="quote">
        Treat this as the canonical app description. Future layers should
        reference these terms verbatim — Job, Technician, Dispatch — not synonyms.
      </Annotation>
    </div>
  );
}

// ─── UX canvas ──────────────────────────────────────────────────────────

function UXCanvas({ ctx }) {
  // Screens derived from the entity noun + workflow. When the entity is "job"
  // (field-service default) we keep the original dispatch-flow naming; for
  // other entities we substitute to keep the flow recognizable.
  const isJob = ctx.entityNoun === 'job';
  const e = ctx.entityTitle;       // "Habit"
  const eLow = ctx.entityNoun;     // "habit"
  const eList = ctx.entityPlural;  // "habits"
  const screens = isJob ? [
    { name: 'Dashboard', note: "Today's jobs" },
    { name: 'Job detail', note: 'Status, location, parts' },
    { name: 'Schedule', note: 'Drag-to-reorder' },
    { name: 'Dispatch', note: 'Confirm + assign' },
    { name: 'Confirmation', note: 'Synced or queued' },
  ] : [
    { name: 'Dashboard', note: `Today's ${eList}` },
    { name: `${e} detail`, note: 'Status, history, notes' },
    { name: 'Schedule', note: 'Drag-to-reorder' },
    { name: 'Action', note: `Confirm ${eLow}` },
    { name: 'Confirmation', note: 'Synced or queued' },
  ];
  return (
    <div>
      <SectionHeader
        filename={isJob ? "dispatch-flow.md" : `${eLow}-flow.md`}
        badge={ctx.isFieldService ? 'DEMO CONTENT · 5 SCREENS' : '5 SCREENS'}
        badgeColor={ctx.isFieldService ? C.amber : C.ink3}
      />
      <div style={{ overflowX: 'auto', paddingBottom: 4 }}>
        <div style={{ display: 'flex', alignItems: 'flex-start', gap: 6, minWidth: 'fit-content' }}>
          {screens.map((s, i) => (
            <React.Fragment key={s.name}>
              <div style={{
                width: 116, background: C.paper2,
                border: `1px solid ${C.hairline}`, borderRadius: 6,
                padding: '10px 12px',
                display: 'flex', flexDirection: 'column', gap: 8,
                flexShrink: 0,
              }}>
                <Eyebrow size={9} color={C.ink4} weight={500}>SCREEN {i + 1}</Eyebrow>
                <Body color={C.ink} size={13} weight={500}>{s.name}</Body>
                <div style={{ display: 'flex', flexDirection: 'column', gap: 3, marginTop: 4 }}>
                  <div style={{ height: 3, background: C.hairline, borderRadius: 1 }} />
                  <div style={{ height: 3, background: C.hairline, borderRadius: 1, width: '70%' }} />
                  <div style={{ height: 3, background: C.hairline, borderRadius: 1, width: '85%' }} />
                </div>
                <Mono color={C.ink3} size={11}>{s.note}</Mono>
              </div>
              {i < screens.length - 1 && (
                <span style={{ color: C.ink4, fontFamily: SANS, fontSize: 14, padding: '0 2px', alignSelf: 'center' }}>→</span>
              )}
            </React.Fragment>
          ))}
        </div>
      </div>
      <Annotation label="Why this flow">
        Five steps maps to a five-second mental model. Confirmation isn't a
        modal — it's a real terminal screen so the offline-queued case has
        somewhere to live without surprising the user.
      </Annotation>
      <Annotation label="Agent prompt" voice="quote">
        Each screen represents a discrete user state. The architecture in §03
        will pick the navigation primitive — for now, treat the flow as the
        canonical mental model regardless of how it gets wired.
      </Annotation>
    </div>
  );
}

// ─── ARCHITECTURE canvas — v8 rich blueprint with hover-to-explore ─────

// Static base modules and edges. ArchitectureCanvas may filter/relabel
// them based on Intent context (drop HTTP for offline-first apps, swap
// the Services description to use the user's entity nouns, etc.).
const BASE_MODULES = [
  { id: 'pages',    label: 'Pages',         pos: { x: 110, y: 90 },  color: C.ink,    desc: 'Route surfaces — Calendar, Jobs, Technicians', files: 4 },
  { id: 'nav',      label: 'Navigation',    pos: { x: 110, y: 230 }, color: C.ink,    desc: 'Region-based routes',                            files: 2 },
  { id: 'mvux',     label: 'State (MVUX)',  pos: { x: 380, y: 90 },  color: C.indigo, desc: 'Feeds, States, Selection',                       files: 6 },
  { id: 'services', label: 'Services',      pos: { x: 380, y: 230 }, color: C.ink2,   desc: 'Job, Technician, Schedule',                      files: 5 },
  { id: 'http',     label: 'HTTP (Kiota)',  pos: { x: 650, y: 90 },  color: C.ink2,   desc: 'Typed clients, generated',                       files: 3 },
  { id: 'storage',  label: 'Storage',       pos: { x: 650, y: 230 }, color: C.ink2,   desc: 'Local cache, offline-first',                     files: 2 },
];
const BASE_EDGES = [
  { from: 'pages', to: 'mvux', label: 'binds' },
  { from: 'pages', to: 'nav', label: 'requests' },
  { from: 'mvux', to: 'services', label: 'consumes' },
  { from: 'services', to: 'http', label: 'calls' },
  { from: 'services', to: 'storage', label: 'persists' },
];

// Apply ctx to base modules — substitute the entity noun in the Services
// description, and drop HTTP module + its edge when intent is offline-first.
function deriveArchitecture(ctx) {
  const e = ctx.entityTitle;
  let modules = BASE_MODULES.map((m) => {
    if (m.id === 'services' && !ctx.isFieldService) {
      return { ...m, desc: `${e}, ${ctx.userNoun.charAt(0).toUpperCase() + ctx.userNoun.slice(1)}, Schedule` };
    }
    return m;
  });
  let edges = BASE_EDGES;
  if (ctx.isOfflineFirst) {
    modules = modules.filter((m) => m.id !== 'http');
    edges = edges.filter((e) => e.to !== 'http');
  }
  return { modules, edges };
}

function ArchitectureCanvas({ ctx }) {
  const { modules: MODULES, edges: EDGES } = useMemo(() => deriveArchitecture(ctx), [ctx]);
  const [hovered, setHovered] = useState(null);

  // Compute the set of connected module IDs and edge keys for the hovered module
  const connected = useMemo(() => {
    if (!hovered) return null;
    const modSet = new Set([hovered]);
    const edgeSet = new Set();
    EDGES.forEach((e) => {
      if (e.from === hovered || e.to === hovered) {
        edgeSet.add(`${e.from}-${e.to}`);
        modSet.add(e.from);
        modSet.add(e.to);
      }
    });
    return { modules: modSet, edges: edgeSet };
  }, [hovered, EDGES]);

  const isModConnected = (id) => !connected || connected.modules.has(id);
  const isEdgeConnected = (e) => !connected || connected.edges.has(`${e.from}-${e.to}`);

  // Edge connection point — picks the right side of each rect based on relative position
  const edgeXY = (edge) => {
    const f = MODULES.find((m) => m.id === edge.from);
    const t = MODULES.find((m) => m.id === edge.to);
    const x1 = f.pos.x + (t.pos.x > f.pos.x ? 60 : t.pos.x < f.pos.x ? -60 : 0);
    const y1 = f.pos.y + (t.pos.y > f.pos.y ? 22 : t.pos.y < f.pos.y ? -22 : 0);
    const x2 = t.pos.x + (f.pos.x > t.pos.x ? 60 : f.pos.x < t.pos.x ? -60 : 0);
    const y2 = t.pos.y + (f.pos.y > t.pos.y ? 22 : f.pos.y < t.pos.y ? -22 : 0);
    return { x1, y1, x2, y2, mx: (x1 + x2) / 2, my: (y1 + y2) / 2 };
  };

  const hoveredModule = hovered ? MODULES.find((m) => m.id === hovered) : null;

  return (
    <div>
      <SectionHeader
        filename="blueprint.svg"
        badge={ctx.isFieldService ? 'DEMO CONTENT · LOCKED CONTEXT' : 'LOCKED CONTEXT'}
        badgeColor={ctx.isFieldService ? C.amber : C.amber}
        action={<GhostButton small>↻ Regenerate</GhostButton>}
      />
      <div style={{ padding: '24px 0', borderBottom: `1px solid ${C.hairline}` }}>
        <svg width="100%" height={340} viewBox="0 0 800 340" style={{ display: 'block', margin: '0 auto', maxWidth: 800 }}>
          <defs>
            {/* Hand-drawn wobble filter — subtle turbulence + displacement */}
            <filter id="rough-arch" x="-3%" y="-3%" width="106%" height="106%">
              <feTurbulence type="fractalNoise" baseFrequency="0.022" numOctaves="2" seed="3" result="turb" />
              <feDisplacementMap in="SourceGraphic" in2="turb" scale="1.5" />
            </filter>
            {/* Slightly stronger filter for emphasis on hover */}
            <filter id="rough-arch-bold" x="-3%" y="-3%" width="106%" height="106%">
              <feTurbulence type="fractalNoise" baseFrequency="0.018" numOctaves="2" seed="5" result="turb2" />
              <feDisplacementMap in="SourceGraphic" in2="turb2" scale="2" />
            </filter>
            <pattern id="archGridV8" width="20" height="20" patternUnits="userSpaceOnUse">
              <path d="M 20 0 L 0 0 0 20" fill="none" stroke={C.hairline} strokeWidth="0.5" opacity="0.6" />
            </pattern>
          </defs>
          <rect width="800" height="340" fill="url(#archGridV8)" />

          {/* EDGES — render first so modules sit on top */}
          {EDGES.map((edge) => {
            const { x1, y1, x2, y2, mx, my } = edgeXY(edge);
            const isConn = isEdgeConnected(edge);
            const opacity = !connected ? 0.65 : (isConn ? 1 : 0.12);
            const stroke = !connected ? C.ink3 : (isConn ? C.ink : C.ink4);
            const strokeWidth = isConn && connected ? 1.8 : 1;
            return (
              <g key={`${edge.from}-${edge.to}`}
                opacity={opacity}
                style={{ transition: 'opacity 240ms ease' }}>
                <line x1={x1} y1={y1} x2={x2} y2={y2}
                  stroke={stroke} strokeWidth={strokeWidth}
                  strokeDasharray={isConn && connected ? '0' : '4 3'}
                  strokeLinecap="round"
                  filter="url(#rough-arch)"
                  style={{ transition: 'all 240ms ease' }} />
                <rect x={mx - 32} y={my - 8} width={64} height={16} fill={C.paper} rx={2} />
                <text x={mx} y={my + 4} textAnchor="middle"
                  style={{
                    fontFamily: SANS, fontSize: 10, fontStyle: 'italic',
                    fill: isConn && connected ? C.ink : C.ink3,
                    fontWeight: isConn && connected ? 600 : 400,
                    transition: 'all 240ms ease',
                  }}>
                  {edge.label}
                </text>
              </g>
            );
          })}

          {/* MODULES — hand-drawn rects with hover state and labels rendered outside the filter */}
          {MODULES.map((m) => {
            const isHov = hovered === m.id;
            const isConn = isModConnected(m.id);
            const opacity = !connected ? 1 : (isConn ? 1 : 0.25);
            return (
              <g key={m.id} transform={`translate(${m.pos.x}, ${m.pos.y})`}
                onMouseEnter={() => setHovered(m.id)} onMouseLeave={() => setHovered(null)}
                style={{ cursor: 'pointer', transition: 'opacity 240ms ease' }}
                opacity={opacity}>
                {/* Filter applied only to rect so text stays crisp */}
                <rect x={-60} y={-22} width={120} height={44} rx={6}
                  fill={isHov ? m.color : C.paper}
                  stroke={m.color}
                  strokeWidth={isHov ? 2 : 1.3}
                  filter={isHov ? 'url(#rough-arch-bold)' : 'url(#rough-arch)'}
                  style={{ transition: 'fill 200ms ease, stroke-width 200ms ease' }} />
                <text x={0} y={4} textAnchor="middle"
                  style={{
                    fontFamily: SANS, fontSize: 11, fontWeight: isHov ? 600 : 500,
                    letterSpacing: '0.02em',
                    fill: isHov ? C.paper : m.color,
                    transition: 'fill 200ms ease',
                    pointerEvents: 'none',
                  }}>
                  {m.label}
                </text>
                {/* File count badge — only visible on hover */}
                {isHov && (
                  <g style={{ pointerEvents: 'none' }}>
                    <rect x={42} y={-32} width={28} height={14} rx={7} fill={C.paper} stroke={m.color} strokeWidth={1} />
                    <text x={56} y={-22} textAnchor="middle"
                      style={{ fontFamily: MONO, fontSize: 9, fontWeight: 600, fill: m.color }}>
                      {m.files}f
                    </text>
                  </g>
                )}
              </g>
            );
          })}
        </svg>
      </div>

      {/* Hover detail panel — richer than v7 */}
      <div style={{ padding: '14px 0', minHeight: 64 }}>
        {hoveredModule ? (
          <div>
            <div style={{ display: 'flex', alignItems: 'baseline', gap: 12, marginBottom: 4, flexWrap: 'wrap' }}>
              <Eyebrow size={10} color={C.ink2} weight={600}>
                {hoveredModule.label}
              </Eyebrow>
              <Eyebrow size={9} color={C.ink4} weight={500}>
                {hoveredModule.files} {hoveredModule.files === 1 ? 'file' : 'files'} · {connected.edges.size} {connected.edges.size === 1 ? 'connection' : 'connections'}
              </Eyebrow>
            </div>
            <Body color={C.ink2} size={13} style={{ fontStyle: 'italic' }}>
              {hoveredModule.desc}
            </Body>
          </div>
        ) : (
          <Body color={C.ink4} size={13} style={{ fontStyle: 'italic' }}>
            Hover any module to trace its connections. Each module becomes a folder; each line, a binding contract.
          </Body>
        )}
      </div>

      <CodeBlock
        label="solution-tree.txt"
        source="DERIVED FROM BLUEPRINT"
        language="tree"
        theme="light"
        code={`${ctx.appName}/
├── ${ctx.appName}/                   # shared
│   ├── Models/
│   ├── Presentation/                # MVUX feeds
│   ├── Services/
│   ├── Views/
│   └── App.xaml
├── ${ctx.appName}.Mobile/             # iOS · Android
├── ${ctx.appName}.Desktop/            # WinAppSDK · Mac
├── ${ctx.appName}.Skia.Wpf/
├── ${ctx.appName}.Wasm/
└── ${ctx.appName}.Tests/`}
      />
      <Annotation label="Why this fits">
        MVUX gives both views identical reactive feeds with offline-first
        state — no MVVM ceremony, no Rx plumbing. Navigation regions let
        Shell and TabBar animate independently, which the dispatch flow
        in §02 depends on.
      </Annotation>
      <Annotation label="Agent prompt" voice="quote">
        When generating XAML, use uen:Region.Attached for navigation surfaces
        and bind ItemsSource to JobsModel.Jobs (an IFeed). Never construct
        ViewModels in code-behind.
      </Annotation>
    </div>
  );
}

// ─── DESIGN canvas ───────────────────────────────────────────────────────

const DESIGN_DEFAULTS = {
  surface: '#0c0d0f', action: '#c89c3f', info: '#7ab3df', success: '#9bbf9d',
  warn: '#e87f6d', panel: '#16181c', tag: '#b288c4', locked: '#0c0d0f',
  bodyFont: 'Inter',
};

function DesignCanvas({ design, setDesign, onDirty, layerState, previewDesign }) {
  const isPreviewing = layerState === 'previewing';
  const displayed = isPreviewing ? previewDesign : design;
  const colors = [
    { key: 'surface', name: 'Surface' }, { key: 'action', name: 'Action' },
    { key: 'info', name: 'Info' }, { key: 'success', name: 'Success' },
    { key: 'warn', name: 'Warn' }, { key: 'panel', name: 'Panel' },
    { key: 'tag', name: 'Tag' }, { key: 'locked', name: 'Locked' },
  ];
  const upper = (h) => h.toUpperCase();
  const xaml = `<ResourceDictionary
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

  <ResourceDictionary.ThemeDictionaries>

    <ResourceDictionary x:Key="Light">
      <Color x:Key="PrimaryColor">#1A1A1A</Color>
      <Color x:Key="OnPrimaryColor">#FAFAFA</Color>
      <Color x:Key="SecondaryColor">${upper(displayed.action)}</Color>
      <Color x:Key="OnSecondaryColor">#1A1A1A</Color>
      <Color x:Key="SurfaceColor">#FAFAFA</Color>
      <Color x:Key="OnSurfaceColor">#1A1A1A</Color>
      <Color x:Key="BackgroundColor">#FFFFFF</Color>
      <Color x:Key="ErrorColor">${upper(displayed.warn)}</Color>
    </ResourceDictionary>

    <ResourceDictionary x:Key="Dark">
      <Color x:Key="PrimaryColor">#FAFAFA</Color>
      <Color x:Key="OnPrimaryColor">${upper(displayed.surface)}</Color>
      <Color x:Key="SecondaryColor">${upper(displayed.action)}</Color>
      <Color x:Key="OnSecondaryColor">#1A1A1A</Color>
      <Color x:Key="SurfaceColor">${upper(displayed.panel)}</Color>
      <Color x:Key="OnSurfaceColor">#FAFAFA</Color>
      <Color x:Key="BackgroundColor">${upper(displayed.surface)}</Color>
      <Color x:Key="ErrorColor">${upper(displayed.warn)}</Color>
    </ResourceDictionary>

  </ResourceDictionary.ThemeDictionaries>
</ResourceDictionary>`;
  return (
    <div>
      <SectionHeader
        filename="design-tokens.md"
        badge={isPreviewing ? 'PREVIEW' : 'EDITING'}
        badgeColor={isPreviewing ? C.amber : C.ink3}
      />
      {/* Color tokens — tabular grid, hex inline */}
      <div style={{ marginBottom: 24 }}>
        <Eyebrow size={10} color={C.ink3} weight={500} style={{ display: 'block', marginBottom: 12 }}>
          Color tokens
        </Eyebrow>
        <div style={{ display: 'grid', gridTemplateColumns: '24px 1fr 100px 80px', gap: '0 16px' }}>
          {colors.map((c, i) => {
            const value = displayed[c.key];
            const original = design[c.key];
            const proposed = isPreviewing && value !== original;
            const isAction = c.key === 'action';
            return (
              <React.Fragment key={c.key}>
                <div style={{
                  alignSelf: 'center',
                  padding: '10px 0',
                  borderTop: i > 0 ? `1px solid ${C.hairline2}` : 'none',
                }}>
                  {isPreviewing ? (
                    <div style={{
                      width: 20, height: 20, background: value, borderRadius: 4,
                      border: isAction ? `1.5px solid ${C.amber}` : `1px solid ${C.hairline}`,
                    }} />
                  ) : (
                    <input type="color" value={value}
                      onChange={(e) => { setDesign({ ...design, [c.key]: e.target.value }); onDirty(); }}
                      style={{
                        width: 20, height: 20, padding: 0,
                        border: isAction ? `1.5px solid ${C.amber}` : `1px solid ${C.hairline}`,
                        borderRadius: 4, cursor: 'pointer', background: 'transparent',
                      }} />
                  )}
                </div>
                <div style={{ alignSelf: 'center', padding: '10px 0', borderTop: i > 0 ? `1px solid ${C.hairline2}` : 'none' }}>
                  <Body color={C.ink} size={13} weight={500}>{c.name}</Body>
                </div>
                <div style={{ alignSelf: 'center', padding: '10px 0', borderTop: i > 0 ? `1px solid ${C.hairline2}` : 'none' }}>
                  <Mono color={C.ink3} size={12}>{value}</Mono>
                </div>
                <div style={{ alignSelf: 'center', padding: '10px 0', borderTop: i > 0 ? `1px solid ${C.hairline2}` : 'none', textAlign: 'right' }}>
                  {proposed && <Eyebrow size={9} color={C.amber} weight={600}>was {original}</Eyebrow>}
                </div>
              </React.Fragment>
            );
          })}
        </div>
      </div>
      {/* Type scale + control gallery */}
      <div style={{ marginBottom: 18 }}>
        <Eyebrow size={10} color={C.ink3} weight={500} style={{ display: 'block', marginBottom: 12 }}>
          Type scale
        </Eyebrow>
        <div style={{ display: 'grid', gridTemplateColumns: '60px 1fr 50px', gap: '12px 16px', alignItems: 'baseline', marginBottom: 18 }}>
          <Eyebrow size={9} color={C.ink4}>Display</Eyebrow>
          <span style={{ fontFamily: SANS, fontSize: 26, color: C.ink, fontWeight: 500, letterSpacing: '-0.015em' }}>
            Today's schedule
          </span>
          <Mono color={C.ink4} size={10}>26px</Mono>
          <Eyebrow size={9} color={C.ink4}>Heading</Eyebrow>
          <Mono color={C.ink} size={15} weight={500}>Job #4471 · Boiler service</Mono>
          <Mono color={C.ink4} size={10}>15px</Mono>
          <Eyebrow size={9} color={C.ink4}>Body</Eyebrow>
          <Body color={C.ink2} size={13}>Tap a job to assign a technician. Conflicts surface inline.</Body>
          <Mono color={C.ink4} size={10}>13px</Mono>
          <Eyebrow size={9} color={C.ink4}>Caption</Eyebrow>
          <Eyebrow color={C.ink3} size={10}>ARRIVED · 09:14 SYNCED</Eyebrow>
          <Mono color={C.ink4} size={10}>10px</Mono>
        </div>
      </div>
      <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12, marginBottom: 8 }}>
        <div style={{ border: `1px solid ${C.hairline}`, borderRadius: 6, padding: 12 }}>
          <Eyebrow size={9} color={C.ink3} weight={500} style={{ display: 'block', marginBottom: 10 }}>
            Primary
          </Eyebrow>
          <span style={{
            display: 'inline-block', background: displayed.action, color: C.paper4,
            fontFamily: SANS, fontSize: 12, fontWeight: 500,
            padding: '6px 12px', borderRadius: 5, marginRight: 6,
          }}>Assign tech</span>
          <span style={{
            display: 'inline-block', background: 'transparent', color: C.ink2,
            border: `1px solid ${C.hairline}`,
            fontFamily: SANS, fontSize: 12, fontWeight: 500,
            padding: '6px 12px', borderRadius: 5,
          }}>View map</span>
        </div>
        <div style={{ border: `1px solid ${C.hairline}`, borderRadius: 6, padding: 12 }}>
          <Eyebrow size={9} color={C.ink3} weight={500} style={{ display: 'block', marginBottom: 10 }}>
            Tab Bar · Toolkit
          </Eyebrow>
          <div style={{ display: 'flex', border: `1px solid ${C.hairline}`, borderRadius: 5, overflow: 'hidden' }}>
            {['Today', 'Week', 'Map'].map((t, i) => (
              <span key={t} style={{
                padding: '5px 10px', fontFamily: SANS, fontSize: 11, fontWeight: 500,
                background: i === 0 ? displayed.action : 'transparent',
                color: i === 0 ? C.paper4 : C.ink2,
                flex: 1, textAlign: 'center',
              }}>{t}</span>
            ))}
          </div>
        </div>
      </div>
      <CodeBlock label="ColorPaletteOverride.xaml" source="GENERATED FROM TOKEN MAP" code={xaml} />
      <Annotation label="Why this palette">
        Outdoor sun + a phone in a truck mount means low chroma, high
        contrast. Amber is the only saturated hue and is reserved for
        the next action — never decoration.
      </Annotation>
      <Annotation label="Agent prompt" voice="quote">
        Apply tokens via ThemeResource — never hex. Reference SurfaceBrush,
        OnSurfaceBrush, SecondaryBrush. Light and Dark theme dictionaries
        let the same XAML render correctly in both themes.
      </Annotation>
    </div>
  );
}

// ─── INTERACTIONS canvas — v8 state transition diagram ─────────────────

// 6 states laid out as a 3x2 grid, color-coded by category.
// Coordinates correspond to centers of 120w x 44h pill-shaped rects.
const STATES = {
  default: {
    pos: { x: 130, y: 100 }, label: 'Default', color: C.ink,
    description: 'Empty calendar; primary CTA visible.',
  },
  loading: {
    pos: { x: 400, y: 100 }, label: 'Loading', color: C.indigo,
    description: 'Skeleton rows; CTA disabled.',
  },
  success: {
    pos: { x: 670, y: 100 }, label: 'Success', color: '#7a9b6e',
    description: 'Toast confirmation; new row animates in.',
  },
  empty: {
    pos: { x: 400, y: 240 }, label: 'Empty', color: C.ink3,
    description: 'No technicians available; suggest filters.',
  },
  error: {
    pos: { x: 670, y: 240 }, label: 'Error', color: '#b04534',
    description: 'Validation conflict; inline message + retry.',
  },
  offline: {
    pos: { x: 130, y: 240 }, label: 'Offline', color: C.amber,
    description: 'Queued locally; banner indicates sync pending.',
  },
};

// Transitions with explicit SVG paths and label positions — pre-computed
// so each curve reads cleanly without overlapping its inverse.
const TRANSITIONS = [
  { from: 'default', to: 'loading', label: 'submit',
    path: 'M 190 100 L 340 100', labelX: 265, labelY: 100 },
  { from: 'loading', to: 'success', label: 'resolves',
    path: 'M 460 100 L 610 100', labelX: 535, labelY: 100 },
  { from: 'loading', to: 'error', label: 'rejects',
    path: 'M 460 110 Q 510 200 615 222', labelX: 510, labelY: 180 },
  { from: 'loading', to: 'empty', label: 'no data',
    path: 'M 400 122 L 400 218', labelX: 400, labelY: 170 },
  { from: 'error', to: 'loading', label: 'retry',
    path: 'M 615 218 Q 540 145 460 92', labelX: 540, labelY: 145 },
  { from: 'default', to: 'offline', label: 'disconnect',
    path: 'M 130 122 L 130 218', labelX: 130, labelY: 170 },
  { from: 'offline', to: 'loading', label: 'reconnect',
    path: 'M 190 230 Q 280 165 340 110', labelX: 270, labelY: 158 },
  { from: 'success', to: 'default', label: 'reset',
    path: 'M 670 78 Q 400 12 130 78', labelX: 400, labelY: 24 },
];

function InteractionsCanvas({ ctx }) {
  // The primary flow's id stays stable for state-machine compatibility,
  // but the label reflects the user's entity. "Create job" → "Create habit".
  const primaryFlowLabel = ctx.entityNoun === 'job'
    ? 'Create job'
    : `Create ${ctx.entityNoun}`;
  const flows = [
    { id: 'create-job', label: primaryFlowLabel },
    { id: 'sign-in', label: 'Sign in' },
    { id: 'sync', label: 'Sync data' },
  ];
  const [activeFlow, setActiveFlow] = useState('create-job');
  const [hoveredState, setHoveredState] = useState(null);
  const [activeState, setActiveState] = useState('default');

  // Hovered state highlights its incoming and outgoing transitions
  const connected = useMemo(() => {
    if (!hoveredState) return null;
    const states = new Set([hoveredState]);
    const trans = new Set();
    TRANSITIONS.forEach((t) => {
      if (t.from === hoveredState || t.to === hoveredState) {
        trans.add(`${t.from}-${t.to}`);
        states.add(t.from);
        states.add(t.to);
      }
    });
    return { states, trans };
  }, [hoveredState]);

  const isStateConnected = (id) => !connected || connected.states.has(id);
  const isTransConnected = (t) => !connected || connected.trans.has(`${t.from}-${t.to}`);

  // Show hovered state in the detail panel (preview), otherwise the active state
  const detailState = hoveredState ? STATES[hoveredState] : STATES[activeState];
  const showingHoverPreview = hoveredState && hoveredState !== activeState;

  return (
    <div>
      <SectionHeader
        filename="interaction-states.md"
        badge={ctx.isFieldService ? 'DEMO CONTENT · 3 FLOWS · 6 STATES' : '3 FLOWS · 6 STATES'}
        badgeColor={ctx.isFieldService ? C.amber : C.ink3}
        action={
          <div style={{ display: 'flex', gap: 4 }}>
            {flows.map((f) => (
              <button key={f.id} onClick={() => setActiveFlow(f.id)} style={{
                fontFamily: SANS, fontSize: 11, fontWeight: 500,
                padding: '4px 10px', borderRadius: 5,
                background: activeFlow === f.id ? C.ink : 'transparent',
                color: activeFlow === f.id ? C.paper : C.ink2,
                border: `1px solid ${activeFlow === f.id ? C.ink : C.hairline}`,
                cursor: 'pointer',
              }}>{f.label}</button>
            ))}
          </div>
        }
      />

      {/* State transition diagram */}
      <div style={{ padding: '24px 0', borderBottom: `1px solid ${C.hairline}` }}>
        <svg width="100%" height={340} viewBox="0 0 800 340" style={{ display: 'block', margin: '0 auto', maxWidth: 800 }}>
          <defs>
            <filter id="rough-int" x="-3%" y="-3%" width="106%" height="106%">
              <feTurbulence type="fractalNoise" baseFrequency="0.02" numOctaves="2" seed="7" result="turb" />
              <feDisplacementMap in="SourceGraphic" in2="turb" scale="1.3" />
            </filter>
            <filter id="rough-int-bold" x="-3%" y="-3%" width="106%" height="106%">
              <feTurbulence type="fractalNoise" baseFrequency="0.018" numOctaves="2" seed="11" result="turb2" />
              <feDisplacementMap in="SourceGraphic" in2="turb2" scale="1.8" />
            </filter>
            <marker id="arrow-int" viewBox="0 0 10 10" refX="9" refY="5"
              markerWidth="7" markerHeight="7" orient="auto-start-reverse">
              <path d="M 0 1 L 9 5 L 0 9 z" fill={C.ink3} />
            </marker>
            <marker id="arrow-int-bold" viewBox="0 0 10 10" refX="9" refY="5"
              markerWidth="7" markerHeight="7" orient="auto-start-reverse">
              <path d="M 0 1 L 9 5 L 0 9 z" fill={C.ink} />
            </marker>
            <pattern id="intGrid" width="20" height="20" patternUnits="userSpaceOnUse">
              <path d="M 20 0 L 0 0 0 20" fill="none" stroke={C.hairline} strokeWidth="0.5" opacity="0.5" />
            </pattern>
          </defs>
          <rect width="800" height="340" fill="url(#intGrid)" />

          {/* TRANSITIONS — render first so states sit on top */}
          {TRANSITIONS.map((t) => {
            const isConn = isTransConnected(t);
            const opacity = !connected ? 0.7 : (isConn ? 1 : 0.1);
            const stroke = !connected ? C.ink3 : (isConn ? C.ink : C.ink4);
            const strokeWidth = isConn && connected ? 1.8 : 1.1;
            const markerEnd = isConn && connected ? 'url(#arrow-int-bold)' : 'url(#arrow-int)';
            return (
              <g key={`${t.from}-${t.to}`}
                opacity={opacity}
                style={{ transition: 'opacity 240ms ease' }}>
                <path d={t.path}
                  fill="none"
                  stroke={stroke}
                  strokeWidth={strokeWidth}
                  strokeLinecap="round"
                  markerEnd={markerEnd}
                  filter="url(#rough-int)"
                  style={{ transition: 'all 240ms ease' }} />
                <rect x={t.labelX - 30} y={t.labelY - 7} width={60} height={14} fill={C.paper} rx={2} />
                <text x={t.labelX} y={t.labelY + 3} textAnchor="middle"
                  style={{
                    fontFamily: SANS, fontSize: 10, fontStyle: 'italic',
                    fill: isConn && connected ? C.ink : C.ink3,
                    fontWeight: isConn && connected ? 600 : 400,
                  }}>
                  {t.label}
                </text>
              </g>
            );
          })}

          {/* STATES — pill-shaped rects, click to set active, hover to preview */}
          {Object.entries(STATES).map(([id, s]) => {
            const isHov = hoveredState === id;
            const isActive = activeState === id;
            const isConn = isStateConnected(id);
            const opacity = !connected ? 1 : (isConn ? 1 : 0.25);
            const filled = isHov || isActive;
            return (
              <g key={id} transform={`translate(${s.pos.x}, ${s.pos.y})`}
                onMouseEnter={() => setHoveredState(id)}
                onMouseLeave={() => setHoveredState(null)}
                onClick={() => setActiveState(id)}
                style={{ cursor: 'pointer', transition: 'opacity 240ms ease' }}
                opacity={opacity}>
                <rect x={-60} y={-22} width={120} height={44} rx={22}
                  fill={filled ? s.color : C.paper}
                  stroke={s.color}
                  strokeWidth={filled ? 2 : 1.3}
                  filter={isHov ? 'url(#rough-int-bold)' : 'url(#rough-int)'}
                  style={{ transition: 'fill 200ms ease, stroke-width 200ms ease' }} />
                {/* Pulsing dot on the active state when not hovered */}
                {isActive && !isHov && (
                  <circle cx={48} cy={-12} r={3.5} fill={C.amber}>
                    <animate attributeName="opacity" values="1;0.35;1" dur="1.6s" repeatCount="indefinite" />
                  </circle>
                )}
                <text x={0} y={4} textAnchor="middle"
                  style={{
                    fontFamily: SANS, fontSize: 11, fontWeight: filled ? 600 : 500,
                    letterSpacing: '0.02em',
                    fill: filled ? C.paper : s.color,
                    transition: 'fill 200ms ease',
                    pointerEvents: 'none',
                  }}>
                  {s.label}
                </text>
              </g>
            );
          })}
        </svg>
      </div>

      {/* Detail panel — Annotation style with hover-preview indicator */}
      <div style={{ paddingLeft: 16, paddingTop: 14, borderLeft: `1px solid ${showingHoverPreview ? C.amber : C.hairline}`, marginTop: 14, transition: 'border-color 200ms ease' }}>
        <div style={{ display: 'flex', alignItems: 'baseline', gap: 12, marginBottom: 4, flexWrap: 'wrap' }}>
          <Eyebrow size={10} color={showingHoverPreview ? C.amber : C.ink2} weight={600}>
            {detailState.label} state
          </Eyebrow>
          {showingHoverPreview && (
            <Eyebrow size={9} color={C.ink4} weight={500}>
              hover preview · click to select
            </Eyebrow>
          )}
          {!hoveredState && (
            <Eyebrow size={9} color={C.ink4} weight={500}>
              hover any state to trace its transitions
            </Eyebrow>
          )}
        </div>
        <Body color={C.ink} size={14} style={{ fontStyle: 'italic' }}>
          {detailState.description}
        </Body>
      </div>

      <Annotation label="Why this matters">
        Offline-first means six states aren't optional — they're load-bearing.
        A technician in a basement with no signal needs the queued state to
        feel as resolved as success.
      </Annotation>
      <Annotation label="Agent prompt" voice="quote">
        Every screen must implement all six states. Use VisualStateManager
        groups named exactly: Default, Loading, Empty, Error, Success, Offline.
      </Annotation>
    </div>
  );
}

// ─── DATA canvas ────────────────────────────────────────────────────────

function DataCanvas({ ctx }) {
  const e = ctx.entityTitle;
  const eId = `${ctx.entityNoun}Id`;
  const userTitle = ctx.userNoun.charAt(0).toUpperCase() + ctx.userNoun.slice(1);
  // Substitute primary entity. Keep the secondary entities (User/Schedule)
  // unchanged so the data model still reads as a real domain rather than
  // a single-entity demo.
  const entities = ctx.isFieldService ? [
    { name: 'Job', fields: [['id', 'string'], ['title', 'string'], ['status', 'enum'], ['scheduledAt', 'datetime'], ['technicianId', 'string'], ['notes', 'string?']] },
    { name: 'Technician', fields: [['id', 'string'], ['name', 'string'], ['skills', 'string[]'], ['available', 'bool']] },
    { name: 'Schedule', fields: [['date', 'date'], ['jobs', 'Job[]'], ['conflicts', 'Conflict[]']] },
  ] : [
    { name: e, fields: [['id', 'string'], ['title', 'string'], ['status', 'enum'], ['scheduledAt', 'datetime'], [eId === 'id' ? 'ownerId' : `${ctx.userNoun.replace(/s$/, '')}Id`, 'string'], ['notes', 'string?']] },
    { name: userTitle, fields: [['id', 'string'], ['name', 'string'], ['skills', 'string[]'], ['available', 'bool']] },
    { name: 'Schedule', fields: [['date', 'date'], [ctx.entityPlural, `${e}[]`], ['conflicts', 'Conflict[]']] },
  ];
  const ownerField = ctx.isFieldService ? 'TechnicianId' : `${ctx.userNoun.replace(/s$/, '').charAt(0).toUpperCase()}${ctx.userNoun.replace(/s$/, '').slice(1)}Id`;
  return (
    <div>
      <SectionHeader
        filename="data-contracts.md"
        badge={ctx.isFieldService ? 'DEMO CONTENT · 3 RECORDS' : '3 RECORDS'}
        badgeColor={ctx.isFieldService ? C.amber : C.ink3}
      />
      <div>
        {entities.map((e) => (
          <div key={e.name} style={{ paddingTop: 14, paddingBottom: 14, borderBottom: `1px solid ${C.hairline}` }}>
            <div style={{ display: 'flex', alignItems: 'baseline', gap: 10, marginBottom: 10 }}>
              <Mono color={C.ink} size={13} weight={600}>{e.name}</Mono>
              <Eyebrow size={10} color={C.ink4}>record</Eyebrow>
            </div>
            <div style={{ display: 'grid', gridTemplateColumns: '160px 1fr', gap: '4px 16px' }}>
              {e.fields.map(([f, t]) => (
                <React.Fragment key={f}>
                  <Mono color={C.ink2} size={12}>{f}</Mono>
                  <Mono color={C.indigo} size={12}>{t}</Mono>
                </React.Fragment>
              ))}
            </div>
          </div>
        ))}
      </div>
      <CodeBlock
        label={`Models/${e}.cs`}
        source="GENERATED FROM CONTRACTS"
        code={`public partial record ${e}(
  string Id,
  string Title,
  ${e}Status Status,
  DateTime ScheduledAt,
  string ${ownerField},
  string? Notes);`}
      />
      <Annotation label="Why this shape">
        Records, not classes — immutability + structural equality is what
        MVUX feeds need to detect change cheaply. Nullability is explicit
        because a missing Notes field shouldn't be a bug.
      </Annotation>
      <Annotation label="Agent prompt" voice="quote">
        Use C# records for all domain types. Mutations go through commands
        on the model, not setters. The "?" in field types is load-bearing — preserve it.
      </Annotation>
    </div>
  );
}

// ─── IMPLEMENTATION canvas ──────────────────────────────────────────────

function ImplementationCanvas() {
  const phases = [
    { num: 1, label: 'Scaffold', accent: C.phaseRed, title: 'Solution skeleton', desc: 'Multi-head Uno solution wired with MVUX + Uno.Extensions.', files: ['FieldDispatch.sln', 'Directory.Packages.props', '4 platform heads'], prompt: 'Run dotnet new unoapp. Verify each head compiles.' },
    { num: 2, label: 'Shell', accent: C.phaseBlue, title: 'Shell & navigation', desc: 'Two regions: top-level Shell and inner TabBar. Routes registered.', files: ['Views/Shell.xaml', 'MainPage.xaml', 'Navigation/RouteMap.cs'], prompt: 'Wire deeplink for jobs/{id}.' },
    { num: 3, label: 'Domain', accent: C.phasePurple, title: 'Models & feeds', desc: 'WorkOrder, Technician, Conflict. JobsModel with IFeed + IListFeed.', files: ['Models/WorkOrder.cs', 'Models/Technician.cs', 'Presentation/JobsModel.cs'], prompt: 'Records immutable. Mutations through commands on the model.' },
    { num: 4, label: 'Screens', accent: C.phasePink, title: 'Screens & bindings', desc: 'Today, Week, Map, Job-detail. Tokens from §04 — no hardcoded colors.', files: ['Views/TodayPage.xaml', 'Views/WeekPage.xaml', 'Views/JobDetailPage.xaml'], prompt: 'Drop into Hot Design after compile — verify spacing scale.' },
    { num: 5, label: 'States', accent: C.phaseGreen, title: 'Interaction states', desc: 'All six states from §05 wired into VSM groups per screen.', files: ['Per-screen VSM definitions', 'Views/States/*.xaml'], prompt: 'Test offline state with airplane mode; queued indicator must surface.' },
    { num: 6, label: 'Polish', accent: C.phaseAmber, title: 'Tests & accessibility', desc: 'UI tests, contrast verification, performance budgets, bundle profiling.', files: ['Tests/*.UI.cs', 'perf harness', 'A11y audit'], prompt: 'Each screen: keyboard nav, contrast 4.5:1, sub-100ms first paint.' },
  ];
  return (
    <div>
      <SectionHeader filename="implementation-plan.md" badge="6 PHASES" badgeColor={C.ink3} />
      <div>
        {phases.map((p, i) => (
          <div key={p.num} style={{
            display: 'grid', gridTemplateColumns: '40px 140px 1fr 240px',
            gap: 20, padding: '16px 0',
            borderBottom: i < phases.length - 1 ? `1px solid ${C.hairline}` : 'none',
            alignItems: 'baseline',
          }}>
            <div>
              <Eyebrow size={10} color={p.accent} weight={600}>P{p.num}</Eyebrow>
            </div>
            <div>
              <Eyebrow size={10} color={p.accent} weight={600} style={{ display: 'block', marginBottom: 2 }}>{p.label}</Eyebrow>
              <Body color={C.ink} size={14} weight={500}>{p.title}</Body>
            </div>
            <div>
              <Body color={C.ink2} size={13} style={{ display: 'block', marginBottom: 6 }}>{p.desc}</Body>
              <div style={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
                {p.files.map((f) => (
                  <Mono key={f} color={C.ink3} size={11}>+ {f}</Mono>
                ))}
              </div>
            </div>
            <div style={{ paddingLeft: 12, borderLeft: `1px solid ${C.hairline}` }}>
              <Eyebrow size={9} color={C.ink3} weight={500} style={{ display: 'block', marginBottom: 4 }}>Agent prompt</Eyebrow>
              <span style={{ fontFamily: SANS, fontSize: 12, color: C.ink3, fontStyle: 'italic', lineHeight: 1.5 }}>
                "{p.prompt}"
              </span>
            </div>
          </div>
        ))}
      </div>
      <Annotation label="Why this phasing">
        Linear with one parallel option (Domain + Screens) keeps the agent
        from rebuilding scaffolding when a domain change cascades. Each
        phase has a verifiable acceptance step before the next is unlocked.
      </Annotation>
    </div>
  );
}

// ─── SCAFFOLD canvas ────────────────────────────────────────────────────

function ScaffoldCanvas({ intent, design, overrides, onShip }) {
  const [copied, setCopied] = useState(false);
  const [ctxCopied, setCtxCopied] = useState(false);
  const [downloaded, setDownloaded] = useState(false);
  const command = `dotnet new unoapp \\
  -n ${(intent.appType || 'App').replace(/\s+/g, '')} \\
  --tfm net10.0 \\
  --platforms wasm,ios,android \\
  --markup xaml --presentation mvux --theme material \\
  --features config,http,logging,nav,mvux`;

  // Build the full prompt-context document from all layers' markdown,
  // using user overrides where set, generated content otherwise.
  const buildPromptContext = () => {
    const state = { intent, design };
    return LAYERS.map((layer) => {
      const gen = MARKDOWN_GEN[layer.id]?.(state) ?? '';
      const ov = overrides?.[layer.id];
      const content = ov !== undefined ? ov : gen;
      return `<!-- ${layer.file} -->\n\n${content}`;
    }).join('\n\n---\n\n');
  };

  const copyPromptContext = () => {
    const content = buildPromptContext();
    navigator.clipboard?.writeText(content);
    setCtxCopied(true);
    setTimeout(() => setCtxCopied(false), 1400);
  };

  // No JSZip available in artifact runtime, so the "bundle" is a single
  // markdown file with all layers concatenated. Practical, downloadable,
  // useful to drop into Claude Code / Cursor / Copilot as one paste.
  const downloadBundle = () => {
    const appName = (intent.appType || 'App').replace(/[^a-zA-Z0-9]/g, '');
    const content = `# ${intent.appType || 'App'} — Composition bundle

Generated by Composer Context Engine.

${buildPromptContext()}

---

## Scaffold command

\`\`\`bash
${command}
\`\`\`
`;
    const blob = new Blob([content], { type: 'text/markdown' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `${appName || 'composition'}-bundle.md`;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
    setDownloaded(true);
    setTimeout(() => setDownloaded(false), 1800);
    onShip?.();
  };

  return (
    <div>
      <SectionHeader filename="scaffold.command" badge="READY" />
      <div style={{ background: C.paper4, padding: 20, borderRadius: 6, position: 'relative', marginBottom: 16, border: `1px solid ${C.hairlineDk}` }}>
        <button onClick={() => { navigator.clipboard?.writeText(command); setCopied(true); setTimeout(() => setCopied(false), 1400); }}
          style={{
            position: 'absolute', top: 10, right: 10,
            fontFamily: SANS, fontSize: 11, color: '#fafafa',
            background: 'rgba(255,255,255,0.06)', border: '1px solid rgba(255,255,255,0.16)',
            padding: '4px 10px', borderRadius: 5, cursor: 'pointer', fontWeight: 500,
          }}>
          {copied ? '✓ Copied' : 'Copy'}
        </button>
        <pre style={{ fontFamily: MONO, fontSize: 12, lineHeight: 1.7, color: '#fafafa', margin: 0, whiteSpace: 'pre' }}>
          {command}
        </pre>
      </div>
      <div style={{ display: 'flex', gap: 8, alignItems: 'center', flexWrap: 'wrap' }}>
        <PrimaryButton onClick={downloadBundle}>
          {downloaded ? '✓ Bundle downloaded' : 'Download bundle ↓'}
        </PrimaryButton>
        <GhostButton onClick={copyPromptContext}>
          {ctxCopied ? '✓ Copied' : 'Copy prompt-context.md'}
        </GhostButton>
        <span style={{ marginLeft: 'auto', fontFamily: SANS, fontSize: 13, fontStyle: 'italic', color: C.ink3 }}>
          the composition is, for now, complete.
        </span>
      </div>
    </div>
  );
}

// ─── Composer footer ────────────────────────────────────────────────────

function ComposerFooter({
  layerState, leadQuestion, suggestions,
  onLockAndContinue, onGeneratePreview, onAcceptPreview, onDiscardPreview,
  promptValue, setPromptValue,
  isFirstLayer, aiConfigured, previewAck,
}) {
  const [focused, setFocused] = useState(false);
  const empty = promptValue.trim().length === 0;
  const isPreviewing = layerState === 'previewing';
  const isDirty = layerState === 'dirty';

  // Cmd/Ctrl+Enter to submit. Plain Enter inserts a newline (standard chat
  // convention). When dirty and a preview is available, the shortcut fires
  // generate-preview. When clean, it fires lock-and-continue.
  const handleKeyDown = (e) => {
    if (e.key === 'Enter' && (e.metaKey || e.ctrlKey)) {
      e.preventDefault();
      if (isDirty && aiConfigured) onGeneratePreview();
      else if (!isDirty && !isPreviewing) onLockAndContinue();
    }
  };

  // The primary action label softens to "Continue →" on the very first
  // layer so a brand-new user doesn't have to puzzle out what "lock" means.
  const continueLabel = isFirstLayer ? 'Continue →' : 'Lock and continue →';

  return (
    <div style={{ marginTop: 32, paddingTop: 24, borderTop: `1px solid ${C.hairline}` }}>
      <div style={{ marginBottom: 14 }}>
        <div style={{ display: 'flex', alignItems: 'baseline', gap: 8, marginBottom: 8 }}>
          <Eyebrow size={10} color={C.ink2} weight={600}>Composer</Eyebrow>
          <span style={{ color: C.ink5, fontFamily: SANS, fontSize: 11 }}>·</span>
          <Eyebrow size={10} color={C.ink3} weight={500}>
            {COMPOSER_STATUS[layerState] || COMPOSER_STATUS.clean}
          </Eyebrow>
        </div>
        <Body color={C.ink} size={14} style={{ display: 'block', maxWidth: 600 }}>
          {isPreviewing
            ? "Here's how I'd redraw this layer with your edits applied. Accept to lock the proposed version, or discard to revert."
            : leadQuestion}
        </Body>
        {/* Acknowledgment line — when previewing, echo back what the user
            asked for so the interaction feels reciprocal. Templated from
            the prompt that triggered the preview. */}
        {isPreviewing && previewAck && (
          <div style={{
            paddingLeft: 12, marginTop: 10,
            borderLeft: `1px solid ${C.amber}`,
          }}>
            <span style={{
              fontFamily: SANS, fontSize: 13, fontStyle: 'italic',
              color: C.ink2, lineHeight: 1.55, display: 'block',
            }}>
              {previewAck}
            </span>
          </div>
        )}
      </div>
      {!isPreviewing && (
        <>
          <div style={{
            border: `1px solid ${focused ? C.ink2 : C.hairline}`,
            borderRadius: 6, background: C.paper,
            display: 'flex', alignItems: 'flex-end', padding: 10, gap: 10,
            transition: 'border-color 140ms ease',
          }}>
            <textarea value={promptValue}
              onChange={(e) => setPromptValue(e.target.value)}
              onKeyDown={handleKeyDown}
              onFocus={() => setFocused(true)} onBlur={() => setFocused(false)}
              rows={1} placeholder="Refine, or accept what's drawn…"
              style={{
                flex: 1, border: 'none', outline: 'none', background: 'transparent',
                resize: 'none', fontFamily: SANS, fontSize: 14,
                lineHeight: 1.5, color: C.ink, minHeight: 22,
              }} />
          </div>
          {/* Suggestion chips — now ALWAYS visible (not just when empty),
              positioned below the textarea so they don't compete with the
              user's typing surface. */}
          {suggestions && suggestions.length > 0 && (
            <div style={{
              display: 'flex', gap: 6, marginTop: 10, flexWrap: 'wrap',
              alignItems: 'center',
            }}>
              <Eyebrow size={9} color={C.ink4} weight={500} style={{ marginRight: 4 }}>
                Try
              </Eyebrow>
              {suggestions.map((s) => (
                <button key={s} onClick={() => setPromptValue(s)} style={{
                  fontFamily: SANS, fontSize: 12, fontWeight: 500,
                  color: C.ink3, background: 'transparent',
                  border: `1px solid ${C.hairline}`, borderRadius: 5,
                  padding: '4px 10px', cursor: 'pointer',
                }}
                  onMouseEnter={(e) => { e.currentTarget.style.color = C.ink; e.currentTarget.style.borderColor = C.ink3; }}
                  onMouseLeave={(e) => { e.currentTarget.style.color = C.ink3; e.currentTarget.style.borderColor = C.hairline; }}>
                  {s}
                </button>
              ))}
              <span style={{ marginLeft: 'auto', fontFamily: MONO, fontSize: 10, color: C.ink4 }}>
                ⌘↵ to submit
              </span>
            </div>
          )}
        </>
      )}
      <div style={{ display: 'flex', gap: 8, alignItems: 'center', marginTop: 16, flexWrap: 'wrap' }}>
        {layerState === 'clean' && (
          <>
            <PrimaryButton onClick={onLockAndContinue}>{continueLabel}</PrimaryButton>
            <span style={{ fontFamily: SANS, fontSize: 12, fontStyle: 'italic', color: C.ink3 }}>
              accepting the recommendation
            </span>
          </>
        )}
        {layerState === 'dirty' && (
          <>
            <PrimaryButton tone="amber" onClick={onGeneratePreview} disabled={!aiConfigured}>
              Generate preview →
            </PrimaryButton>
            <GhostButton onClick={onDiscardPreview}>Discard edits</GhostButton>
            <span style={{ fontFamily: SANS, fontSize: 12, fontStyle: 'italic', color: C.ink3 }}>
              {aiConfigured
                ? "I'll redraw with your edits"
                : "Add an API key in settings to enable previews"}
            </span>
          </>
        )}
        {layerState === 'previewing' && (
          <>
            <PrimaryButton onClick={onAcceptPreview}>Accept and lock →</PrimaryButton>
            <GhostButton onClick={onDiscardPreview}>← Discard preview</GhostButton>
          </>
        )}
      </div>
    </div>
  );
}

// ─── RIGHT — Files Rail ─────────────────────────────────────────────────

function FileRow({ file, state }) {
  return (
    <div style={{ display: 'flex', alignItems: 'center', gap: 10, padding: '7px 0', opacity: state === 'planned' ? 0.5 : 1 }}>
      <span aria-hidden style={{
        width: 7, height: 7, borderRadius: '50%',
        background: state === 'drafted' ? C.amber : state === 'writing' ? C.indigo : 'transparent',
        border: state === 'planned' ? `1px solid ${C.ink4}` : 'none', flexShrink: 0,
        boxShadow: state === 'writing' ? `0 0 0 4px ${C.indigo}22` : 'none',
        animation: state === 'writing' ? 'pulse 1.6s ease-in-out infinite' : 'none',
      }} />
      <Mono color={state === 'planned' ? C.ink3 : C.ink} size={12} weight={state !== 'planned' ? 500 : 400} style={{ flex: 1 }}>
        {file}
      </Mono>
      <Eyebrow size={9} color={state === 'drafted' ? C.amber : state === 'writing' ? C.indigo : C.ink4} weight={600}>
        {state}
      </Eyebrow>
    </div>
  );
}

function FilesRail({ activeIndex, lockedIds, visible, intent, design, overrides, setOverride, onDirty }) {
  const [editing, setEditing] = useState(false);
  // When on the Scaffold layer with all files drafted, the user can flip
  // to a "View all" mode that concatenates every layer's markdown into
  // one scrollable document. Returns to per-layer view when toggled off.
  const [viewAll, setViewAll] = useState(false);

  const fileStates = LAYERS.map((layer, i) => {
    if (lockedIds.has(layer.id)) return { file: layer.file, state: 'drafted' };
    if (i === activeIndex)        return { file: layer.file, state: 'writing' };
    return { file: layer.file, state: 'planned' };
  });
  fileStates.push({ file: 'prompt-context.md', state: lockedIds.has('scaffold') ? 'drafted' : 'planned' });

  const activeLayer = LAYERS[activeIndex];
  const canvasState = { intent, design };
  // For per-layer mode: derive from generators (or use override if set).
  // For view-all mode: concat all 8 layer files with --- separators so
  // the user can copy the full bundle into a single AI prompt.
  const isScaffold = activeLayer.id === 'scaffold';
  const canViewAll = isScaffold && lockedIds.size >= LAYERS.length - 1;
  const inViewAll = canViewAll && viewAll;

  const buildViewAll = () => {
    return LAYERS.map((layer) => {
      const layerState = { intent, design };
      const gen = MARKDOWN_GEN[layer.id]?.(layerState) ?? '';
      const ov = overrides[layer.id];
      const content = ov !== undefined ? ov : gen;
      return `<!-- ${layer.file} -->\n\n${content}`;
    }).join('\n\n---\n\n');
  };

  const generated = inViewAll
    ? buildViewAll()
    : (MARKDOWN_GEN[activeLayer.id]?.(canvasState) ?? '');
  const override = overrides[activeLayer.id];
  const hasOverride = !inViewAll && override !== undefined;
  const displayed = inViewAll ? generated : (hasOverride ? override : generated);

  const previewLabel = inViewAll
    ? 'prompt-context.md'
    : activeLayer.file;
  const previewSource = inViewAll
    ? `ALL ${LAYERS.length} LAYERS · CONCATENATED`
    : (hasOverride ? 'YOUR EDITS' : 'GENERATED FROM CANVAS');

  const copyToClipboard = () => {
    navigator.clipboard?.writeText(displayed);
  };

  return (
    <aside style={{
      width: visible ? 340 : 0, flexShrink: 0,
      borderLeft: visible ? `1px solid ${C.hairline}` : 'none',
      paddingTop: 8, paddingLeft: visible ? 24 : 0, paddingRight: visible ? 16 : 0,
      position: 'sticky', top: 32, alignSelf: 'flex-start',
      maxHeight: 'calc(100vh - 32px)', overflowY: 'auto', overflowX: 'hidden',
      opacity: visible ? 1 : 0,
      transition: `width 480ms ${E_REVEAL}, padding 480ms ${E_REVEAL}, opacity 320ms ${E} ${visible ? '160ms' : '0ms'}, border-color 320ms ${E}`,
    }}>
      <div style={{ minWidth: 300 }}>
        {/* PRIMARY: live markdown preview/editor at the top of the rail.
            This is what the user cares about — what the system is writing
            on their behalf. The file list is reference material below. */}
        <div>
          <div style={{ display: 'flex', alignItems: 'baseline', justifyContent: 'space-between', marginBottom: 4, gap: 8 }}>
            <Eyebrow size={10} color={C.ink} weight={600}>
              {inViewAll ? 'Full bundle' : 'Live file'}
            </Eyebrow>
            {canViewAll && (
              <button onClick={() => setViewAll(!viewAll)} style={{
                fontFamily: SANS, fontSize: 10, fontWeight: 500,
                color: viewAll ? C.amber : C.ink3,
                background: 'transparent', border: 'none', padding: 0, cursor: 'pointer',
                textDecoration: 'underline', textUnderlineOffset: 2,
              }}>
                {viewAll ? '← Per-layer' : 'View all →'}
              </button>
            )}
          </div>
          <Body color={C.ink3} size={12} style={{ display: 'block', marginBottom: 12 }}>
            {inViewAll
              ? 'All locked layers, concatenated. Copy to ship the full brief.'
              : 'Updates as the canvas changes. Edit to override.'}
          </Body>

          <div style={{ display: 'flex', alignItems: 'baseline', justifyContent: 'space-between', marginBottom: 10, gap: 8 }}>
            <Mono color={C.ink} size={11} weight={500}
              style={{ overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap', flex: 1, minWidth: 0 }}>
              {previewLabel}
            </Mono>
            <div style={{ display: 'flex', flexShrink: 0, gap: 4, alignItems: 'center' }}>
              <button onClick={copyToClipboard} title="Copy markdown" style={{
                fontFamily: SANS, fontSize: 10, fontWeight: 500,
                color: C.ink3, background: 'transparent',
                border: `1px solid ${C.hairline}`, borderRadius: 4,
                padding: '3px 7px', cursor: 'pointer',
              }}
                onMouseEnter={(e) => { e.currentTarget.style.color = C.ink; e.currentTarget.style.borderColor = C.ink3; }}
                onMouseLeave={(e) => { e.currentTarget.style.color = C.ink3; e.currentTarget.style.borderColor = C.hairline; }}>
                Copy
              </button>
              {!inViewAll && (
                <div style={{ display: 'flex', marginLeft: 4 }}>
                  <button onClick={() => setEditing(false)} style={{
                    fontFamily: SANS, fontSize: 10, fontWeight: 500,
                    padding: '3px 8px',
                    borderRadius: '4px 0 0 4px',
                    background: !editing ? C.ink : 'transparent',
                    color: !editing ? C.paper : C.ink3,
                    border: `1px solid ${!editing ? C.ink : C.hairline}`,
                    cursor: 'pointer',
                    marginRight: -1,
                  }}>Preview</button>
                  <button onClick={() => setEditing(true)} style={{
                    fontFamily: SANS, fontSize: 10, fontWeight: 500,
                    padding: '3px 8px',
                    borderRadius: '0 4px 4px 0',
                    background: editing ? C.ink : 'transparent',
                    color: editing ? C.paper : C.ink3,
                    border: `1px solid ${editing ? C.ink : C.hairline}`,
                    cursor: 'pointer',
                  }}>Edit</button>
                </div>
              )}
            </div>
          </div>
          <div style={{ marginBottom: 8 }}>
            <Eyebrow size={9} color={C.ink4} weight={500}>{previewSource}</Eyebrow>
          </div>
          {hasOverride && (
            <div style={{ display: 'flex', alignItems: 'baseline', justifyContent: 'space-between', marginBottom: 8 }}>
              <Eyebrow size={9} color={C.amber} weight={600}>Override active</Eyebrow>
              <button onClick={() => setOverride(activeLayer.id, undefined)} style={{
                fontFamily: SANS, fontSize: 10, fontWeight: 500,
                color: C.ink3, background: 'transparent', border: 'none',
                padding: 0, cursor: 'pointer', textDecoration: 'underline',
                textUnderlineOffset: 2,
              }}>Reset</button>
            </div>
          )}
          {editing && !inViewAll ? (
            <textarea value={displayed}
              onChange={(e) => { setOverride(activeLayer.id, e.target.value); onDirty?.(); }}
              spellCheck={false}
              style={{
                display: 'block', width: '100%', minHeight: 260, maxHeight: 480,
                fontFamily: MONO, fontSize: 11, lineHeight: 1.6,
                color: C.ink2, background: C.paper2,
                border: `1px solid ${C.hairline}`, borderRadius: 4,
                padding: '10px 12px', resize: 'vertical',
                outline: 'none', boxSizing: 'border-box',
              }} />
          ) : (
            <MarkdownPreview source={displayed} />
          )}
        </div>

        {/* SECONDARY: file list and status panel, below the preview. */}
        <div style={{ marginTop: 32, paddingTop: 18, borderTop: `1px solid ${C.hairline}` }}>
          <Eyebrow size={10} color={C.ink} weight={600} style={{ display: 'block', marginBottom: 4 }}>Files</Eyebrow>
          <Body color={C.ink3} size={12} style={{ display: 'block', marginBottom: 12 }}>
            Each layer emits files as it locks.
          </Body>
          <div>
            {fileStates.map((f) => <FileRow key={f.file} file={f.file} state={f.state} />)}
          </div>
          <div style={{ marginTop: 14, paddingTop: 12, borderTop: `1px solid ${C.hairline}` }}>
            <Eyebrow size={10} color={C.ink3} weight={600} style={{ display: 'block', marginBottom: 4 }}>
              {lockedIds.size} of {LAYERS.length} locked
            </Eyebrow>
            <Body color={C.ink2} size={12} style={{ fontStyle: 'italic' }}>
              {lockedIds.size === LAYERS.length ? 'All layers locked. Bundle ready.' : `${LAYERS[activeIndex]?.label} will write ${LAYERS[activeIndex]?.file} when locked.`}
            </Body>
          </div>
        </div>
      </div>
    </aside>
  );
}

// ─── Main ─────────────────────────────────────────────────────────────────

export default function CompositionEngine() {
  const [activeIndex, setActiveIndex] = useState(0);
  const [lockedIds, setLockedIds] = useState(new Set());
  const [layerStates, setLayerStates] = useState(() => {
    const obj = {}; LAYERS.forEach((l) => { obj[l.id] = 'clean'; }); return obj;
  });
  const [prompts, setPrompts] = useState(() => {
    const obj = {}; LAYERS.forEach((l) => { obj[l.id] = ''; }); return obj;
  });
  const [intent, setIntent] = useState({ ...INTENT_EXAMPLE });
  const [design, setDesign] = useState(DESIGN_DEFAULTS);
  const [snapshots, setSnapshots] = useState({});
  const [previewValues, setPreviewValues] = useState({});
  const [overrideMarkdown, setOverrideMarkdown] = useState({});
  const [previewAcks, setPreviewAcks] = useState({});
  // One-shot "you can revisit any layer" toast — surfaced after the first
  // lock, dismissible, persists for the rest of the session.
  const [revisitHintShown, setRevisitHintShown] = useState(false);
  const [revisitHintDismissed, setRevisitHintDismissed] = useState(false);

  // In a real build this would check for a configured ILayerPreviewService.
  // The prototype runs identity-copy preview, so we flip this to true for
  // the demo so the button is active and we can show the conversational
  // acknowledgment line. Toggle to false to see the gated/disabled state.
  const aiConfigured = true;

  const setOverride = (layerId, value) => {
    setOverrideMarkdown((prev) => {
      const next = { ...prev };
      if (value === undefined) delete next[layerId];
      else next[layerId] = value;
      return next;
    });
  };

  const railsVisible = lockedIds.size > 0 || activeIndex > 0;
  const currentLayer = LAYERS[activeIndex];
  const currentLayerState = layerStates[currentLayer.id];

  const setLayerState = (id, s) => setLayerStates((p) => ({ ...p, [id]: s }));
  const setPrompt = (id, t) => setPrompts((p) => ({ ...p, [id]: t }));

  const markDirty = () => {
    if (currentLayerState === 'clean') {
      setSnapshots((p) => ({ ...p, [currentLayer.id]: { intent: { ...intent }, design: { ...design } } }));
      setLayerState(currentLayer.id, 'dirty');
    }
    if (currentLayerState === 'previewing') setLayerState(currentLayer.id, 'dirty');
  };
  const generatePreview = () => {
    const id = currentLayer.id;
    if (id === 'design') setPreviewValues((p) => ({ ...p, design: { ...design } }));
    if (id === 'intent') setPreviewValues((p) => ({ ...p, intent: { ...intent, workflow: intent.workflow.endsWith('.') ? intent.workflow : `${intent.workflow}.` } }));
    // Build a templated acknowledgment from the user's prompt for that layer.
    // Use the first sentence (or first 80 chars) so the echo reads as concise
    // recognition rather than a verbatim repeat.
    const userPrompt = prompts[id]?.trim();
    if (userPrompt) {
      const firstSentence = userPrompt.match(/^[^.!?]+[.!?]?/)?.[0] || userPrompt;
      const concise = firstSentence.length > 80
        ? firstSentence.slice(0, 80).trim() + '…'
        : firstSentence.trim();
      const ack = `You asked: "${concise}" — here's what changes if I apply that.`;
      setPreviewAcks((p) => ({ ...p, [id]: ack }));
    } else {
      setPreviewAcks((p) => ({ ...p, [id]: null }));
    }
    setLayerState(id, 'previewing');
  };
  const acceptPreview = () => {
    const id = currentLayer.id;
    if (id === 'design' && previewValues.design) setDesign(previewValues.design);
    if (id === 'intent' && previewValues.intent) setIntent(previewValues.intent);
    setLayerState(id, 'clean');
    setPrompt(id, '');
    setLockedIds((p) => new Set([...p, id]));
    if (lockedIds.size === 0) setRevisitHintShown(true);
    if (activeIndex < LAYERS.length - 1) setActiveIndex(activeIndex + 1);
  };
  const discardPreview = () => {
    const id = currentLayer.id;
    const snap = snapshots[id];
    if (snap) {
      if (id === 'design') setDesign(snap.design);
      if (id === 'intent') setIntent(snap.intent);
    }
    setLayerState(id, 'clean');
    setPrompt(id, '');
  };
  const lockAndContinue = () => {
    const id = currentLayer.id;
    setLockedIds((p) => new Set([...p, id]));
    setLayerState(id, 'clean');
    if (lockedIds.size === 0) setRevisitHintShown(true);
    if (activeIndex < LAYERS.length - 1) setActiveIndex(activeIndex + 1);
  };
  const jumpTo = (i) => {
    if (i === activeIndex) return;
    if (i < activeIndex || lockedIds.has(LAYERS[i].id)) setActiveIndex(i);
  };
  const reset = () => {
    setActiveIndex(0); setLockedIds(new Set());
    setIntent({ ...INTENT_EXAMPLE });
    setDesign(DESIGN_DEFAULTS);
    setLayerStates(() => { const o = {}; LAYERS.forEach((l) => { o[l.id] = 'clean'; }); return o; });
    setPrompts(() => { const o = {}; LAYERS.forEach((l) => { o[l.id] = ''; }); return o; });
    setSnapshots({}); setPreviewValues({}); setOverrideMarkdown({});
  };
  const onPromptChange = (text) => {
    setPrompt(currentLayer.id, text);
    if (text.trim().length > 0 && currentLayerState === 'clean') markDirty();
  };

  const summaries = useMemo(() => {
    const s = {};
    if (lockedIds.has('intent'))   s.intent = `"${intent.appType}, for ${intent.primaryUser}."`;
    if (lockedIds.has('ux'))       s.ux = '5 screens, dispatch flow';
    if (lockedIds.has('arch'))     s.arch = 'Pages → MVUX → Services → HTTP, Storage';
    if (lockedIds.has('design'))   s.design = `${design.bodyFont}, action ${design.action}`;
    if (lockedIds.has('interact')) s.interact = '6 states across 3 flows';
    if (lockedIds.has('data'))     s.data = '3 entities, 17 fields';
    if (lockedIds.has('impl'))     s.impl = '6 phases, build order locked';
    if (lockedIds.has('scaffold')) s.scaffold = 'dotnet new unoapp · ready';
    return s;
  }, [lockedIds, intent, design]);

  const lockedCards = useMemo(() => {
    // First pass: identify which layers are locked and active (i < activeIndex
    // && locked). We collapse all but the most recent 2 to keep the active
    // canvas in viewport as the stack grows.
    const lockedLayerIndices = LAYERS
      .map((layer, i) => ({ layer, i }))
      .filter(({ layer, i }) => i < activeIndex && lockedIds.has(layer.id));
    const lastTwoIndices = new Set(lockedLayerIndices.slice(-2).map(({ i }) => i));

    const cards = [];
    LAYERS.forEach((layer, i) => {
      if (i >= activeIndex || !lockedIds.has(layer.id)) return;
      let summary = '', contents = [];
      if (layer.id === 'intent') {
        summary = `"${intent.appType}, for ${intent.primaryUser}. ${intent.workflow}"`;
        contents = [['App type', intent.appType], ['Primary user', intent.primaryUser], ['Workflow', intent.workflow], ['Platforms', intent.platforms]];
      }
      if (layer.id === 'ux')       { summary = 'Five-screen dispatch flow with confirmation as terminal state.'; contents = [['Screens', '5'], ['Primary flow', 'Dispatch'], ['Empty states', '4'], ['Error states', '2']]; }
      if (layer.id === 'arch')     { summary = 'Pages bind State (MVUX); Services consume HTTP and Storage.'; contents = [['Modules', '6'], ['Connections', '5'], ['Pattern', 'MVUX'], ['Persistence', 'Offline-first']]; }
      if (layer.id === 'design')   { summary = `${design.bodyFont} body, action ${design.action}, low-chroma palette.`; contents = [['Body', design.bodyFont], ['Action', design.action], ['Type scale', '4 levels'], ['Spacing', '4px grid']]; }
      if (layer.id === 'interact') { summary = 'Six states across three flows; offline-first throughout.'; contents = [['Flows', '3'], ['States/flow', '6'], ['Offline', 'Yes'], ['Permission states', 'Yes']]; }
      if (layer.id === 'data')     { summary = 'Three entities — Job, Technician, Schedule — with explicit nullability.'; contents = [['Entities', '3'], ['Fields', '17'], ['Records', '3'], ['Collections', '3']]; }
      if (layer.id === 'impl')     { summary = 'Six phases, scaffold → polish, with explicit dependencies.'; contents = [['Phases', '6'], ['Acceptance', 'Per phase'], ['Verification', 'Per phase'], ['Order', 'Linear']]; }
      const collapsed = !lastTwoIndices.has(i);
      cards.push(<LockedContextCard key={layer.id} layer={layer} summary={summary} contents={contents} onEdit={() => setActiveIndex(i)} defaultCollapsed={collapsed} />);
    });
    return cards;
  }, [lockedIds, activeIndex, intent, design]);

  const futureCards = useMemo(() => {
    const cards = [];
    LAYERS.forEach((layer, i) => {
      if (i <= activeIndex) return;
      cards.push(<FuturePreviewCard key={layer.id} layer={layer} distance={i - activeIndex} />);
    });
    return cards;
  }, [activeIndex]);

  const renderCanvas = () => {
    const layer = currentLayer;
    const promptValue = prompts[layer.id];
    const layerState = currentLayerState;
    const ctx = deriveContext(intent);
    const footer = ({ leadQuestion, suggestions }) => (
      <ComposerFooter
        layerState={layerState} leadQuestion={leadQuestion} suggestions={suggestions}
        promptValue={promptValue} setPromptValue={onPromptChange}
        onLockAndContinue={lockAndContinue}
        onGeneratePreview={generatePreview}
        onAcceptPreview={acceptPreview}
        onDiscardPreview={discardPreview}
        isFirstLayer={lockedIds.size === 0 && activeIndex === 0}
        aiConfigured={aiConfigured}
        previewAck={previewAcks[layer.id]}
      />
    );
    if (layer.id === 'intent') return (<>
      <ActiveLayerHeader index={activeIndex} layer={layer} layerState={layerState}
        title="What are we building?" subtitle="Fill what you know. I'll infer the rest as we go." />
      <IntentCanvas intent={intent} setIntent={setIntent} onDirty={markDirty}
        layerState={layerState} previewIntent={previewValues.intent || intent} />
      {footer({ leadQuestion: 'If I summarize the intent right now, the agent has enough to scaffold a meaningful skeleton. Anything else worth adding before locking?', suggestions: ['Mobile-first', 'Offline-first', 'No backend yet'] })}
    </>);
    if (layer.id === 'ux') return (<>
      <ActiveLayerHeader index={activeIndex} layer={layer} layerState={layerState}
        title="How do users move through it?" subtitle={`Five screens for the primary ${ctx.entityNoun} flow. Architecture in §03 will pick the navigation primitive.`} />
      <UXCanvas ctx={ctx} />
      {footer({ leadQuestion: 'Drag-to-reorder schedule, or list-with-time-pickers? Stay on screen after dispatch, or return to dashboard?', suggestions: ['Drag-to-reorder', 'Return to dashboard', 'Modal confirmation'] })}
    </>);
    if (layer.id === 'arch') return (<>
      <ActiveLayerHeader index={activeIndex} layer={layer} layerState={layerState}
        title="How is this app shaped?" subtitle="A blueprint of how modules connect, and the solution tree they imply." />
      <ArchitectureCanvas ctx={ctx} />
      {footer({ leadQuestion: `Two open questions before locking — does ${ctx.entityNoun} logic live in a service, or stay inside State (MVUX)? And do ${ctx.userNoun} authenticate, or is access role-less?`, suggestions: ['Region-based navigation', `Single ${ctx.userNoun.replace(/s$/, '')} role`, 'Offline-first'] })}
    </>);
    if (layer.id === 'design') return (<>
      <ActiveLayerHeader index={activeIndex} layer={layer} layerState={layerState}
        title="How should it feel?" subtitle="Tokens, type scale on real sample copy, and the ColorPaletteOverride.xaml the agent will write." />
      <DesignCanvas design={design} setDesign={setDesign} onDirty={markDirty}
        layerState={layerState} previewDesign={previewValues.design || design} />
      {footer({ leadQuestion: 'The palette is low-chroma for outdoor visibility. Want a brand override on Action, or stay with amber?', suggestions: ['Stay with amber', 'Use a brand color', 'Show alternatives'] })}
    </>);
    if (layer.id === 'interact') return (<>
      <ActiveLayerHeader index={activeIndex} layer={layer} layerState={layerState}
        title="What about every state, every flow?" subtitle="For each flow, six states. Click around to see what each one means." />
      <InteractionsCanvas ctx={ctx} />
      {footer({ leadQuestion: 'Offline state — queue silently, or always show a sync-pending banner?', suggestions: ['Queue silently', 'Always show banner', 'Banner only when queue has items'] })}
    </>);
    if (layer.id === 'data') return (<>
      <ActiveLayerHeader index={activeIndex} layer={layer} layerState={layerState}
        title="What shapes does the data take?" subtitle="Three entities with explicit fields and nullability." />
      <DataCanvas ctx={ctx} />
      {footer({ leadQuestion: `Audit trail on ${ctx.entityPlural}, or latest status only? GeoPoint as record struct, or two doubles?`, suggestions: ['Latest status only', 'Audit trail', 'GeoPoint as record'] })}
    </>);
    if (layer.id === 'impl') return (<>
      <ActiveLayerHeader index={activeIndex} layer={layer} layerState={layerState}
        title="How does it get built?" subtitle="Six phases as a tabular plan — files, dependencies, agent prompts." />
      <ImplementationCanvas />
      {footer({ leadQuestion: 'Strictly linear, or can Domain and Screens parallelize?', suggestions: ['Strictly linear', 'Parallelize', 'Add a tooling phase'] })}
    </>);
    if (layer.id === 'scaffold') return (<>
      <ActiveLayerHeader index={activeIndex} layer={layer} layerState={layerState}
        title="The bundle is ready." subtitle="Every layer locked. Copy the scaffold, or download the full bundle." />
      <ScaffoldCanvas intent={intent} design={design} overrides={overrideMarkdown} onShip={lockAndContinue} />
    </>);
    return null;
  };

  const centerMaxWidth = railsVisible ? 880 : 680;

  return (
    <div style={{ background: C.paper, minHeight: '100vh', fontFamily: SANS, color: C.ink2 }}>
      <div style={{ display: 'flex', alignItems: 'flex-start', minHeight: '100vh', justifyContent: railsVisible ? 'flex-start' : 'center' }}>
        <CompositionStack activeIndex={activeIndex} lockedIds={lockedIds} summaries={summaries} onJump={jumpTo} visible={railsVisible} />
        <main style={{
          flex: railsVisible ? 1 : 'none', minWidth: 0,
          padding: railsVisible ? '32px 48px 80px' : '64px 48px 80px',
          maxWidth: centerMaxWidth, width: railsVisible ? 'auto' : '100%',
          transition: `padding 480ms ${E_REVEAL}, max-width 480ms ${E_REVEAL}`,
        }}>
          <ProgressIndicator activeIndex={activeIndex} total={LAYERS.length} />
          <div style={{ display: 'flex', alignItems: 'baseline', gap: 12, marginBottom: 22, flexWrap: 'wrap' }}>
            <Body color={C.ink} size={18} weight={500}>{intent.appType || 'Untitled'}</Body>
            {lockedIds.size > 0 && (
              <button onClick={reset} style={{
                marginLeft: 'auto', fontFamily: SANS, fontSize: 12, fontWeight: 500,
                color: C.ink3, background: 'transparent', border: 'none',
                cursor: 'pointer', padding: 0,
              }}>
                Reset
              </button>
            )}
          </div>
          {lockedCards}
          <div style={{ marginTop: lockedCards.length > 0 ? 28 : 0 }}>
            {renderCanvas()}
          </div>
          {railsVisible && futureCards.length > 0 && (
            <div style={{ marginTop: 40 }}>{futureCards}</div>
          )}
        </main>
        <FilesRail
          activeIndex={activeIndex}
          lockedIds={lockedIds}
          visible={railsVisible}
          intent={intent}
          design={design}
          overrides={overrideMarkdown}
          setOverride={setOverride}
          onDirty={markDirty}
        />
      </div>
      <style>{`
        @import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600&family=JetBrains+Mono:wght@400;500;600&display=swap');
        * { box-sizing: border-box; }
        body { margin: 0; }
        ::selection { background: ${C.ink}; color: ${C.paper}; }
        textarea::placeholder, input::placeholder { color: ${C.ink4}; }
        @keyframes pulse { 0%, 100% { opacity: 1; } 50% { opacity: 0.5; } }
      `}</style>
    </div>
  );
}
