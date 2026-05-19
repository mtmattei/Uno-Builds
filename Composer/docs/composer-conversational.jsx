import React, { useState, useEffect, useRef } from 'react';

// ─── Editorial monochromatic palette (inherited from composer) ─────────────
const C = {
  paper:    '#ffffff',
  paper2:   '#fafafa',
  paper3:   '#f4f4f5',
  ink:      '#18181b',
  ink2:     '#3f3f46',
  ink3:     '#71717a',
  ink4:     '#a1a1aa',
  hairline: '#e4e4e7',
};

const SERIF = "'Fraunces', 'Iowan Old Style', Georgia, serif";
const MONO  = "'Martian Mono', 'JetBrains Mono', ui-monospace, monospace";

// ─── Artifact registry ─────────────────────────────────────────────────────
const ARTIFACTS = [
  { id: 'readme',         file: 'README.md',              layer: 'foundation' },
  { id: 'claude',         file: 'CLAUDE.md',              layer: 'foundation' },
  { id: 'mcp',            file: '.mcp.json',              layer: 'wiring' },
  { id: 'design',         file: 'DESIGN.md',              layer: 'design' },
  { id: 'interactions',   file: 'INTERACTIONS.md',        layer: 'design' },
  { id: 'architecture',   file: 'ARCHITECTURE.md',        layer: 'architecture' },
  { id: 'plan',           file: 'implementation-plan.md', layer: 'plan' },
  { id: 'scaffold',       file: 'scaffold.sh',            layer: 'plan' },
];

const PLANNED = 'planned';
const DRAFTED = 'drafted';

// Which artifacts each chat layer is about to populate. Drives expandedIds
// so the artifact panel's expanded card matches the chat's current focus.
const LAYER_TO_ARTIFACTS = {
  description:  ['readme', 'claude'],   // description re-drafts these via foundation effect
  wiring:       ['mcp'],
  design:       ['design'],
  interactions: ['interactions'],
  architecture: ['architecture'],
  plan:         ['plan'],
  done:         [],
};

// ─── Artifact content templates ────────────────────────────────────────────
// Pure functions: app context → artifact content. Used by client-side
// handlers to avoid the API round-trip for predictable layers.
const PLATFORM_FLAGS = {
  Web:     'wasm',
  Windows: 'windows',
  Android: 'android',
  iOS:     'ios',
  Desktop: 'desktop',
};

const TEMPLATES = {
  readme: ({ appName, description, platforms }) => {
    const desc = description?.trim()
      || `A cross-platform application built with Uno Platform.`;
    return `# ${appName}

${desc}

Built with Uno Platform on .NET 9.

## Targets

${platforms.length ? platforms.map((p) => `- ${p}`).join('\n') : '_(platforms not yet selected)_'}`;
  },

  claude: ({ appName, description, runtime }) => {
    const overview = description?.trim() || `<!-- Brief description of ${appName} -->`;
    const tfm = runtime === '.NET 9' ? 'net9.0-desktop' : 'net10.0-desktop';

    return `# Project Instructions

## Overview

${overview}

## Project Structure

- \`src/\` — Application source
- \`src/Models/\` — State records and domain types
- \`src/Presentation/\` — Pages, ViewModels, UserControls
- \`src/Services/\` — Service interfaces and implementations
- \`src/Strings/en/\` — Localization resources

## Conventions

- New pages get a corresponding partial record model in \`Models/\`.
- Use \`INavigator\` for navigation, never frame-based.
- Prefer Uno Toolkit controls (\`NavigationBar\`, \`TabBar\`) over raw WinUI equivalents.
- Keep XAML lean — use Lightweight Styling and theme resources over inline values.
- Search Uno Platform docs via MCP before assuming API usage or patterns.

## Key References

Before starting any new feature or architectural decision, read these first:

- \`docs/ARCHITECTURE.md\` — system architecture, stack choices, layers, dependencies
- \`docs/DESIGN.md\` — design language, spacing, color tokens, component patterns
- \`docs/INTERACTIONS.md\` — state model, user flows, component states, animation inventory

## Pre-Review Cleanup

Before submitting code for review, scan for and remove dead code:

- Remove commented-out code blocks that are no longer needed.
- Remove unreferenced methods/functions that are safe to delete.
- Remove obviously unreachable or orphaned code from prior refactors.
- Leave functional code, active comments, TODOs, and intentional extension points untouched.
- If usage is uncertain, do not delete — mark with \`// REVIEW: possibly unused\` instead.

## Verification

\`\`\`bash
dotnet build
dotnet test
dotnet run -f ${tfm}
\`\`\`

Always run \`dotnet build\` after changes to confirm the project still compiles. Run tests when they exist.`;
  },

  mcp: ({ servers }) => {
    const config = {
      mcpServers: Object.fromEntries(
        servers.map((s) => {
          const id = s.toLowerCase().replace(/\s+/g, '-');
          return [id, { url: `https://mcp.${id}.com/v1` }];
        })
      ),
    };
    return JSON.stringify(config, null, 2);
  },

  design: ({ source, figmaUrl, prototypeUrl, screenshots }) => {
    const base = source === 'Fluent defaults' ? 'Fluent' : 'Material';
    const lines = ['# Design tokens', ''];
    lines.push(`Base   · ${base}`);
    if (figmaUrl)     lines.push(`Figma  · ${figmaUrl}`);
    if (prototypeUrl) lines.push(`Prototype · ${prototypeUrl}`);
    if (screenshots)  lines.push(`Refs   · ${screenshots}`);
    if (!figmaUrl && !prototypeUrl && !screenshots) {
      lines.push(`Source · ${source || 'Material defaults'}`);
    }
    lines.push('');
    lines.push('Primary    #3D5AFE → #2A3FA3');
    lines.push('Surface    #FFFFFF / #F7F7F8');
    lines.push('Ink        #1C1B1F');
    if (figmaUrl || screenshots || prototypeUrl) {
      lines.push('');
      lines.push('_Tokens above are placeholder defaults. Refine to match the referenced assets._');
    }
    return lines.join('\n');
  },

  interactions: ({ motion, gestures }) => {
    const m = motion || 'smooth';
    const easings = m === 'snappy'
      ? 'cubic-bezier(0.4, 0, 0.2, 1) for entrance'
      : m === 'minimal'
        ? 'system defaults (no custom easings)'
        : 'cubic-bezier(0.16, 1, 0.3, 1) for entrance';
    const durations = m === 'snappy'
      ? '100 / 160 / 220 ms tokens'
      : m === 'minimal'
        ? '_(no custom durations — system defaults)_'
        : '180 / 280 / 420 ms tokens';
    const pageTransition = m === 'snappy'
      ? 'forward slide 200ms'
      : m === 'minimal'
        ? 'instant (no transition)'
        : 'forward slide 320ms';
    const lines = [
      '# Motion',
      '',
      `Profile · ${m}`,
      `Easings · ${easings}`,
      `Durations · ${durations}`,
      `Page transitions · ${pageTransition}`,
    ];
    if (gestures) {
      lines.push(`Gestures · ${gestures}`);
    }
    return lines.join('\n');
  },

  architecture: ({ pattern, markup, renderer, http, nav, theme, runtime, platforms }) => `# Architecture

Runtime   · ${runtime || '.NET 10'}
Targets   · ${(platforms || []).join(', ') || 'Web, Android, iOS'}
Pattern   · ${pattern}
Markup    · ${markup}
Renderer  · ${renderer}
HTTP      · ${http}
Theme     · ${theme} + DSP overrides
Nav       · ${nav}
DI        · Microsoft.Extensions.DependencyInjection via Uno.Extensions.Hosting
Logging   · Serilog
Auth      · OIDC
Tests     · xUnit + Snapshot`,

  plan: ({ appName }) => `# Implementation plan

Vertical slices, end-to-end:
1. App shell + navigation
2. Primary list view
3. Detail view
4. Authoring (create / edit)
5. Save & search
6. Settings

_Edit any slice inline to refine the plan for ${appName}._`,

  scaffold: ({ appName, platforms, runtime }) => {
    const flags = platforms.map((p) => PLATFORM_FLAGS[p]).filter(Boolean).join(' ');
    const tfm = runtime === '.NET 9' ? 'net9.0' : 'net10.0';
    return `#!/usr/bin/env bash
set -euo pipefail

dotnet new unoapp -n ${appName || 'MyApp'} \\
  -platforms "${flags || 'wasm windows android ios'}" \\
  -presets blank -tfm ${tfm}`;
  },
};

// ─── Live composer is a hybrid: client-side handlers for predictable
// layer transitions, falling back to a Sonnet 4 API call for free-text
// answers (descriptions, custom Figma URLs, plan refinements). ─────────────

// ─── Quick-start prompts ───────────────────────────────────────────────────
const PROMPT_SUGGESTIONS = [
  'A field-service tool for technicians',
  'A reading list across desktop and mobile',
  'A pricing calculator for sales teams',
];

// ─── Initial state helpers ─────────────────────────────────────────────────
const initialArtifactState = () =>
  Object.fromEntries(ARTIFACTS.map((a) => [a.id, { status: PLANNED, content: '' }]));

// ─── Components ────────────────────────────────────────────────────────────

function MonoLabel({ children, color = C.ink3, ...rest }) {
  return (
    <span
      style={{
        fontFamily: MONO,
        fontSize: 10,
        letterSpacing: '0.18em',
        textTransform: 'uppercase',
        fontWeight: 500,
        color,
      }}
      {...rest}
    >
      {children}
    </span>
  );
}

// One inline build-state callout inside a composer message.
function Callout({ layer, status, notes }) {
  return (
    <div
      style={{
        margin: '10px 0',
        paddingLeft: 14,
        borderLeft: `2px solid ${C.ink}`,
        display: 'flex',
        flexDirection: 'column',
        gap: 2,
      }}
    >
      <div style={{ display: 'flex', alignItems: 'baseline', gap: 8 }}>
        <span
          style={{
            fontFamily: MONO,
            fontSize: 10,
            letterSpacing: '0.2em',
            textTransform: 'uppercase',
            color: C.ink,
            fontWeight: 600,
          }}
        >
          {layer}
        </span>
        <span
          aria-hidden
          style={{
            display: 'inline-block',
            width: 6,
            height: 6,
            borderRadius: 999,
            background: C.ink,
          }}
        />
        <span
          style={{
            fontFamily: MONO,
            fontSize: 9,
            letterSpacing: '0.18em',
            textTransform: 'uppercase',
            color: C.ink2,
          }}
        >
          {status}
        </span>
      </div>
      <span
        style={{
          fontFamily: MONO,
          fontSize: 10,
          letterSpacing: '0.04em',
          color: C.ink3,
        }}
      >
        {notes}
      </span>
    </div>
  );
}

function QuickReplies({
  data,
  selected,
  interactive,
  onToggle,
  onSubmit,
}) {
  if (!data || !Array.isArray(data) || data.length === 0) return null;

  // Submission-ready check: at least one option selected across all groups
  const hasAnySelection = Object.values(selected).some(
    (arr) => Array.isArray(arr) && arr.length > 0
  );

  const chipBase = {
    fontFamily: MONO,
    fontSize: 10,
    letterSpacing: '0.14em',
    textTransform: 'uppercase',
    fontWeight: 500,
    padding: '6px 12px',
    borderRadius: 999,
    cursor: interactive ? 'pointer' : 'default',
    transition: 'all 180ms ease-out',
    whiteSpace: 'nowrap',
  };

  return (
    <div
      style={{
        marginTop: 14,
        display: 'flex',
        flexDirection: 'column',
        gap: 10,
      }}
    >
      {data.map((group, groupIdx) => {
        const groupSelected = selected[groupIdx] || [];
        return (
          <div
            key={groupIdx}
            style={{
              display: 'flex',
              alignItems: 'center',
              flexWrap: 'wrap',
              gap: 8,
            }}
          >
            {group.label && (
              <span
                style={{
                  fontFamily: MONO,
                  fontSize: 10,
                  letterSpacing: '0.18em',
                  textTransform: 'uppercase',
                  color: C.ink3,
                  minWidth: 96,
                  paddingRight: 4,
                }}
              >
                {group.label}
              </span>
            )}
            <div style={{ display: 'flex', flexWrap: 'wrap', gap: 6, flex: 1 }}>
              {group.options.map((opt) => {
                const isSelected = groupSelected.includes(opt);
                const styles = isSelected
                  ? {
                      background: C.ink,
                      color: C.paper,
                      border: `1px solid ${C.ink}`,
                    }
                  : {
                      background: 'transparent',
                      color: interactive ? C.ink2 : C.ink4,
                      border: `1px solid ${C.hairline}`,
                    };
                return (
                  <button
                    key={opt}
                    onClick={() => interactive && onToggle(groupIdx, opt, group.multi)}
                    disabled={!interactive}
                    style={{ ...chipBase, ...styles }}
                    onMouseEnter={(e) => {
                      if (!interactive || isSelected) return;
                      e.currentTarget.style.borderColor = C.ink;
                      e.currentTarget.style.color = C.ink;
                    }}
                    onMouseLeave={(e) => {
                      if (!interactive || isSelected) return;
                      e.currentTarget.style.borderColor = C.hairline;
                      e.currentTarget.style.color = C.ink2;
                    }}
                  >
                    {opt}
                  </button>
                );
              })}
            </div>
          </div>
        );
      })}

      {interactive && hasAnySelection && (
        <div style={{ display: 'flex', justifyContent: 'flex-end', marginTop: 4 }}>
          <button
            onClick={onSubmit}
            style={{
              fontFamily: MONO,
              fontSize: 10,
              letterSpacing: '0.18em',
              textTransform: 'uppercase',
              fontWeight: 500,
              padding: '7px 14px',
              background: C.ink,
              color: C.paper,
              border: `1px solid ${C.ink}`,
              borderRadius: 999,
              cursor: 'pointer',
              display: 'inline-flex',
              alignItems: 'center',
              gap: 6,
              whiteSpace: 'nowrap',
            }}
          >
            Send
            <span style={{ display: 'inline-block', transform: 'translateY(-1px)' }}>→</span>
          </button>
        </div>
      )}
    </div>
  );
}

function SuggestionPanel({
  reasoning,
  suggestion,
  alternatives,
  freeTextHint,
  alternativesOpen,
  onApply,
  onPickAlternative,
  onToggleAlternatives,
  interactive,
  // Design layer extras
  assetInputs,
  assetValues,
  onAssetChange,
  onApplyAssets,
}) {
  const hasAnyAsset = assetInputs && (
    assetValues?.figmaUrl?.trim() ||
    assetValues?.prototypeUrl?.trim() ||
    assetValues?.screenshots?.trim()
  );

  return (
    <div style={{ marginTop: 16 }}>
      {/* Decision card — tinted, bordered, slightly elevated visually */}
      <div
        style={{
          background: C.paper2,
          border: `1px solid ${C.hairline}`,
          borderRadius: 6,
          padding: '20px 22px 18px',
          position: 'relative',
        }}
      >
        {/* Eyebrow */}
        {suggestion && (
          <div
            style={{
              display: 'flex',
              alignItems: 'baseline',
              gap: 8,
              marginBottom: 8,
              fontFamily: MONO,
              fontSize: 9,
              letterSpacing: '0.2em',
              textTransform: 'uppercase',
              color: C.ink3,
            }}
          >
            <span>Suggested</span>
            <span style={{ flex: 1, height: 1, background: C.hairline, alignSelf: 'center' }} />
          </div>
        )}

        {/* Suggestion label */}
        {suggestion && (
          <div
            style={{
              fontFamily: SERIF,
              fontSize: 18,
              lineHeight: 1.35,
              color: C.ink,
              marginBottom: 12,
              fontWeight: 400,
            }}
          >
            {suggestion.label}
          </div>
        )}

        {/* Reasoning */}
        {reasoning && (
          <p
            style={{
              fontFamily: SERIF,
              fontStyle: 'italic',
              fontSize: 14,
              lineHeight: 1.55,
              color: C.ink2,
              margin: '0 0 16px',
              paddingLeft: 12,
              borderLeft: `2px solid ${C.ink4}`,
            }}
          >
            {reasoning}
          </p>
        )}

        {/* Action buttons */}
        {suggestion && (
          <div style={{ display: 'flex', flexWrap: 'wrap', gap: 8, marginBottom: 0 }}>
            <button
              type="button"
              onClick={onApply}
              disabled={!interactive}
              style={{
                fontFamily: MONO,
                fontSize: 10,
                letterSpacing: '0.16em',
                textTransform: 'uppercase',
                fontWeight: 500,
                padding: '9px 16px',
                background: C.ink,
                color: C.paper,
                border: `1px solid ${C.ink}`,
                borderRadius: 999,
                cursor: interactive ? 'pointer' : 'not-allowed',
                opacity: interactive ? 1 : 0.4,
                transition: 'transform 140ms ease-out',
              }}
            >
              {suggestion.actionLabel || 'Apply'}
            </button>

            {((alternatives && alternatives.length > 0) || assetInputs) && (
              <button
                type="button"
                onClick={onToggleAlternatives}
                disabled={!interactive}
                style={{
                  fontFamily: MONO,
                  fontSize: 10,
                  letterSpacing: '0.16em',
                  textTransform: 'uppercase',
                  fontWeight: 500,
                  padding: '9px 14px',
                  background: 'transparent',
                  color: C.ink2,
                  border: `1px solid ${C.ink3}`,
                  borderRadius: 999,
                  cursor: interactive ? 'pointer' : 'not-allowed',
                  opacity: interactive ? 1 : 0.4,
                  transition: 'all 180ms ease-out',
                  display: 'inline-flex',
                  alignItems: 'center',
                  gap: 6,
                }}
              >
                {alternativesOpen
                  ? 'Hide options'
                  : assetInputs
                    ? 'Show alternatives & assets'
                    : 'Show alternatives'}
                <svg width="9" height="9" viewBox="0 0 9 9" style={{
                  transform: alternativesOpen ? 'rotate(180deg)' : 'rotate(0deg)',
                  transition: 'transform 220ms ease-out',
                }}>
                  <path d="M 2 3.5 L 4.5 6 L 7 3.5" fill="none" stroke="currentColor" strokeWidth="1.2" strokeLinecap="round" strokeLinejoin="round" />
                </svg>
              </button>
            )}
          </div>
        )}

        {/* Alternatives & assets panel — collapsible, lives inside the card */}
        {((alternatives && alternatives.length > 0) || assetInputs) && (
          <div
            style={{
              display: 'grid',
              gridTemplateRows: alternativesOpen ? '1fr' : '0fr',
              transition: 'grid-template-rows 280ms ease-out',
            }}
          >
            <div style={{ overflow: 'hidden' }}>
              <div
                style={{
                  marginTop: 16,
                  paddingTop: 16,
                  borderTop: `1px solid ${C.hairline}`,
                }}
              >
                {/* Alternative buttons */}
                {alternatives && alternatives.length > 0 && (
                  <>
                    <div
                      style={{
                        fontFamily: MONO,
                        fontSize: 9,
                        letterSpacing: '0.2em',
                        textTransform: 'uppercase',
                        color: C.ink3,
                        marginBottom: 8,
                      }}
                    >
                      Alternatives
                    </div>
                    <div style={{ display: 'flex', flexDirection: 'column', gap: 6, marginBottom: assetInputs ? 18 : 0 }}>
                      {alternatives.map((alt, i) => (
                        <button
                          key={i}
                          type="button"
                          onClick={() => onPickAlternative(alt)}
                          disabled={!interactive}
                          style={{
                            fontFamily: SERIF,
                            fontSize: 14,
                            color: C.ink,
                            textAlign: 'left',
                            padding: '10px 14px',
                            background: C.paper,
                            border: `1px solid ${C.hairline}`,
                            borderRadius: 4,
                            cursor: interactive ? 'pointer' : 'not-allowed',
                            transition: 'all 180ms ease-out',
                          }}
                          onMouseEnter={(e) => {
                            if (interactive) {
                              e.currentTarget.style.borderColor = C.ink2;
                              e.currentTarget.style.background = C.paper3;
                            }
                          }}
                          onMouseLeave={(e) => {
                            e.currentTarget.style.borderColor = C.hairline;
                            e.currentTarget.style.background = C.paper;
                          }}
                        >
                          {alt.label}
                        </button>
                      ))}
                    </div>
                  </>
                )}

                {/* Design assets panel — only for design layer */}
                {assetInputs && (
                  <>
                    <div
                      style={{
                        fontFamily: MONO,
                        fontSize: 9,
                        letterSpacing: '0.2em',
                        textTransform: 'uppercase',
                        color: C.ink3,
                        marginBottom: 8,
                      }}
                    >
                      Or provide your own assets
                    </div>
                    <div style={{ display: 'flex', flexDirection: 'column', gap: 8 }}>
                      <AssetField
                        label="Figma URL"
                        placeholder="https://www.figma.com/file/..."
                        value={assetValues?.figmaUrl || ''}
                        onChange={(v) => onAssetChange('figmaUrl', v)}
                      />
                      <AssetField
                        label="Prototype URL"
                        placeholder="https://claude.ai/artifacts/..."
                        value={assetValues?.prototypeUrl || ''}
                        onChange={(v) => onAssetChange('prototypeUrl', v)}
                      />
                      <AssetField
                        label="Screenshots"
                        placeholder="Image URLs or short description of the visual references"
                        value={assetValues?.screenshots || ''}
                        onChange={(v) => onAssetChange('screenshots', v)}
                        multiline
                      />
                      <button
                        type="button"
                        onClick={onApplyAssets}
                        disabled={!interactive || !hasAnyAsset}
                        style={{
                          alignSelf: 'flex-start',
                          marginTop: 4,
                          fontFamily: MONO,
                          fontSize: 10,
                          letterSpacing: '0.16em',
                          textTransform: 'uppercase',
                          fontWeight: 500,
                          padding: '8px 14px',
                          background: hasAnyAsset ? C.ink : C.paper3,
                          color: hasAnyAsset ? C.paper : C.ink4,
                          border: `1px solid ${hasAnyAsset ? C.ink : C.hairline}`,
                          borderRadius: 999,
                          cursor: (interactive && hasAnyAsset) ? 'pointer' : 'not-allowed',
                          opacity: interactive ? 1 : 0.4,
                          transition: 'all 220ms ease-out',
                        }}
                      >
                        Apply with these assets
                      </button>
                    </div>
                  </>
                )}
              </div>
            </div>
          </div>
        )}
      </div>

      {/* Free-text hint — outside the card, lighter weight */}
      {freeTextHint && (
        <p
          style={{
            fontFamily: SERIF,
            fontStyle: 'italic',
            fontSize: 13,
            color: C.ink4,
            margin: '12px 0 0',
            paddingLeft: 2,
          }}
        >
          {freeTextHint}
        </p>
      )}
    </div>
  );
}

// Small asset input field used inside the design layer's alternatives panel
function AssetField({ label, placeholder, value, onChange, multiline }) {
  const Tag = multiline ? 'textarea' : 'input';
  return (
    <label style={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
      <span
        style={{
          fontFamily: MONO,
          fontSize: 8.5,
          letterSpacing: '0.18em',
          textTransform: 'uppercase',
          color: C.ink3,
        }}
      >
        {label}
      </span>
      <Tag
        type={multiline ? undefined : 'text'}
        rows={multiline ? 2 : undefined}
        value={value}
        placeholder={placeholder}
        onChange={(e) => onChange(e.target.value)}
        spellCheck={false}
        style={{
          fontFamily: multiline ? SERIF : MONO,
          fontSize: multiline ? 13 : 12,
          color: C.ink,
          background: C.paper,
          border: `1px solid ${C.hairline}`,
          borderRadius: 3,
          padding: '7px 10px',
          outline: 'none',
          resize: multiline ? 'vertical' : 'none',
          boxSizing: 'border-box',
          width: '100%',
          transition: 'border-color 180ms ease-out',
        }}
        onFocus={(e) => (e.currentTarget.style.borderColor = C.ink)}
        onBlur={(e) => (e.currentTarget.style.borderColor = C.hairline)}
      />
    </label>
  );
}

function MessageBlock({
  role,
  body,
  callouts,
  post,
  reasoning,
  suggestion,
  alternatives,
  freeTextHint,
  timestamp,
  justAdded,
  // Suggestion panel props
  alternativesOpen,
  onApply,
  onPickAlternative,
  onToggleAlternatives,
  interactive,
  // Design layer extras
  assetInputs,
  assetValues,
  onAssetChange,
  onApplyAssets,
}) {
  const isUser = role === 'USER';
  return (
    <div
      style={{
        opacity: justAdded ? 0 : 1,
        transform: justAdded ? 'translateY(8px)' : 'translateY(0)',
        animation: justAdded ? 'msgIn 480ms cubic-bezier(0.16, 1, 0.3, 1) forwards' : 'none',
        paddingTop: 24,
        paddingBottom: 24,
        borderTop: `1px solid ${C.hairline}`,
      }}
    >
      <div
        style={{
          display: 'flex',
          alignItems: 'baseline',
          gap: 10,
          marginBottom: 8,
        }}
      >
        <MonoLabel color={C.ink}>{role}</MonoLabel>
        <span
          style={{
            fontFamily: MONO,
            fontSize: 9,
            letterSpacing: '0.16em',
            color: C.ink4,
          }}
        >
          {timestamp}
        </span>
      </div>

      <p
        style={{
          fontFamily: SERIF,
          fontSize: 16,
          lineHeight: 1.55,
          color: isUser ? C.ink2 : C.ink,
          fontStyle: isUser ? 'italic' : 'normal',
          margin: 0,
          whiteSpace: 'pre-wrap',
        }}
      >
        {body}
      </p>

      {callouts && callouts.length > 0 && (
        <div style={{ marginTop: 12 }}>
          {callouts.map((c, i) => (
            <Callout key={i} {...c} />
          ))}
        </div>
      )}

      {post && (
        <p
          style={{
            fontFamily: SERIF,
            fontSize: 16,
            lineHeight: 1.55,
            color: C.ink,
            margin: 0,
            marginTop: callouts && callouts.length > 0 ? 12 : 0,
          }}
        >
          {post}
        </p>
      )}

      {(suggestion || reasoning) && (
        <SuggestionPanel
          reasoning={reasoning}
          suggestion={suggestion}
          alternatives={alternatives}
          freeTextHint={freeTextHint}
          alternativesOpen={alternativesOpen}
          onApply={onApply}
          onPickAlternative={onPickAlternative}
          onToggleAlternatives={onToggleAlternatives}
          interactive={interactive}
          assetInputs={assetInputs}
          assetValues={assetValues}
          onAssetChange={onAssetChange}
          onApplyAssets={onApplyAssets}
        />
      )}
    </div>
  );
}

// Compact representation of a "locked-in" prior composer turn. Shows layer
// name, the answer that was applied, and an Edit button that truncates the
// conversation back to that point.
function CompactComposerTurn({ layer, appliedLabel, onEdit }) {
  const layerLabel = (() => {
    switch (layer) {
      case 'description':  return 'Description';
      case 'wiring':       return 'Wiring';
      case 'design':       return 'Design system';
      case 'interactions': return 'Interactions';
      case 'architecture': return 'Architecture';
      case 'plan':         return 'Plan';
      default:             return layer;
    }
  })();

  const display = appliedLabel
    ? (appliedLabel.length > 80 ? appliedLabel.slice(0, 80).trim() + '…' : appliedLabel)
    : 'Locked in';

  return (
    <div
      style={{
        paddingTop: 14,
        paddingBottom: 14,
        borderTop: `1px solid ${C.hairline}`,
        display: 'flex',
        alignItems: 'center',
        gap: 12,
      }}
    >
      <svg width="14" height="14" viewBox="0 0 14 14" style={{ flexShrink: 0 }} aria-hidden>
        <circle cx="7" cy="7" r="6" fill={C.ink} stroke={C.ink} strokeWidth="1" />
        <path
          d="M 4 7.4 L 6 9.2 L 10 5.2"
          fill="none"
          stroke={C.paper}
          strokeWidth="1.5"
          strokeLinecap="round"
          strokeLinejoin="round"
        />
      </svg>

      <div style={{ flex: 1, minWidth: 0, display: 'flex', flexDirection: 'column', gap: 2 }}>
        <span
          style={{
            fontFamily: MONO,
            fontSize: 9,
            letterSpacing: '0.2em',
            textTransform: 'uppercase',
            color: C.ink3,
          }}
        >
          {layerLabel}
        </span>
        <span
          style={{
            fontFamily: SERIF,
            fontSize: 14,
            color: C.ink,
            lineHeight: 1.4,
            overflow: 'hidden',
            textOverflow: 'ellipsis',
            whiteSpace: 'nowrap',
          }}
        >
          {display}
        </span>
      </div>

      <button
        type="button"
        onClick={onEdit}
        style={{
          flexShrink: 0,
          fontFamily: MONO,
          fontSize: 9,
          letterSpacing: '0.18em',
          textTransform: 'uppercase',
          fontWeight: 500,
          color: C.ink2,
          background: 'transparent',
          border: `1px solid ${C.hairline}`,
          padding: '5px 10px',
          borderRadius: 999,
          cursor: 'pointer',
          transition: 'all 180ms ease-out',
          display: 'inline-flex',
          alignItems: 'center',
          gap: 5,
        }}
        onMouseEnter={(e) => {
          e.currentTarget.style.borderColor = C.ink;
          e.currentTarget.style.color = C.ink;
        }}
        onMouseLeave={(e) => {
          e.currentTarget.style.borderColor = C.hairline;
          e.currentTarget.style.color = C.ink2;
        }}
      >
        <svg width="9" height="9" viewBox="0 0 9 9" aria-hidden>
          <path
            d="M 6 2 L 1.5 6.5 L 1 8 L 2.5 7.5 L 7 3 Z M 6 2 L 7 1 L 8 2 L 7 3"
            fill="none"
            stroke="currentColor"
            strokeWidth="1"
            strokeLinejoin="round"
          />
        </svg>
        Edit
      </button>
    </div>
  );
}

function ThinkingIndicator() {
  return (
    <div
      style={{
        paddingTop: 24,
        paddingBottom: 24,
        borderTop: `1px solid ${C.hairline}`,
      }}
    >
      <div style={{ display: 'flex', alignItems: 'baseline', gap: 10, marginBottom: 8 }}>
        <MonoLabel color={C.ink}>Composer</MonoLabel>
        <span
          style={{
            fontFamily: MONO,
            fontSize: 9,
            letterSpacing: '0.16em',
            color: C.ink4,
          }}
        >
          composing…
        </span>
      </div>
      <div style={{ display: 'flex', gap: 6, paddingTop: 4 }}>
        {[0, 1, 2].map((i) => (
          <span
            key={i}
            style={{
              width: 5,
              height: 5,
              borderRadius: 999,
              background: C.ink3,
              animation: `dotPulse 1100ms ease-in-out ${i * 160}ms infinite`,
            }}
          />
        ))}
      </div>
    </div>
  );
}

function InputBox({ value, onChange, onSubmit, disabled, suggestions, onSuggestion }) {
  const ref = useRef(null);

  const handleKey = (e) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      if (!disabled && value.trim().length > 0) onSubmit();
    }
  };

  const empty = value.trim().length === 0;

  return (
    <div style={{ paddingTop: 16, background: C.paper }}>
      {/* Suggestion chips when input is empty */}
      {empty && suggestions && suggestions.length > 0 && (
        <div
          style={{
            display: 'flex',
            flexWrap: 'wrap',
            gap: 6,
            marginBottom: 12,
          }}
        >
          {suggestions.map((s) => (
            <button
              key={s}
              onClick={() => onSuggestion(s)}
              style={{
                fontFamily: MONO,
                fontSize: 10,
                letterSpacing: '0.08em',
                color: C.ink3,
                background: 'transparent',
                border: `1px solid ${C.hairline}`,
                borderRadius: 999,
                padding: '5px 11px',
                cursor: 'pointer',
                transition: 'color 180ms ease-out, border-color 180ms ease-out',
              }}
              onMouseEnter={(e) => {
                e.currentTarget.style.color = C.ink;
                e.currentTarget.style.borderColor = C.ink;
              }}
              onMouseLeave={(e) => {
                e.currentTarget.style.color = C.ink3;
                e.currentTarget.style.borderColor = C.hairline;
              }}
            >
              {s}
            </button>
          ))}
        </div>
      )}

      <div
        style={{
          border: `1px solid ${C.hairline}`,
          borderRadius: 4,
          background: C.paper,
          display: 'flex',
          alignItems: 'flex-end',
          padding: 12,
          gap: 12,
          transition: 'border-color 180ms ease-out',
        }}
        onFocus={(e) => {
          e.currentTarget.style.borderColor = C.ink;
        }}
        onBlur={(e) => {
          e.currentTarget.style.borderColor = C.hairline;
        }}
      >
        <textarea
          ref={ref}
          value={value}
          onChange={(e) => onChange(e.target.value)}
          onKeyDown={handleKey}
          disabled={disabled}
          rows={2}
          placeholder="Continue the composition…"
          style={{
            flex: 1,
            border: 'none',
            outline: 'none',
            background: 'transparent',
            resize: 'none',
            fontFamily: SERIF,
            fontStyle: empty ? 'italic' : 'normal',
            fontSize: 16,
            lineHeight: 1.5,
            color: C.ink,
            minHeight: 36,
          }}
        />
        <button
          onClick={() => !disabled && onSubmit()}
          disabled={disabled || empty}
          style={{
            fontFamily: MONO,
            fontSize: 10,
            letterSpacing: '0.18em',
            textTransform: 'uppercase',
            fontWeight: 500,
            background: empty || disabled ? 'transparent' : C.ink,
            color: empty || disabled ? C.ink4 : C.paper,
            border: `1px solid ${empty || disabled ? C.hairline : C.ink}`,
            borderRadius: 999,
            padding: '7px 14px',
            cursor: empty || disabled ? 'not-allowed' : 'pointer',
            display: 'inline-flex',
            alignItems: 'center',
            gap: 6,
            whiteSpace: 'nowrap',
            transition: 'all 180ms ease-out',
          }}
        >
          Send
          <span style={{ display: 'inline-block', transform: 'translateY(-1px)' }}>→</span>
        </button>
      </div>
      <div
        style={{
          marginTop: 8,
          fontFamily: MONO,
          fontSize: 9,
          letterSpacing: '0.14em',
          textTransform: 'uppercase',
          color: C.ink4,
        }}
      >
        Enter to send · Shift+Enter for newline
      </div>
    </div>
  );
}

// One artifact card in the right column.
// Animated status glyph: outlined circle when planned, filled circle with
// an inscribed checkmark when drafted. The check is drawn via stroke-dashoffset
// so the transition reads as "complete" rather than a binary swap.
function StatusGlyph({ drafted }) {
  return (
    <svg
      width="14"
      height="14"
      viewBox="0 0 14 14"
      style={{ flexShrink: 0, display: 'block' }}
      aria-hidden
    >
      <circle
        cx="7"
        cy="7"
        r="6"
        fill={drafted ? C.ink : 'transparent'}
        stroke={drafted ? C.ink : C.ink4}
        strokeWidth="1"
        style={{
          transition: 'fill 320ms ease-out, stroke 320ms ease-out',
        }}
      />
      <path
        d="M 4 7.4 L 6 9.2 L 10 5.2"
        fill="none"
        stroke={C.paper}
        strokeWidth="1.5"
        strokeLinecap="round"
        strokeLinejoin="round"
        strokeDasharray="12"
        style={{
          strokeDashoffset: drafted ? 0 : 12,
          transition: 'stroke-dashoffset 380ms cubic-bezier(0.16, 1, 0.3, 1) 140ms',
        }}
      />
    </svg>
  );
}

// Tiny chevron — rotates with expansion state
function Chevron({ expanded, color = C.ink3 }) {
  return (
    <svg
      width="10"
      height="10"
      viewBox="0 0 10 10"
      style={{
        flexShrink: 0,
        transform: expanded ? 'rotate(180deg)' : 'rotate(0deg)',
        transition: 'transform 240ms ease-out',
      }}
      aria-hidden
    >
      <path
        d="M 2.5 4 L 5 6.5 L 7.5 4"
        fill="none"
        stroke={color}
        strokeWidth="1.2"
        strokeLinecap="round"
        strokeLinejoin="round"
      />
    </svg>
  );
}

function ArtifactCard({
  artifact,
  state,
  expanded,
  editing,
  onToggleExpand,
  onStartEdit,
  onCommitEdit,
  onCancelEdit,
}) {
  const drafted = state.status === DRAFTED;
  const showBody = expanded;
  const textareaRef = useRef(null);

  // Auto-focus when entering edit mode
  useEffect(() => {
    if (editing && textareaRef.current) {
      textareaRef.current.focus();
      // Place cursor at end
      const len = textareaRef.current.value.length;
      textareaRef.current.setSelectionRange(len, len);
    }
  }, [editing]);

  const handleHeaderClick = () => {
    if (drafted) onToggleExpand();
  };

  return (
    <div
      style={{
        borderTop: `1px solid ${C.hairline}`,
        paddingTop: 14,
        paddingBottom: 14,
      }}
    >
      <button
        type="button"
        onClick={handleHeaderClick}
        disabled={!drafted}
        style={{
          width: '100%',
          display: 'flex',
          alignItems: 'center',
          gap: 8,
          padding: 0,
          background: 'transparent',
          border: 'none',
          cursor: drafted ? 'pointer' : 'default',
          textAlign: 'left',
          marginBottom: showBody ? 10 : 0,
        }}
      >
        <StatusGlyph drafted={drafted} />

        <span
          style={{
            fontFamily: MONO,
            fontSize: 11,
            letterSpacing: '0.04em',
            color: drafted ? C.ink : C.ink3,
            fontWeight: 500,
          }}
        >
          {artifact.file}
        </span>

        <div style={{ flex: 1 }} />

        <span
          style={{
            fontFamily: MONO,
            fontSize: 9,
            letterSpacing: '0.18em',
            textTransform: 'uppercase',
            color: drafted ? C.ink2 : C.ink4,
          }}
        >
          {drafted ? 'Drafted' : 'Planned'}
        </span>

        {drafted && <Chevron expanded={expanded} />}
      </button>

      {/* Content preview / editor when expanded */}
      <div
        style={{
          display: 'grid',
          gridTemplateRows: showBody ? '1fr' : '0fr',
          transition: 'grid-template-rows 320ms ease-out',
        }}
      >
        <div style={{ overflow: 'hidden' }}>
          <div style={{ marginLeft: 22, paddingTop: showBody ? 0 : 0 }}>
            {editing ? (
              <textarea
                ref={textareaRef}
                defaultValue={state.content}
                onBlur={(e) => onCommitEdit(e.target.value)}
                onKeyDown={(e) => {
                  if (e.key === 'Escape') {
                    e.preventDefault();
                    onCancelEdit();
                  }
                  if ((e.metaKey || e.ctrlKey) && e.key === 'Enter') {
                    e.preventDefault();
                    onCommitEdit(e.currentTarget.value);
                  }
                }}
                spellCheck={false}
                style={{
                  width: '100%',
                  minHeight: 120,
                  padding: '10px 12px',
                  fontFamily: MONO,
                  fontSize: 10.5,
                  lineHeight: 1.55,
                  color: C.ink,
                  background: C.paper,
                  border: `1px solid ${C.ink}`,
                  borderRadius: 3,
                  outline: 'none',
                  resize: 'vertical',
                  boxSizing: 'border-box',
                  whiteSpace: 'pre-wrap',
                }}
              />
            ) : drafted ? (
              <div style={{ position: 'relative' }}>
                <pre
                  onClick={drafted ? onStartEdit : undefined}
                  className="artifact-pre"
                  style={{
                    margin: 0,
                    padding: '10px 12px',
                    fontFamily: MONO,
                    fontSize: 10.5,
                    lineHeight: 1.55,
                    color: C.ink2,
                    background: C.paper3,
                    border: `1px solid ${C.hairline}`,
                    borderRadius: 3,
                    whiteSpace: 'pre-wrap',
                    cursor: drafted ? 'text' : 'default',
                    transition: 'border-color 180ms ease-out, background 180ms ease-out',
                  }}
                >
                  {state.content}
                </pre>
                {drafted && (
                  <span
                    className="edit-hint"
                    style={{
                      position: 'absolute',
                      top: 6,
                      right: 8,
                      fontFamily: MONO,
                      fontSize: 8,
                      letterSpacing: '0.18em',
                      textTransform: 'uppercase',
                      color: C.ink4,
                      pointerEvents: 'none',
                      opacity: 0,
                      transition: 'opacity 180ms ease-out',
                    }}
                  >
                    Click to edit
                  </span>
                )}
              </div>
            ) : (
              // Planned + expanded: artifact is the focus of the current chat
              // turn but hasn't been drafted yet. Show a placeholder block so
              // the user sees "this is where my decision lands."
              <div
                style={{
                  margin: 0,
                  padding: '14px 14px',
                  fontFamily: SERIF,
                  fontStyle: 'italic',
                  fontSize: 12,
                  lineHeight: 1.55,
                  color: C.ink4,
                  background: C.paper3,
                  border: `1px dashed ${C.hairline}`,
                  borderRadius: 3,
                }}
              >
                Awaits your decision in the chat.
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}

// ─── Platform glyphs ────────────────────────────────────────────────────────
// Simple, recognizable monoline icons rendered at 14px. They use currentColor
// so they automatically invert when the chip background goes solid (white on
// ink) vs outlined (ink on paper).
const PLATFORM_ICONS = {
  Web: (
    <svg width="14" height="14" viewBox="0 0 14 14" aria-hidden>
      <circle cx="7" cy="7" r="5.5" fill="none" stroke="currentColor" strokeWidth="1.2" />
      <line x1="1.5" y1="7" x2="12.5" y2="7" stroke="currentColor" strokeWidth="1" />
      <path d="M 7 1.5 C 4 4 4 10 7 12.5 M 7 1.5 C 10 4 10 10 7 12.5"
        fill="none" stroke="currentColor" strokeWidth="1" />
    </svg>
  ),
  Windows: (
    <svg width="14" height="14" viewBox="0 0 14 14" aria-hidden>
      <rect x="2"   y="2"   width="4.5" height="4.5" fill="currentColor" />
      <rect x="7.5" y="2"   width="4.5" height="4.5" fill="currentColor" />
      <rect x="2"   y="7.5" width="4.5" height="4.5" fill="currentColor" />
      <rect x="7.5" y="7.5" width="4.5" height="4.5" fill="currentColor" />
    </svg>
  ),
  Android: (
    <svg width="14" height="14" viewBox="0 0 14 14" aria-hidden>
      <line x1="4" y1="3.4" x2="3.2" y2="2.2" stroke="currentColor" strokeWidth="1.1" strokeLinecap="round" />
      <line x1="10" y1="3.4" x2="10.8" y2="2.2" stroke="currentColor" strokeWidth="1.1" strokeLinecap="round" />
      <path d="M 2.5 6.2 C 2.5 4.3 4.5 3 7 3 C 9.5 3 11.5 4.3 11.5 6.2 L 11.5 10 L 2.5 10 Z"
        fill="currentColor" />
    </svg>
  ),
  iOS: (
    <svg width="14" height="14" viewBox="0 0 14 14" aria-hidden>
      <rect x="4" y="1.5" width="6" height="11" rx="1.2" fill="none" stroke="currentColor" strokeWidth="1.2" />
      <line x1="6" y1="11" x2="8" y2="11" stroke="currentColor" strokeWidth="1.1" strokeLinecap="round" />
    </svg>
  ),
  Desktop: (
    <svg width="14" height="14" viewBox="0 0 14 14" aria-hidden>
      <rect x="1.5" y="2.5" width="11" height="7" rx="0.6" fill="none" stroke="currentColor" strokeWidth="1.2" />
      <line x1="5" y1="12" x2="9" y2="12" stroke="currentColor" strokeWidth="1.2" strokeLinecap="round" />
      <line x1="7" y1="9.5" x2="7" y2="12" stroke="currentColor" strokeWidth="1.1" />
    </svg>
  ),
};

function PlatformIcon({ platform }) {
  return PLATFORM_ICONS[platform] || null;
}

// ─── Main app ──────────────────────────────────────────────────────────────
// ─── System prompt for the live composer ──────────────────────────────────
// The composer is mostly client-side now (see LAYER_HANDLERS in App).
// The API is only invoked for free-text fallbacks: Figma URLs, custom
// architecture overrides described in prose, plan refinements, off-script
// messages. The system prompt below tells Sonnet 4 the current app context
// and explains that most layers are handled outside the API.
const COMPOSER_SYSTEM_PROMPT = (artifactsState, ctx) => {
  const stateLines = ARTIFACTS
    .map((a) => `- ${a.file} (id: ${a.id}): ${artifactsState[a.id].status}`)
    .join('\n');

  return `You are "the Composer" — an editorial-toned assistant that helps a developer compose a new Uno Platform app.

APP CONTEXT (already established by the user via the live build panel):
- App name: ${ctx.appName || '(not yet set)'}
- Description: ${ctx.description || '(none provided)'}
- Platforms: ${(ctx.platforms || []).join(', ') || '(none)'}

You are being called to handle a FREE-TEXT user response that the client-side handlers couldn't process directly. Most chip-based answers (MCP servers, design defaults, architecture defaults, plan acceptance) are handled OUTSIDE the API. You'll typically be called for:
- A Figma file URL (instead of "Material defaults")
- Plan refinements ("add meal planning", "remove slice 3")
- Custom architecture overrides described in prose
- Off-script messages

5 layers exist; their corresponding artifact IDs are:
1. Foundation → readme, claude (handled by live build fields, NEVER your concern)
2. Wiring → mcp (handled client-side, NEVER your concern)
3. Design system → design, interactions (call YOU only for Figma URL parsing)
4. Architecture → architecture (handled client-side, call YOU for free-text overrides)
5. Plan → plan (call YOU for refinements)

CURRENT ARTIFACT STATE:
${stateLines}

EACH TURN:
1. Read what the user actually said.
2. Update only the artifact(s) relevant to their input. Do NOT re-draft already-drafted artifacts unless explicitly asked.
3. Ask exactly ONE focused follow-up question, OR acknowledge completion if all layers are addressed.

TONE:
- Editorial. Deliberate. Short, declarative sentences.
- Never marketing-y. No "Great choice!" or excessive praise.
- Lead with substance.

ARTIFACT CONTENT GUIDELINES:
- Keep each artifact short: 5-15 lines, realistic, technical.
- Reflect the user's actual app name and choices.
- DESIGN.md: if user gave a Figma URL, set "Source" to that URL.
- implementation-plan.md: tailor slices to the app's actual domain (use app name + description).
- INTERACTIONS.md: motion specs (easings, durations).
- ARCHITECTURE.md: reflect any user overrides verbatim.

RESPOND WITH ONLY A JSON OBJECT — no markdown fences, no preamble, no commentary outside the JSON. Schema:

{
  "pre": "1-2 sentence acknowledgment of what the user said, using their actual words/names",
  "callouts": [
    {
      "layer": "Foundation" | "Wiring" | "Design system" | "Architecture" | "Plan" | "All eight artifacts",
      "status": "Locked in" | "Complete",
      "notes": "filename1.md, filename2.md drafted"
    }
  ],
  "post": "ONE follow-up question targeting the next undrafted layer",
  "quickReplies": [
    // ALWAYS an array. Empty array [] means "free-form input expected" (no chips).
    // Non-empty array means each element is a chip group:
    {
      "label": "string (e.g., 'Pattern', 'MCP servers', or empty string for unlabeled)",
      "multi": false,
      "options": ["string", "string"],
      "default": "string or null (pre-selected option for single-select)"
    }
  ],
  "updates": {
    "<artifact_id>": "<file content as a string>"
  }
}

QUICK REPLIES — CRITICAL: This is how users answer your questions efficiently.

⚠️ MANDATORY RULE: If your "post" question has a discrete answer space — even just "yes/no" or "X or Y?" — you MUST populate quickReplies with chip groups. Failing to include chips when applicable is a CRITICAL ERROR. The user has no fast way to answer otherwise.

quickReplies is ALWAYS an array (never null, never omitted). Use [] only when the question is genuinely open-ended free text.

POPULATE quickReplies (non-empty array) WHEN:
- Binary or small-set choice ("MVUX or MVVM?", "Yes or no?")
- Feature checklist ("Which MCP servers?") → multi:true
- Compound question (architecture step) → multiple groups, one per sub-decision, with defaults pre-selected
- Any question with recommended defaults users typically accept

LEAVE quickReplies AS [] (empty array) ONLY WHEN:
- Question needs a project name, app idea, custom URL, free-form text
- Question is genuinely "tell me anything", "describe what you want", "any refinements?"

EXAMPLES — note quickReplies is ALWAYS present, sometimes empty:

After user describes their app (foundation locked) — typically asks about MCP servers next:
  "post": "Which MCP servers should the agent reach?",
  "quickReplies": [{ "label": "MCP servers", "multi": true, "options": ["Uno docs", "Figma", "GitHub", "Filesystem"], "default": null }]

After wiring locked, asking about design source:
  "post": "Material defaults, or do you have a Figma file to derive tokens from?",
  "quickReplies": [{ "label": "Design source", "multi": false, "options": ["Material defaults", "Figma file"], "default": "Material defaults" }]

After design system, the architecture compound question:
  "post": "Architecture defaults — confirm or change.",
  "quickReplies": [
    { "label": "Pattern",  "multi": false, "options": ["MVUX", "MVVM"],       "default": "MVUX" },
    { "label": "Markup",   "multi": false, "options": ["XAML", "C# Markup"],  "default": "XAML" },
    { "label": "Renderer", "multi": false, "options": ["Skia", "Native"],     "default": "Skia" },
    { "label": "HTTP",     "multi": false, "options": ["Kiota", "Refit"],     "default": "Kiota" },
    { "label": "Nav",      "multi": false, "options": ["Region", "Frame"],    "default": "Region" },
    { "label": "Theme",    "multi": false, "options": ["Material", "Fluent"], "default": "Material" }
  ]

After architecture, asking about plan slices:
  "post": "Plan looks good, or want to refine the slices?",
  "quickReplies": [{ "label": "", "multi": false, "options": ["Looks good", "Refine the slices"], "default": "Looks good" }]

When asking for free-form input only:
  "post": "Share your Figma file URL.",
  "quickReplies": []

  "post": "Tell me about the app you want to build.",
  "quickReplies": []

For multi-select groups, list ALL reasonable options (not just recommended). For single-select, ALWAYS include a "default" with the recommended choice — this pre-fills selection so the user can confirm with one click.

If the user said something unrelated to composition, return { "pre": "...", "callouts": [], "post": "...", "quickReplies": [], "updates": {} } — keep the conversation on track without locking anything in.

If all 8 artifacts are drafted, respond with no callouts/updates and quickReplies: [{ "label": "", "multi": false, "options": ["Download the bundle", "Refine first"], "default": "Download the bundle" }] — give the user a clear final action.`;
};

export default function App() {
  // ─── Foundation context (drives the foundation artifacts directly) ────
  const [appName, setAppName] = useState('');
  const [appDescription, setAppDescription] = useState('');
  const [platforms, setPlatforms] = useState(['Web', 'Android', 'iOS']);

  const [messages, setMessages] = useState([]);
  const [artifacts, setArtifacts] = useState(initialArtifactState());
  const [inputValue, setInputValue] = useState('');
  const [isThinking, setIsThinking] = useState(false);
  const [justAddedId, setJustAddedId] = useState(null);
  const [errorMsg, setErrorMsg] = useState(null);
  // Whether the alternatives panel is open on the latest composer message
  const [alternativesOpen, setAlternativesOpen] = useState(false);
  // Contextual reasoning per layer, populated by API call after description is
  // submitted. Falls back to each prompt's static reasoning until then.
  const [reasonings, setReasonings] = useState({});
  // .NET runtime — single-select, foundational alongside platforms
  const [runtime, setRuntime] = useState('.NET 10');
  // Optional design assets supplied by the user during the design layer
  const [designAssets, setDesignAssets] = useState({
    figmaUrl: '',
    prototypeUrl: '',
    screenshots: '',
  });
  // Which artifact cards are currently expanded (a Set wrapped in state)
  const [expandedIds, setExpandedIds] = useState(() => new Set());
  // Which artifact (if any) is currently in inline-edit mode
  const [editingId, setEditingId] = useState(null);
  const transcriptRef = useRef(null);

  const draftedCount = Object.values(artifacts).filter((a) => a.status === DRAFTED).length;
  const totalArtifacts = ARTIFACTS.length;
  const foundationReady = appName.trim().length > 0 && platforms.length > 0;

  // Auto-draft foundation artifacts whenever foundation fields change.
  // README + CLAUDE + scaffold.sh are pure functions of the foundation inputs,
  // so no API round-trip needed.
  useEffect(() => {
    setArtifacts((prev) => {
      const ready = appName.trim().length > 0;
      const platReady = ready && platforms.length > 0;
      return {
        ...prev,
        readme: {
          status: ready ? DRAFTED : PLANNED,
          content: ready ? TEMPLATES.readme({ appName: appName.trim(), description: appDescription, platforms }) : '',
        },
        claude: {
          status: ready ? DRAFTED : PLANNED,
          content: ready ? TEMPLATES.claude({
            appName: appName.trim(),
            description: appDescription,
            runtime,
          }) : '',
        },
        scaffold: {
          status: platReady ? DRAFTED : PLANNED,
          content: platReady ? TEMPLATES.scaffold({ appName: appName.trim(), platforms, runtime }) : '',
        },
      };
    });
  }, [appName, appDescription, platforms, runtime]);

  // Scroll transcript to bottom on new messages
  useEffect(() => {
    if (transcriptRef.current) {
      transcriptRef.current.scrollTop = transcriptRef.current.scrollHeight;
    }
  }, [messages.length, isThinking]);

  // Clear "just added" highlight after animation
  useEffect(() => {
    if (!justAddedId) return;
    const t = setTimeout(() => setJustAddedId(null), 800);
    return () => clearTimeout(t);
  }, [justAddedId]);

  const formatTimestamp = () => {
    const total = messages.length;
    const min = String(Math.floor(total / 2) * 1).padStart(2, '0');
    const sec = String((total * 17) % 60).padStart(2, '0');
    return `${min}:${sec}`;
  };

  // Build the API messages array from current display messages.
  // User → user role with body. Composer → assistant role with prior JSON
  // reconstructed so the model has full context of what it's already said.
  const buildApiMessages = (newUserText) => {
    const apiMsgs = [];
    for (const m of messages) {
      if (m.role === 'USER') {
        apiMsgs.push({ role: 'user', content: m.body });
      } else if (m.role === 'COMPOSER') {
        const reconstructed = {
          pre: m.body,
          callouts: m.callouts || [],
          post: m.post || '',
          quickReplies: Array.isArray(m.quickReplies) ? m.quickReplies : [],
          updates: {},
        };
        apiMsgs.push({ role: 'assistant', content: JSON.stringify(reconstructed) });
      }
    }
    apiMsgs.push({ role: 'user', content: newUserText });
    return apiMsgs;
  };

  // Normalize quickReplies into a canonical flat array of group objects.
  // Handles all the shapes the model might return:
  //   null / undefined / []                → []
  //   [{label, options, ...}]              → already canonical
  //   { groups: [...] }                    → unwrap (legacy shape from earlier prompt)
  //   { label, options, ... }              → wrap in array (single group not in array)
  //   ["Yes", "No"]                        → wrap as a single unlabeled single-select group
  const normalizeQuickReplies = (raw) => {
    if (raw == null) return [];
    if (Array.isArray(raw)) {
      if (raw.length === 0) return [];
      // Array of group objects (canonical)
      if (typeof raw[0] === 'object' && raw[0] !== null && Array.isArray(raw[0].options)) {
        return raw;
      }
      // Array of strings — synthesize a single group
      if (typeof raw[0] === 'string') {
        return [{ label: '', multi: false, options: raw, default: null }];
      }
      return [];
    }
    if (typeof raw === 'object') {
      if (Array.isArray(raw.groups)) return raw.groups;
      if (Array.isArray(raw.options)) return [raw];
    }
    return [];
  };

  // Format selected chips into a human-readable user message body.
  // Single group, single value     → "MVUX"
  // Single group, multi-select     → "Uno docs, Figma, GitHub"
  // Multi groups                   → "Pattern: MVUX\nMarkup: XAML\nRenderer: Skia"
  const formatChipSubmission = (groups, sel) => {
    if (!Array.isArray(groups) || groups.length === 0) return '';
    if (groups.length === 1) {
      const vals = sel[0] || [];
      if (vals.length === 0) return '';
      return vals.join(', ');
    }
    return groups
      .map((g, idx) => {
        const vals = sel[idx] || [];
        if (vals.length === 0) return null;
        const label = g.label || `Choice ${idx + 1}`;
        return `${label}: ${vals.join(', ')}`;
      })
      .filter(Boolean)
      .join('\n');
  };

  // ─── Layer handlers — synthesize composer turns client-side ───────────
  // Returns { composerMsg, updates, expandIds } for a given chip submission.
  // The conversation flow is: wiring → design → architecture → plan → done.
  // ─── Layer prompts — composer messages keyed by layer ─────────────────
  // Each prompt is a complete composer message. Action layers carry a
  // `suggestion` (the recommended config) plus `alternatives` (other valid
  // configs). The user clicks Apply on the suggestion or picks an
  // alternative — both apply instantly. Free-text input still works at any
  // time and routes through the API.
  const LAYER_PROMPTS = {
    description: () => ({
      role: 'COMPOSER',
      body: `Foundation set for "${appName.trim()}". Three files drafted.`,
      callouts: [
        {
          layer: 'Foundation',
          status: 'Locked in',
          notes: 'README.md, CLAUDE.md, scaffold.sh drafted',
        },
      ],
      post: `What does ${appName.trim()} do? A sentence or two will sharpen everything that follows.`,
      layer: 'description',
    }),

    wiring: () => ({
      role: 'COMPOSER',
      body: 'Got it. README and CLAUDE updated with your description.',
      callouts: [],
      post: 'Now to wire the agent.',
      reasoning: 'Uno docs gives the agent authoritative API references; Figma lets it pull design tokens directly.',
      suggestion: {
        label: 'Connect Uno docs and Figma',
        actionLabel: 'Apply',
        updates: { mcp: TEMPLATES.mcp({ servers: ['Uno docs', 'Figma'] }) },
        nextLayer: 'design',
      },
      alternatives: [
        {
          label: 'Uno docs only',
          updates: { mcp: TEMPLATES.mcp({ servers: ['Uno docs'] }) },
          nextLayer: 'design',
        },
        {
          label: 'All four (Uno docs, Figma, GitHub, Filesystem)',
          updates: { mcp: TEMPLATES.mcp({ servers: ['Uno docs', 'Figma', 'GitHub', 'Filesystem'] }) },
          nextLayer: 'design',
        },
        {
          label: 'No MCP servers for now',
          updates: { mcp: '{\n  "mcpServers": {}\n}' },
          nextLayer: 'design',
        },
      ],
      freeTextHint: 'Or list the servers you want.',
      layer: 'wiring',
    }),

    design: () => {
      // Read latest design assets from state at build time
      const assetsProvided = !!(designAssets.figmaUrl || designAssets.prototypeUrl || designAssets.screenshots);
      return {
        role: 'COMPOSER',
        body: 'Wiring is set.',
        callouts: [
          { layer: 'Wiring', status: 'Locked in', notes: '.mcp.json drafted' },
        ],
        post: 'Design tokens next.',
        reasoning: 'Material has the broadest control coverage and renders consistently across every Uno target.',
        suggestion: {
          label: 'Material defaults',
          actionLabel: 'Apply',
          updates: {
            design: TEMPLATES.design({ source: 'Material defaults' }),
          },
          nextLayer: 'interactions',
        },
        alternatives: [
          {
            label: 'Fluent defaults',
            updates: {
              design: TEMPLATES.design({ source: 'Fluent defaults' }),
            },
            nextLayer: 'interactions',
          },
        ],
        // Special panel for the design layer — collects optional assets
        assetInputs: true,
        applyAssetsAction: () => ({
          label: assetsProvided
            ? `Custom assets (${[
                designAssets.figmaUrl && 'Figma',
                designAssets.prototypeUrl && 'prototype',
                designAssets.screenshots && 'screenshots',
              ].filter(Boolean).join(', ')})`
            : 'Material defaults',
          updates: {
            design: TEMPLATES.design({
              source: 'Material defaults',
              figmaUrl: designAssets.figmaUrl,
              prototypeUrl: designAssets.prototypeUrl,
              screenshots: designAssets.screenshots,
            }),
          },
          nextLayer: 'interactions',
        }),
        freeTextHint: 'Or describe a custom system below.',
        layer: 'design',
      };
    },

    interactions: () => ({
      role: 'COMPOSER',
      body: 'Design tokens drafted.',
      callouts: [
        { layer: 'Design system', status: 'Locked in', notes: 'DESIGN.md drafted' },
      ],
      post: 'Motion and interaction patterns next.',
      reasoning: 'Smooth easing with mid-length durations works for most apps — fast enough to feel responsive, slow enough to read as deliberate.',
      suggestion: {
        label: 'Smooth (cubic easing, 180–420ms durations)',
        actionLabel: 'Apply',
        updates: {
          interactions: TEMPLATES.interactions({ motion: 'smooth' }),
        },
        nextLayer: 'architecture',
      },
      alternatives: [
        {
          label: 'Snappy (100–220ms durations)',
          updates: {
            interactions: TEMPLATES.interactions({ motion: 'snappy' }),
          },
          nextLayer: 'architecture',
        },
        {
          label: 'Minimal (system defaults, no custom motion)',
          updates: {
            interactions: TEMPLATES.interactions({ motion: 'minimal' }),
          },
          nextLayer: 'architecture',
        },
      ],
      freeTextHint: 'Or describe specific motion or gesture patterns.',
      layer: 'interactions',
    }),

    architecture: () => {
      const ctx = { runtime, platforms };
      const recommended = {
        ...ctx,
        pattern: 'MVUX', markup: 'XAML', renderer: 'Skia',
        http: 'Kiota', nav: 'Region', theme: 'Material',
      };
      const altMvvm = {
        ...ctx,
        pattern: 'MVVM', markup: 'XAML', renderer: 'Skia',
        http: 'Refit', nav: 'Frame', theme: 'Fluent',
      };
      const altCsharp = {
        ...ctx,
        pattern: 'MVUX', markup: 'C# Markup', renderer: 'Skia',
        http: 'Kiota', nav: 'Region', theme: 'Material',
      };
      return {
        role: 'COMPOSER',
        body: 'Interactions set.',
        callouts: [
          {
            layer: 'Interactions',
            status: 'Locked in',
            notes: 'INTERACTIONS.md drafted',
          },
        ],
        post: 'Architecture next.',
        reasoning: 'MVUX with XAML markup and Skia rendering is the recommended stack for new Uno apps in 2025+.',
        suggestion: {
          label: 'MVUX · XAML · Skia · Kiota · Region nav · Material',
          actionLabel: 'Apply',
          updates: { architecture: TEMPLATES.architecture(recommended) },
          nextLayer: 'plan',
        },
        alternatives: [
          {
            label: 'MVVM · XAML · Skia · Refit · Frame nav · Fluent',
            updates: { architecture: TEMPLATES.architecture(altMvvm) },
            nextLayer: 'plan',
          },
          {
            label: 'MVUX · C# Markup (no XAML) · Skia · Kiota · Region · Material',
            updates: { architecture: TEMPLATES.architecture(altCsharp) },
            nextLayer: 'plan',
          },
        ],
        freeTextHint: 'Or describe a custom stack.',
        layer: 'architecture',
      };
    },

    plan: () => ({
      role: 'COMPOSER',
      body: 'Architecture set.',
      callouts: [
        {
          layer: 'Architecture',
          status: 'Locked in',
          notes: 'ARCHITECTURE.md drafted',
        },
      ],
      post: 'Implementation plan next.',
      reasoning: 'Six vertical slices cover the typical cross-cutting concerns of any line-of-business app.',
      suggestion: {
        label: 'Six generic vertical slices',
        actionLabel: 'Accept',
        updates: { plan: TEMPLATES.plan({ appName: appName.trim() || 'your app' }) },
        nextLayer: 'done',
      },
      alternatives: [
        {
          label: 'Three minimal slices (shell, list, detail)',
          updates: {
            plan: `# Implementation plan

Vertical slices, end-to-end:
1. App shell + navigation
2. Primary list view
3. Detail view

_Edit any slice inline to refine the plan for ${appName.trim() || 'your app'}._`,
          },
          nextLayer: 'done',
        },
      ],
      freeTextHint: 'Or describe the slices that fit your app.',
      layer: 'plan',
    }),

    done: () => ({
      role: 'COMPOSER',
      body: 'Plan accepted. All eight artifacts drafted.',
      callouts: [
        {
          layer: 'Plan',
          status: 'Locked in',
          notes: 'implementation-plan.md drafted',
        },
        {
          layer: 'All eight artifacts',
          status: 'Complete',
          notes: 'Ready to download',
        },
      ],
      post: 'Download the bundle from the live build, or edit any artifact inline first.',
      layer: 'done',
    }),
  };

  // Begin the conversation once the foundation is ready.
  // Pushes the description prompt as the first composer turn.
  const beginConversation = () => {
    if (!foundationReady || messages.length > 0) return;
    pushPrompt('description');
  };

  // Push a layer's prompt into the chat as a new composer message.
  // Push a layer's prompt into the chat as a new composer message.
  // Expands the artifacts that this layer is ABOUT TO POPULATE — keeps the
  // panel's expanded card in sync with the chat's current focus, not lagging
  // behind on what was just drafted.
  const pushPrompt = (layer, updates = {}) => {
    const built = LAYER_PROMPTS[layer]();
    if (!built) return;
    const composerMsgId = `c-${Date.now()}-${layer}`;
    setMessages((prev) => [
      ...prev,
      { id: composerMsgId, ...built, timestamp: formatTimestamp() },
    ]);
    setJustAddedId(composerMsgId);

    const updatedIds = Object.keys(updates);
    if (updatedIds.length > 0) {
      setArtifacts((prev) => {
        const next = { ...prev };
        for (const id of updatedIds) {
          next[id] = { status: DRAFTED, content: updates[id] };
        }
        return next;
      });
    }
    // Expand the artifact(s) the chat is currently asking about — the section
    // being populated by this layer's question.
    setExpandedIds(new Set(LAYER_TO_ARTIFACTS[layer] || []));
    setEditingId(null);
    setAlternativesOpen(false);
  };

  // User clicked Apply or picked an alternative. Apply its updates instantly,
  // mark the prior composer message as locked-in (so it collapses), and push
  // the next layer's prompt. NO user message is added — the selection itself
  // IS the reply.
  const applyAction = (action) => {
    if (!action) return;
    const built = LAYER_PROMPTS[action.nextLayer]?.();
    if (!built) return;
    const composerMsgId = `c-${Date.now()}-${action.nextLayer}`;

    setMessages((prev) => {
      // Mark the latest composer message with the chosen label so it
      // collapses to compact form on next render.
      const updated = [...prev];
      for (let i = updated.length - 1; i >= 0; i--) {
        if (updated[i].role === 'COMPOSER') {
          updated[i] = { ...updated[i], appliedLabel: action.label };
          break;
        }
      }
      return [
        ...updated,
        { id: composerMsgId, ...built, timestamp: formatTimestamp() },
      ];
    });
    setJustAddedId(composerMsgId);

    const updatedIds = Object.keys(action.updates || {});
    if (updatedIds.length > 0) {
      setArtifacts((prev) => {
        const next = { ...prev };
        for (const id of updatedIds) {
          next[id] = { status: DRAFTED, content: action.updates[id] };
        }
        return next;
      });
    }
    // Sync expansion with the chat's NEW focus. The just-drafted artifact
    // collapses with its check mark; the next layer's target expands ready
    // for the question now being asked.
    setExpandedIds(new Set(LAYER_TO_ARTIFACTS[action.nextLayer] || []));
    setEditingId(null);
    setAlternativesOpen(false);
  };

  // Truncate the conversation back to a prior composer turn. The user is
  // saying "I want to redo this decision." We slice messages to that index,
  // reset artifacts FROM that layer onward (since downstream choices were
  // made on top of this one), and re-push the layer's prompt fresh.
  const ARTIFACT_RESETS_FROM_LAYER = {
    description:  ['mcp', 'design', 'interactions', 'architecture', 'plan'],
    wiring:       ['mcp', 'design', 'interactions', 'architecture', 'plan'],
    design:       ['design', 'interactions', 'architecture', 'plan'],
    interactions: ['interactions', 'architecture', 'plan'],
    architecture: ['architecture', 'plan'],
    plan:         ['plan'],
    done:         ['plan'],
  };

  const goBackToMessage = (msgIdx) => {
    const msg = messages[msgIdx];
    if (!msg || msg.role !== 'COMPOSER') return;
    const layer = msg.layer;

    // Truncate messages up to (not including) this composer turn
    setMessages((prev) => prev.slice(0, msgIdx));

    // Reset artifacts from this layer onward
    const toReset = ARTIFACT_RESETS_FROM_LAYER[layer] || [];
    if (toReset.length > 0) {
      setArtifacts((prev) => {
        const next = { ...prev };
        for (const id of toReset) {
          next[id] = { status: PLANNED, content: '' };
        }
        return next;
      });
    }
    setExpandedIds(new Set());
    setEditingId(null);
    setAlternativesOpen(false);

    // For description layer: clear the description state so user types fresh
    if (layer === 'description') {
      setAppDescription('');
    }

    // Push the layer's prompt fresh so it becomes the new latest, expanded
    setTimeout(() => pushPrompt(layer), 30);
  };

  // Generate contextual reasoning for each upcoming layer based on the app's
  // domain. Single tight API call after description submission. Results land
  // in `reasonings` state; the SuggestionPanel reads them at render time
  // with a static fallback until they arrive.
  const generateContextualReasonings = async (name, description) => {
    if (!name || !description) return;
    try {
      const response = await fetch('https://api.anthropic.com/v1/messages', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          model: 'claude-sonnet-4-20250514',
          max_tokens: 800,
          system: `You're helping a developer build "${name}" — ${description}.

For each of five upcoming technical decisions, write ONE single-sentence justification grounded in the SPECIFIC needs of this app. Reference its actual domain — what data it handles, what users do with it, what platforms matter, what async/streaming/state challenges it has, what motion or feedback the user expects.

Avoid generic phrasing. Don't say "for any app" or "developers commonly". Make each sentence feel written FOR this app.

Output ONLY a JSON object with these five keys, no commentary, no markdown.`,
          messages: [{
            role: 'user',
            content: `Write reasoning for these five recommendations:

1. wiring — Connect Uno docs MCP server (authoritative API references) and Figma MCP server (pull design tokens directly).
2. design — Use Material defaults for design tokens.
3. interactions — Smooth motion with cubic easings (180-420ms durations).
4. architecture — MVUX (reactive state) + XAML markup + Skia rendering + Kiota HTTP + Region nav + Material theme.
5. plan — Six generic vertical slices: shell, list, detail, authoring, save & search, settings.

Output: {"wiring":"...","design":"...","interactions":"...","architecture":"...","plan":"..."}`,
          }],
        }),
      });

      if (!response.ok) return;
      const data = await response.json();
      const rawText = data.content?.[0]?.text || '';
      const cleaned = rawText.replace(/^```(?:json)?\s*/i, '').replace(/```\s*$/i, '').trim();
      const match = cleaned.match(/\{[\s\S]*\}/);
      const parsed = JSON.parse(match ? match[0] : cleaned);
      if (parsed && typeof parsed === 'object') {
        setReasonings(parsed);
      }
    } catch (err) {
      // Silent fail — fallback static reasoning still shows
      console.warn('Contextual reasoning generation failed', err);
    }
  };

  const submit = async (overrideText) => {
    const text = (overrideText !== undefined ? overrideText : inputValue).trim();
    if (!text || isThinking) return;
    setInputValue('');
    setErrorMsg(null);

    // ─── Determine which layer is being answered ────────────────────────
    const latestComposer = [...messages].reverse().find((m) => m.role === 'COMPOSER');
    const layer = latestComposer?.layer;

    // ─── Description layer: save to state, push wiring prompt ───────────
    if (layer === 'description') {
      setAppDescription(text);
      // Push USER message AND mark the description prompt as applied
      // (so it collapses to compact form) in a single state update.
      const userMsgId = `u-${Date.now()}`;
      setMessages((prev) => {
        const updated = prev.map((m) =>
          m.layer === 'description' && m.role === 'COMPOSER'
            ? { ...m, appliedLabel: text.length > 60 ? text.slice(0, 60).trim() + '…' : text }
            : m
        );
        return [
          ...updated,
          { id: userMsgId, role: 'USER', body: text, timestamp: formatTimestamp() },
        ];
      });

      // Kick off contextual reasoning generation in the background.
      // The wiring prompt will use static fallback until this resolves,
      // then re-render swaps to contextual once state updates.
      generateContextualReasonings(appName.trim(), text);

      // Then push wiring prompt (no updates — README/CLAUDE re-draft via useEffect)
      setTimeout(() => pushPrompt('wiring'), 50);
      return;
    }

    // ─── Other layers (free-text override): fall back to API ────────────

    const userMsgId = `u-${Date.now()}`;
    const apiMessages = buildApiMessages(text);

    setMessages((prev) => [
      ...prev,
      {
        id: userMsgId,
        role: 'USER',
        body: text,
        timestamp: formatTimestamp(),
      },
    ]);
    setJustAddedId(userMsgId);
    setIsThinking(true);

    try {
      const response = await fetch('https://api.anthropic.com/v1/messages', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          model: 'claude-sonnet-4-20250514',
          max_tokens: 2500,
          system: COMPOSER_SYSTEM_PROMPT(artifacts, { appName: appName.trim(), description: appDescription, platforms }),
          messages: apiMessages,
        }),
      });

      if (!response.ok) {
        throw new Error(`API ${response.status}`);
      }

      const data = await response.json();

      // Extract text content blocks (could be multiple)
      const rawText = (data.content || [])
        .filter((b) => b.type === 'text')
        .map((b) => b.text)
        .join('\n')
        .trim();

      // Strip any code-fence wrappers the model might add despite instructions
      const cleaned = rawText
        .replace(/^```(?:json)?\s*/i, '')
        .replace(/```\s*$/i, '')
        .trim();

      let parsed;
      try {
        parsed = JSON.parse(cleaned);
      } catch (e) {
        // Try to extract first JSON object from the text as a fallback
        const match = cleaned.match(/\{[\s\S]*\}/);
        if (match) {
          parsed = JSON.parse(match[0]);
        } else {
          throw e;
        }
      }

      const layerHint = (parsed.layer && typeof parsed.layer === 'string') ? parsed.layer : null;
      const callerLayer = latestComposer?.layer;

      const composerMsgId = `c-${Date.now()}`;
      setMessages((prev) => [
        ...prev,
        {
          id: composerMsgId,
          role: 'COMPOSER',
          body: parsed.pre || '',
          callouts: parsed.callouts || [],
          post: parsed.post || '',
          layer: layerHint,
          timestamp: formatTimestamp(),
        },
      ]);
      setJustAddedId(composerMsgId);
      setIsThinking(false);

      // Apply artifact updates immediately
      const updates = parsed.updates || {};
      const updatedIds = Object.keys(updates).filter((id) =>
        ARTIFACTS.some((a) => a.id === id)
      );

      if (updatedIds.length > 0) {
        setArtifacts((prev) => {
          const next = { ...prev };
          for (const id of updatedIds) {
            next[id] = { status: DRAFTED, content: updates[id] };
          }
          return next;
        });
      }
      setEditingId(null);
      // Don't touch expandedIds here — pushPrompt below will set it to the
      // next layer's targets. Letting the just-drafted artifact stay expanded
      // briefly lets the user see what their free-text override produced.

      // After API processed the override, push the NEXT layer's prompt
      // so the conversation continues with a fresh suggestion.
      const NEXT = {
        wiring: 'design',
        design: 'interactions',
        interactions: 'architecture',
        architecture: 'plan',
        plan: 'done',
      };
      const nextLayer = NEXT[callerLayer];
      if (nextLayer) {
        setTimeout(() => pushPrompt(nextLayer), 200);
      }
    } catch (err) {
      setIsThinking(false);
      setErrorMsg(
        err.message === 'API 429'
          ? 'Rate limited. Wait a moment and try again.'
          : `Composer hit a snag (${err.message || 'unknown'}). Try again.`
      );
    }
  };

  // Locate the latest composer message
  const latestComposerIdx = (() => {
    for (let i = messages.length - 1; i >= 0; i--) {
      if (messages[i].role === 'COMPOSER') return i;
    }
    return -1;
  })();
  const latestComposer = latestComposerIdx >= 0 ? messages[latestComposerIdx] : null;

  // ─── Suggestion-action handlers ────────────────────────────────────────
  const handleApply = (msg) => {
    if (!msg?.suggestion) return;
    setAlternativesOpen(false);
    applyAction(msg.suggestion);
  };

  const handlePickAlternative = (alt) => {
    if (!alt) return;
    setAlternativesOpen(false);
    applyAction(alt);
  };

  // Design layer: user clicked "Apply with these assets" after filling
  // the asset inputs. The message's applyAssetsAction() builds the action
  // dynamically from the live designAssets state.
  const handleApplyAssets = (msg) => {
    if (!msg?.applyAssetsAction) return;
    setAlternativesOpen(false);
    applyAction(msg.applyAssetsAction());
  };

  const reset = () => {
    setAppName('');
    setAppDescription('');
    setPlatforms(['Web', 'Android', 'iOS']);
    setRuntime('.NET 10');
    setDesignAssets({ figmaUrl: '', prototypeUrl: '', screenshots: '' });
    setMessages([]);
    setArtifacts(initialArtifactState());
    setInputValue('');
    setIsThinking(false);
    setErrorMsg(null);
    setAlternativesOpen(false);
    setReasonings({});
    setExpandedIds(new Set());
    setEditingId(null);
  };

  // ─── Artifact card handlers ────────────────────────────────────────────
  const toggleExpand = (id) => {
    setExpandedIds((prev) => {
      const next = new Set(prev);
      if (next.has(id)) {
        next.delete(id);
        // Cancel edit if collapsing the artifact being edited
        if (editingId === id) setEditingId(null);
      } else {
        next.add(id);
      }
      return next;
    });
  };
  const startEdit = (id) => setEditingId(id);
  const commitEdit = (id, newContent) => {
    setArtifacts((prev) => ({
      ...prev,
      [id]: { ...prev[id], content: newContent },
    }));
    setEditingId(null);
  };
  const cancelEdit = () => setEditingId(null);

  // ─── Download all artifacts as a single bundle .md ─────────────────────
  const downloadBundle = () => {
    const lines = [];
    lines.push('# Golden Path Composition');
    lines.push('');
    lines.push(`> Generated by The Composer · ${new Date().toISOString().slice(0, 10)}`);
    lines.push('>');
    lines.push('> This bundle contains 8 files. Save each section under the indicated filename.');
    lines.push('');
    lines.push('---');
    lines.push('');

    for (const a of ARTIFACTS) {
      const state = artifacts[a.id];
      lines.push(`## ${a.file}`);
      lines.push('');
      if (state.status === DRAFTED && state.content) {
        // Choose fence language by extension
        const ext = a.file.split('.').pop();
        const lang =
          ext === 'json' ? 'json' :
          ext === 'sh'   ? 'bash' :
          ext === 'md'   ? 'md'   : '';
        lines.push('```' + lang);
        lines.push(state.content);
        lines.push('```');
      } else {
        lines.push('_(not drafted)_');
      }
      lines.push('');
      lines.push('---');
      lines.push('');
    }

    const blob = new Blob([lines.join('\n')], { type: 'text/markdown;charset=utf-8' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = 'golden-path-composition.md';
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    setTimeout(() => URL.revokeObjectURL(url), 1000);
  };

  const empty = messages.length === 0;

  return (
    <>
      <style>{`
        @import url('https://fonts.googleapis.com/css2?family=Fraunces:ital,opsz,wght@0,9..144,300..700;1,9..144,300..700&family=Martian+Mono:wght@300;400;500;600&display=swap');

        @keyframes msgIn {
          from { opacity: 0; transform: translateY(8px); }
          to   { opacity: 1; transform: translateY(0); }
        }
        @keyframes dotPulse {
          0%, 100% { opacity: 0.3; transform: scale(1); }
          50%      { opacity: 1;   transform: scale(1.25); }
        }
        @keyframes dotPop {
          0%   { transform: scale(1); }
          50%  { transform: scale(1.6); }
          100% { transform: scale(1); }
        }
        @keyframes pageIn {
          from { opacity: 0; transform: translateY(12px); }
          to   { opacity: 1; transform: translateY(0); }
        }

        body { background: ${C.paper3}; }

        textarea::placeholder { color: ${C.ink4}; }
        textarea:focus { outline: none; }

        /* Custom scrollbar */
        .scroll-region::-webkit-scrollbar { width: 6px; }
        .scroll-region::-webkit-scrollbar-track { background: transparent; }
        .scroll-region::-webkit-scrollbar-thumb {
          background: ${C.hairline};
          border-radius: 999px;
        }
        .scroll-region::-webkit-scrollbar-thumb:hover { background: ${C.ink4}; }

        /* Artifact card edit affordance */
        .artifact-pre:hover {
          border-color: ${C.ink3} !important;
          background: ${C.paper2} !important;
        }
        .artifact-pre:hover + .edit-hint,
        div:hover > .edit-hint {
          opacity: 1 !important;
        }
      `}</style>

      <div
        style={{
          minHeight: '100vh',
          width: '100%',
          background: C.paper3,
          fontFamily: SERIF,
          padding: '40px 32px',
          display: 'flex',
          justifyContent: 'center',
        }}
      >
        <div
          style={{
            width: '100%',
            maxWidth: 1100,
            background: C.paper,
            border: `1px solid ${C.hairline}`,
            borderRadius: 4,
            boxShadow: '0 1px 0 rgba(24,24,27,0.04), 0 12px 48px rgba(24,24,27,0.06)',
            display: 'flex',
            flexDirection: 'column',
            minHeight: 'calc(100vh - 80px)',
            animation: 'pageIn 600ms cubic-bezier(0.16, 1, 0.3, 1)',
          }}
        >
          {/* HEADER */}
          <header
            style={{
              padding: '28px 36px 20px',
              borderBottom: `1px solid ${C.hairline}`,
              display: 'flex',
              alignItems: 'flex-end',
              justifyContent: 'space-between',
              gap: 24,
            }}
          >
            <div>
              <MonoLabel>Plate III · The Conversational Composer</MonoLabel>
              <h1
                style={{
                  fontFamily: SERIF,
                  fontStyle: 'italic',
                  fontSize: 32,
                  fontWeight: 400,
                  color: C.ink,
                  margin: '8px 0 4px',
                  lineHeight: 1.05,
                  letterSpacing: '-0.01em',
                }}
              >
                Compose by talking it through.
              </h1>
              <p
                style={{
                  fontFamily: SERIF,
                  fontStyle: 'italic',
                  fontSize: 14,
                  color: C.ink3,
                  margin: 0,
                  lineHeight: 1.4,
                }}
              >
                Same five layers. Same eight artifacts. The form has been replaced with a conversation.
              </p>
            </div>

            <div style={{ display: 'flex', alignItems: 'center', gap: 16 }}>
              <span
                style={{
                  fontFamily: MONO,
                  fontSize: 10,
                  letterSpacing: '0.18em',
                  textTransform: 'uppercase',
                  color: C.ink3,
                  fontVariantNumeric: 'tabular-nums',
                }}
              >
                {String(draftedCount).padStart(2, '0')} / {String(totalArtifacts).padStart(2, '0')} drafted
              </span>
              <button
                onClick={reset}
                style={{
                  fontFamily: MONO,
                  fontSize: 10,
                  letterSpacing: '0.16em',
                  textTransform: 'uppercase',
                  color: C.ink3,
                  background: 'transparent',
                  border: 'none',
                  cursor: 'pointer',
                  padding: '4px 8px',
                  textDecoration: 'underline',
                  textUnderlineOffset: 3,
                  textDecorationColor: C.hairline,
                }}
                onMouseEnter={(e) => {
                  e.currentTarget.style.color = C.ink;
                  e.currentTarget.style.textDecorationColor = C.ink;
                }}
                onMouseLeave={(e) => {
                  e.currentTarget.style.color = C.ink3;
                  e.currentTarget.style.textDecorationColor = C.hairline;
                }}
              >
                Reset
              </button>
            </div>
          </header>

          {/* TWO-COLUMN BODY */}
          <div
            style={{
              display: 'grid',
              gridTemplateColumns: '1fr 380px',
              flex: 1,
              minHeight: 0,
            }}
          >
            {/* LEFT — TRANSCRIPT */}
            <section
              style={{
                display: 'flex',
                flexDirection: 'column',
                borderRight: `1px solid ${C.hairline}`,
                minWidth: 0,
              }}
            >
              <div
                ref={transcriptRef}
                className="scroll-region"
                style={{
                  flex: 1,
                  overflowY: 'auto',
                  padding: '0 36px',
                }}
              >
                {empty ? (
                  <div
                    style={{
                      paddingTop: 80,
                      paddingBottom: 40,
                      maxWidth: 480,
                    }}
                  >
                    <MonoLabel color={C.ink2}>
                      {foundationReady ? 'Foundation set' : 'Begin'}
                    </MonoLabel>
                    <p
                      style={{
                        fontFamily: SERIF,
                        fontStyle: 'italic',
                        fontSize: 22,
                        lineHeight: 1.4,
                        color: C.ink,
                        margin: '12px 0 16px',
                        fontWeight: 400,
                      }}
                    >
                      {foundationReady
                        ? `${appName.trim()} is ready to compose.`
                        : 'Name your app to start.'}
                    </p>
                    <p
                      style={{
                        fontFamily: SERIF,
                        fontSize: 14,
                        lineHeight: 1.55,
                        color: C.ink2,
                        margin: 0,
                      }}
                    >
                      {foundationReady
                        ? 'Click "Begin composing" in the live build panel to wire the agent. From there I\'ll guide you through MCP servers, design system, architecture, and implementation plan — each step drafts the relevant artifact instantly.'
                        : 'Set the app name, pick your platforms, and write a one-line description in the live build panel. I\'ll draft README, CLAUDE, and scaffold.sh as you type — then we\'ll handle the rest in conversation.'}
                    </p>
                  </div>
                ) : (
                  <>
                    {messages.map((m, idx) => {
                      const isLatestComposer =
                        m.role === 'COMPOSER' && idx === latestComposerIdx;
                      const isOlderComposer =
                        m.role === 'COMPOSER' && !isLatestComposer;

                      // Older composer turns collapse to a compact summary
                      if (isOlderComposer) {
                        return (
                          <CompactComposerTurn
                            key={m.id}
                            layer={m.layer}
                            appliedLabel={m.appliedLabel || (m.layer === 'description' ? appDescription : null)}
                            onEdit={() => goBackToMessage(idx)}
                          />
                        );
                      }

                      return (
                        <MessageBlock
                          key={m.id}
                          role={m.role}
                          body={m.body}
                          callouts={m.callouts}
                          post={m.post}
                          reasoning={reasonings[m.layer] || m.reasoning}
                          suggestion={m.suggestion}
                          alternatives={m.alternatives}
                          freeTextHint={m.freeTextHint}
                          timestamp={m.timestamp}
                          justAdded={m.id === justAddedId}
                          alternativesOpen={isLatestComposer && alternativesOpen}
                          interactive={isLatestComposer && !isThinking}
                          onApply={() => isLatestComposer && handleApply(m)}
                          onPickAlternative={(alt) => isLatestComposer && handlePickAlternative(alt)}
                          onToggleAlternatives={() => isLatestComposer && setAlternativesOpen((v) => !v)}
                          assetInputs={isLatestComposer && m.assetInputs}
                          assetValues={designAssets}
                          onAssetChange={(key, value) =>
                            setDesignAssets((prev) => ({ ...prev, [key]: value }))
                          }
                          onApplyAssets={() => isLatestComposer && handleApplyAssets(m)}
                        />
                      );
                    })}
                    {isThinking && <ThinkingIndicator />}
                  </>
                )}
              </div>

              <div style={{ padding: '0 36px 28px' }}>
                {errorMsg && (
                  <div
                    style={{
                      marginBottom: 12,
                      padding: '10px 14px',
                      border: `1px solid ${C.hairline}`,
                      borderLeft: `3px solid ${C.ink}`,
                      borderRadius: 3,
                      background: C.paper2,
                      display: 'flex',
                      alignItems: 'center',
                      gap: 10,
                    }}
                  >
                    <span
                      style={{
                        flex: 1,
                        fontFamily: SERIF,
                        fontStyle: 'italic',
                        fontSize: 13,
                        color: C.ink2,
                      }}
                    >
                      {errorMsg}
                    </span>
                    <button
                      onClick={() => setErrorMsg(null)}
                      aria-label="Dismiss"
                      style={{
                        fontFamily: MONO,
                        fontSize: 9,
                        letterSpacing: '0.18em',
                        textTransform: 'uppercase',
                        color: C.ink3,
                        background: 'transparent',
                        border: 'none',
                        cursor: 'pointer',
                        padding: '4px 8px',
                      }}
                    >
                      Dismiss
                    </button>
                  </div>
                )}
                <InputBox
                  value={inputValue}
                  onChange={setInputValue}
                  onSubmit={submit}
                  disabled={isThinking}
                  suggestions={null}
                  onSuggestion={(s) => setInputValue(s)}
                />
              </div>
            </section>

            {/* RIGHT — LIVE BUILD */}
            <aside
              className="scroll-region"
              style={{
                background: C.paper2,
                padding: '28px 28px',
                overflowY: 'auto',
                minWidth: 0,
              }}
            >
              <MonoLabel>Live build</MonoLabel>
              <p
                style={{
                  fontFamily: SERIF,
                  fontStyle: 'italic',
                  fontSize: 13,
                  color: C.ink3,
                  margin: '6px 0 18px',
                  lineHeight: 1.4,
                }}
              >
                Foundation set here, then layers lock as you decide.
              </p>

              {/* ── Foundation setup ───────────────────────────────────── */}
              <div
                style={{
                  paddingBottom: 18,
                  marginBottom: 6,
                  borderBottom: `1px solid ${C.hairline}`,
                }}
              >
                <div
                  style={{
                    fontFamily: MONO,
                    fontSize: 9,
                    letterSpacing: '0.2em',
                    textTransform: 'uppercase',
                    color: C.ink2,
                    marginBottom: 10,
                  }}
                >
                  Foundation
                </div>

                {/* App name (required) */}
                <label
                  style={{
                    display: 'block',
                    fontFamily: MONO,
                    fontSize: 9,
                    letterSpacing: '0.18em',
                    textTransform: 'uppercase',
                    color: C.ink3,
                    marginBottom: 4,
                  }}
                >
                  App name <span style={{ color: C.ink }}>*</span>
                </label>
                <input
                  type="text"
                  value={appName}
                  onChange={(e) => setAppName(e.target.value)}
                  placeholder="MyApp"
                  spellCheck={false}
                  style={{
                    width: '100%',
                    padding: '8px 10px',
                    fontFamily: SERIF,
                    fontSize: 15,
                    color: C.ink,
                    background: C.paper,
                    border: `1px solid ${C.hairline}`,
                    borderRadius: 3,
                    outline: 'none',
                    boxSizing: 'border-box',
                    transition: 'border-color 180ms ease-out',
                  }}
                  onFocus={(e) => (e.currentTarget.style.borderColor = C.ink)}
                  onBlur={(e) => (e.currentTarget.style.borderColor = C.hairline)}
                />

                {/* Platforms (chip multi-select, mandatory) */}
                <div
                  style={{
                    fontFamily: MONO,
                    fontSize: 9,
                    letterSpacing: '0.18em',
                    textTransform: 'uppercase',
                    color: C.ink3,
                    marginTop: 12,
                    marginBottom: 6,
                  }}
                >
                  Platforms <span style={{ color: C.ink }}>*</span>
                </div>
                <div style={{ display: 'flex', flexWrap: 'wrap', gap: 6 }}>
                  {Object.keys(PLATFORM_FLAGS).map((p) => {
                    const sel = platforms.includes(p);
                    return (
                      <button
                        key={p}
                        aria-label={p}
                        aria-pressed={sel}
                        onClick={() =>
                          setPlatforms((prev) =>
                            prev.includes(p)
                              ? prev.filter((x) => x !== p)
                              : [...prev, p]
                          )
                        }
                        style={{
                          display: 'inline-flex',
                          alignItems: 'center',
                          justifyContent: 'center',
                          height: 28,
                          minWidth: sel ? 28 : 'auto',
                          padding: sel ? 0 : '0 12px',
                          fontFamily: MONO,
                          fontSize: 10,
                          letterSpacing: '0.14em',
                          textTransform: 'uppercase',
                          fontWeight: 500,
                          background: sel ? C.ink : 'transparent',
                          color: sel ? C.paper : C.ink2,
                          border: `1px solid ${sel ? C.ink : C.hairline}`,
                          borderRadius: 999,
                          cursor: 'pointer',
                          overflow: 'hidden',
                          transition:
                            'min-width 360ms cubic-bezier(0.16, 1, 0.3, 1), ' +
                            'padding 360ms cubic-bezier(0.16, 1, 0.3, 1), ' +
                            'background 240ms ease-out, ' +
                            'border-color 240ms ease-out, ' +
                            'color 240ms ease-out',
                        }}
                      >
                        {/* Text label — shrinks to 0 width on select */}
                        <span
                          style={{
                            maxWidth: sel ? 0 : 120,
                            opacity: sel ? 0 : 1,
                            overflow: 'hidden',
                            whiteSpace: 'nowrap',
                            transition:
                              'max-width 360ms cubic-bezier(0.16, 1, 0.3, 1), ' +
                              'opacity 200ms ease-out',
                          }}
                        >
                          {p}
                        </span>
                        {/* Icon — expands from 0 to 14px on select */}
                        <span
                          style={{
                            display: 'inline-flex',
                            alignItems: 'center',
                            justifyContent: 'center',
                            maxWidth: sel ? 14 : 0,
                            opacity: sel ? 1 : 0,
                            transform: sel ? 'scale(1)' : 'scale(0.5)',
                            overflow: 'hidden',
                            transition:
                              'max-width 360ms cubic-bezier(0.16, 1, 0.3, 1), ' +
                              'opacity 240ms ease-out 100ms, ' +
                              'transform 320ms cubic-bezier(0.16, 1, 0.3, 1) 100ms',
                          }}
                        >
                          <PlatformIcon platform={p} />
                        </span>
                      </button>
                    );
                  })}
                </div>

                {/* .NET Runtime (single-select) */}
                <div
                  style={{
                    fontFamily: MONO,
                    fontSize: 9,
                    letterSpacing: '0.18em',
                    textTransform: 'uppercase',
                    color: C.ink3,
                    marginTop: 14,
                    marginBottom: 6,
                  }}
                >
                  .NET runtime <span style={{ color: C.ink }}>*</span>
                </div>
                <div style={{ display: 'flex', gap: 6 }}>
                  {['.NET 10', '.NET 9'].map((r) => {
                    const sel = runtime === r;
                    const isRecommended = r === '.NET 10';
                    return (
                      <button
                        key={r}
                        type="button"
                        aria-pressed={sel}
                        onClick={() => setRuntime(r)}
                        style={{
                          fontFamily: MONO,
                          fontSize: 10,
                          letterSpacing: '0.14em',
                          textTransform: 'uppercase',
                          fontWeight: 500,
                          padding: '6px 12px',
                          background: sel ? C.ink : 'transparent',
                          color: sel ? C.paper : C.ink2,
                          border: `1px solid ${sel ? C.ink : C.hairline}`,
                          borderRadius: 999,
                          cursor: 'pointer',
                          display: 'inline-flex',
                          alignItems: 'center',
                          gap: 6,
                          transition: 'all 200ms ease-out',
                        }}
                      >
                        {r}
                        {isRecommended && (
                          <span
                            style={{
                              fontSize: 8,
                              letterSpacing: '0.18em',
                              opacity: sel ? 0.7 : 0.55,
                              fontWeight: 400,
                            }}
                          >
                            REC
                          </span>
                        )}
                      </button>
                    );
                  })}
                </div>

                {/* Begin button — appears once foundation is ready and chat is empty */}
                {foundationReady && messages.length === 0 && (
                  <button
                    onClick={beginConversation}
                    style={{
                      marginTop: 14,
                      fontFamily: MONO,
                      fontSize: 10,
                      letterSpacing: '0.18em',
                      textTransform: 'uppercase',
                      fontWeight: 500,
                      padding: '8px 14px',
                      background: C.ink,
                      color: C.paper,
                      border: `1px solid ${C.ink}`,
                      borderRadius: 999,
                      cursor: 'pointer',
                      display: 'inline-flex',
                      alignItems: 'center',
                      gap: 6,
                    }}
                  >
                    Begin composing
                    <span style={{ display: 'inline-block', transform: 'translateY(-1px)' }}>→</span>
                  </button>
                )}
              </div>

              {/* ── Artifacts ──────────────────────────────────────────── */}
              <div
                style={{
                  fontFamily: MONO,
                  fontSize: 9,
                  letterSpacing: '0.2em',
                  textTransform: 'uppercase',
                  color: C.ink2,
                  marginTop: 14,
                  marginBottom: 4,
                }}
              >
                Artifacts
              </div>

              <div>
                {ARTIFACTS.map((artifact) => (
                  <ArtifactCard
                    key={artifact.id}
                    artifact={artifact}
                    state={artifacts[artifact.id]}
                    expanded={expandedIds.has(artifact.id)}
                    editing={editingId === artifact.id}
                    onToggleExpand={() => toggleExpand(artifact.id)}
                    onStartEdit={() => startEdit(artifact.id)}
                    onCommitEdit={(newContent) => commitEdit(artifact.id, newContent)}
                    onCancelEdit={cancelEdit}
                  />
                ))}
              </div>

              {/* Final action */}
              <div
                style={{
                  marginTop: 20,
                  paddingTop: 20,
                  borderTop: `1px solid ${C.hairline}`,
                  display: 'flex',
                  flexDirection: 'column',
                  gap: 10,
                }}
              >
                <button
                  onClick={downloadBundle}
                  disabled={draftedCount === 0}
                  style={{
                    fontFamily: MONO,
                    fontSize: 10,
                    letterSpacing: '0.2em',
                    textTransform: 'uppercase',
                    fontWeight: 500,
                    padding: '12px 16px',
                    background: draftedCount === 0 ? C.paper3 : C.ink,
                    color: draftedCount === 0 ? C.ink4 : C.paper,
                    border: `1px solid ${draftedCount === 0 ? C.hairline : C.ink}`,
                    borderRadius: 999,
                    cursor: draftedCount === 0 ? 'not-allowed' : 'pointer',
                    transition: 'all 220ms ease-out',
                    display: 'inline-flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    gap: 8,
                  }}
                >
                  <svg width="12" height="12" viewBox="0 0 12 12" aria-hidden>
                    <path d="M 6 1 L 6 8 M 3 5.5 L 6 8 L 9 5.5 M 2 10 L 10 10"
                      fill="none" stroke="currentColor" strokeWidth="1.2"
                      strokeLinecap="round" strokeLinejoin="round" />
                  </svg>
                  Download bundle
                </button>
                <span
                  style={{
                    fontFamily: SERIF,
                    fontStyle: 'italic',
                    fontSize: 12,
                    color: C.ink4,
                    lineHeight: 1.4,
                  }}
                >
                  {draftedCount === 0
                    ? `Available once at least one artifact has drafted.`
                    : draftedCount < totalArtifacts
                      ? `${draftedCount} of ${totalArtifacts} artifacts so far. Download anytime, or finish the composition first.`
                      : `All eight artifacts ready. Download as a single Markdown file.`}
                </span>
              </div>
            </aside>
          </div>
        </div>
      </div>
    </>
  );
}
