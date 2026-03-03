# MSN Messenger 2026 – Mobile App Design Brief

## Executive Summary

A nostalgic reimagining of MSN Messenger for modern mobile platforms. The app preserves the **contact-centric** UX of classic MSN (buddy list as home, editable display names, custom groups, personal messages) while applying contemporary mobile design patterns (touch-first, fluid animations, glassmorphism).

**Core Philosophy:** This is NOT a chat-centric app like WhatsApp/iMessage. The buddy list IS the app. Conversations are secondary.

---

## Table of Contents

1. [Design System](#1-design-system)
2. [App Architecture & Navigation](#2-app-architecture--navigation)
3. [Screen Specifications](#3-screen-specifications)
4. [Component Library](#4-component-library)
5. [Animation Specifications](#5-animation-specifications)
6. [State Management](#6-state-management)
7. [Interaction Patterns](#7-interaction-patterns)
8. [Accessibility Requirements](#8-accessibility-requirements)

---

## 1. Design System

### 1.1 Color Tokens

#### Light Mode
```
┌─────────────────────────────────────────────────────────────┐
│ BRAND COLORS (Heritage MSN Palette)                         │
├─────────────────────────────────────────────────────────────┤
│ primary            │ #00B377  │ MSN signature green         │
│ primary-light      │ #33CC99  │ Lighter variant             │
│ primary-dark       │ #009966  │ Darker variant              │
│ secondary          │ #0078D4  │ MSN blue                    │
│ secondary-light    │ #3399FF  │ Lighter blue                │
│ accent             │ #FF6B00  │ Orange (notifications/CTA)  │
│ purple             │ #9B59B6  │ Fun accent                  │
│ pink               │ #E91E8C  │ Wink/nudge accent           │
├─────────────────────────────────────────────────────────────┤
│ PRESENCE STATES                                             │
├─────────────────────────────────────────────────────────────┤
│ online             │ #00CC66  │ Bright green + glow effect  │
│ away               │ #FFB800  │ Amber/yellow                │
│ busy               │ #FF3B30  │ Red                         │
│ offline            │ #8E8E93  │ Gray                        │
├─────────────────────────────────────────────────────────────┤
│ BACKGROUNDS                                                 │
├─────────────────────────────────────────────────────────────┤
│ bg                 │ #E8EEF4  │ App background              │
│ bg-secondary       │ #FFFFFF  │ Card/surface background     │
│ bg-tertiary        │ #F0F4F8  │ Input fields, subtle fills  │
│ bg-glass           │ rgba(255,255,255,0.95) │ Blur overlays │
├─────────────────────────────────────────────────────────────┤
│ TEXT                                                        │
├─────────────────────────────────────────────────────────────┤
│ text               │ #1A1A2E  │ Primary text                │
│ text-secondary     │ #5A6A7A  │ Secondary/body text         │
│ text-muted         │ #8A9AAA  │ Hints, timestamps           │
├─────────────────────────────────────────────────────────────┤
│ BORDERS                                                     │
├─────────────────────────────────────────────────────────────┤
│ border             │ #D0DAE4  │ Card borders                │
│ border-light       │ #E8EEF4  │ Dividers within cards       │
├─────────────────────────────────────────────────────────────┤
│ CHAT BUBBLES                                                │
├─────────────────────────────────────────────────────────────┤
│ bubble-sent        │ linear-gradient(135deg, #00B377, #00A5E0) │
│ bubble-sent-text   │ #FFFFFF                                   │
│ bubble-received    │ #FFFFFF                                   │
│ bubble-received-text │ #1A1A2E                                 │
└─────────────────────────────────────────────────────────────┘
```

#### Dark Mode
```
┌─────────────────────────────────────────────────────────────┐
│ BRAND COLORS                                                │
├─────────────────────────────────────────────────────────────┤
│ primary            │ #00DD99  │ Brighter green for dark bg  │
│ primary-light      │ #33FFBB  │                             │
│ primary-dark       │ #00B377  │                             │
│ secondary          │ #3399FF  │                             │
│ secondary-light    │ #66B3FF  │                             │
│ accent             │ #FF8833  │                             │
│ purple             │ #BB77DD  │                             │
│ pink               │ #FF4499  │                             │
├─────────────────────────────────────────────────────────────┤
│ PRESENCE STATES                                             │
├─────────────────────────────────────────────────────────────┤
│ online             │ #00FF7F  │ Brighter for contrast       │
│ away               │ #FFCC00  │                             │
│ busy               │ #FF5555  │                             │
│ offline            │ #666677  │                             │
├─────────────────────────────────────────────────────────────┤
│ BACKGROUNDS                                                 │
├─────────────────────────────────────────────────────────────┤
│ bg                 │ #0D1117  │ Deep navy-black             │
│ bg-secondary       │ #161B22  │ Elevated surfaces           │
│ bg-tertiary        │ #21262D  │ Input fields                │
│ bg-glass           │ rgba(13,17,23,0.95) │                  │
├─────────────────────────────────────────────────────────────┤
│ TEXT                                                        │
├─────────────────────────────────────────────────────────────┤
│ text               │ #E6EDF3  │                             │
│ text-secondary     │ #8B949E  │                             │
│ text-muted         │ #6E7681  │                             │
├─────────────────────────────────────────────────────────────┤
│ BORDERS                                                     │
├─────────────────────────────────────────────────────────────┤
│ border             │ #30363D  │                             │
│ border-light       │ #21262D  │                             │
├─────────────────────────────────────────────────────────────┤
│ CHAT BUBBLES                                                │
├─────────────────────────────────────────────────────────────┤
│ bubble-received    │ #21262D  │                             │
│ bubble-received-text │ #E6EDF3 │                            │
└─────────────────────────────────────────────────────────────┘
```

#### Gradient Definitions
```
┌─────────────────────────────────────────────────────────────┐
│ SIGNATURE GRADIENTS                                         │
├─────────────────────────────────────────────────────────────┤
│ header-gradient    │ 135deg: primary → #00A5E0 → secondary  │
│ cta-gradient       │ 135deg: primary → secondary            │
│ nudge-gradient     │ 135deg: accent → pink                  │
│ avatar-frame       │ 135deg: [custom color] → secondary     │
└─────────────────────────────────────────────────────────────┘
```

### 1.2 Typography Scale

```
┌────────────────┬──────────┬────────┬─────────────┬───────────────────────┐
│ Style Name     │ Size     │ Weight │ Line Height │ Usage                 │
├────────────────┼──────────┼────────┼─────────────┼───────────────────────┤
│ display-lg     │ 32px     │ 800    │ 1.2         │ Onboarding titles     │
│ display-md     │ 26px     │ 700    │ 1.25        │ Section headings      │
│ display-sm     │ 20-22px  │ 700    │ 1.3         │ Screen titles         │
│ body-lg        │ 16-17px  │ 600-700│ 1.4         │ Display names, CTAs   │
│ body-md        │ 15px     │ 400-600│ 1.5         │ Message text, content │
│ body-sm        │ 13-14px  │ 400-500│ 1.5         │ Secondary content     │
│ caption        │ 12px     │ 600-700│ 1.4         │ Group headers, labels │
│ micro          │ 10-11px  │ 500-600│ 1.3         │ Timestamps, badges    │
└────────────────┴──────────┴────────┴─────────────┴───────────────────────┘

Font Stack: SF Pro Display, -apple-system, BlinkMacSystemFont, Segoe UI, sans-serif
```

### 1.3 Spacing Scale

```
Base unit: 4px

┌─────────┬───────┬────────────────────────────────────┐
│ Token   │ Value │ Usage                              │
├─────────┼───────┼────────────────────────────────────┤
│ space-1 │ 4px   │ Tight gaps (icon-text)             │
│ space-2 │ 8px   │ Related element spacing            │
│ space-3 │ 12px  │ Component internal padding         │
│ space-4 │ 16px  │ Section padding, card margins      │
│ space-5 │ 20px  │ Large internal spacing             │
│ space-6 │ 24px  │ Screen edge padding                │
│ space-8 │ 32px  │ Section separation                 │
│ space-10│ 40px  │ Major section breaks               │
└─────────┴───────┴────────────────────────────────────┘
```

### 1.4 Border Radius Scale

```
┌──────────────┬───────┬────────────────────────────────┐
│ Token        │ Value │ Usage                          │
├──────────────┼───────┼────────────────────────────────┤
│ radius-sm    │ 6-8px │ Small buttons, tags            │
│ radius-md    │ 10-12px│ Cards, inputs, buttons        │
│ radius-lg    │ 16px  │ Large cards, modals            │
│ radius-xl    │ 20-24px│ Chat bubbles, pills           │
│ radius-full  │ 50%   │ Avatars, status badges         │
│ radius-avatar│ 25-28%│ Avatar squares with rounding   │
└──────────────┴───────┴────────────────────────────────┘
```

### 1.5 Shadow Definitions

```
┌──────────────┬────────────────────────────────────────────┐
│ Token        │ Value                                      │
├──────────────┼────────────────────────────────────────────┤
│ shadow-sm    │ 0 1px 3px rgba(0,0,0,0.06)                 │
│ shadow-md    │ 0 2px 8px rgba(0,0,0,0.08)                 │
│ shadow-lg    │ 0 4px 20px rgba(0,0,0,0.12)                │
│ shadow-glow  │ 0 0 8px [presence-color]                   │
│ shadow-button│ 0 4px 12px [color]40 (40% opacity)         │
└──────────────┴────────────────────────────────────────────┘
```

---

## 2. App Architecture & Navigation

### 2.1 Screen Hierarchy

```
┌─────────────────────────────────────────────────────────────┐
│                      APP STRUCTURE                          │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌─────────────┐                                            │
│  │ Onboarding  │ ──→ Sign In ──→ Main App                   │
│  │ (3 steps)   │                                            │
│  └─────────────┘                                            │
│                                                             │
│  ┌─────────────────────────────────────────────────────┐    │
│  │                    MAIN APP                          │    │
│  │  ┌──────────────────────────────────────────────┐   │    │
│  │  │              TAB BAR NAVIGATION               │   │    │
│  │  ├──────────────────────────────────────────────┤   │    │
│  │  │                                              │   │    │
│  │  │  👥 Buddies    💬 Chats       😊 Me          │   │    │
│  │  │  (DEFAULT)    (Secondary)   (Profile)       │   │    │
│  │  │                                              │   │    │
│  │  └──────────────────────────────────────────────┘   │    │
│  │                         │                           │    │
│  │           ┌─────────────┼─────────────┐             │    │
│  │           ▼             ▼             ▼             │    │
│  │    ┌───────────┐ ┌───────────┐ ┌───────────┐       │    │
│  │    │  Buddy    │ │  Recent   │ │  Profile  │       │    │
│  │    │   List    │ │   Chats   │ │  Editor   │       │    │
│  │    └─────┬─────┘ └─────┬─────┘ └─────┬─────┘       │    │
│  │          │             │             │              │    │
│  │          ▼             ▼             ▼              │    │
│  │    ┌───────────┐ ┌───────────┐ ┌───────────┐       │    │
│  │    │   Chat    │ │   Chat    │ │ Settings  │       │    │
│  │    │   View    │ │   View    │ │           │       │    │
│  │    └───────────┘ └───────────┘ └───────────┘       │    │
│  └─────────────────────────────────────────────────────┘    │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

### 2.2 Navigation Behavior

| From | To | Transition | Trigger |
|------|-----|------------|---------|
| Onboarding → Main | Fade + Scale | Complete onboarding or sign in |
| Tab → Tab | None (instant swap) | Tap tab bar item |
| Buddy List → Chat | Slide from right | Tap any contact |
| Recent Chats → Chat | Slide from right | Tap any chat |
| Profile → Settings | Slide from right | Tap settings icon |
| Chat → Previous | Slide to right | Tap back button or swipe |
| Settings → Profile | Slide to right | Tap back button |

### 2.3 Tab Bar Specification

```
┌─────────────────────────────────────────────────────────────┐
│ POSITION: Fixed bottom                                      │
│ HEIGHT: ~75px (including safe area)                         │
│ BACKGROUND: bg-glass with 20px blur                         │
│ BORDER-TOP: 1px solid border                                │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│   ┌─────────┐      ┌─────────┐      ┌─────────┐            │
│   │   👥    │      │   💬    │      │   😊    │            │
│   │ Buddies │      │  Chats  │      │   Me    │            │
│   └─────────┘      └────●────┘      └─────────┘            │
│                         │                                   │
│                    Badge (unread count)                     │
│                                                             │
├─────────────────────────────────────────────────────────────┤
│ STATES:                                                     │
│ • Inactive: icon + muted text                               │
│ • Active: icon + primary text + subtle bg highlight         │
│ • Badge: gradient pill (accent→pink), white text            │
│                                                             │
│ TAP: Scale down to 0.9, then bounce back                    │
└─────────────────────────────────────────────────────────────┘
```

---

## 3. Screen Specifications

### 3.1 Onboarding Flow (3 Steps)

#### Layout Structure
```
┌─────────────────────────────────────────┐
│ ○ ○ ○  Step Indicators (top center)     │
├─────────────────────────────────────────┤
│                                         │
│         [Animated Content Area]         │
│                                         │
│              Icon/Logo                  │
│              Title                      │
│              Subtitle                   │
│                                         │
├─────────────────────────────────────────┤
│         [Primary Button]                │
│         [Secondary Button] (step 3)     │
└─────────────────────────────────────────┘
```

#### Step Content

| Step | Visual | Title | Subtitle |
|------|--------|-------|----------|
| 1 | Animated butterfly logo (scale + rotate in) | "MSN Messenger" | "Your buddy list, reimagined ✨" |
| 2 | ✏️ emoji + example display name | "Be Whoever You Want" | "Change your display name anytime. Add song lyrics, quotes, or ~*fancy text*~" |
| 3 | Nudge button + wink emojis | "Nudges & Winks" | "The playful features you remember are back" |

#### Background
- Full gradient background (header-gradient)
- 5-6 floating translucent circles (10% white)
- Circles animate: gentle vertical bob + slight horizontal sway
- Staggered animation delays (0.3s between each)

#### Step Indicators
- 3 horizontal pills
- Inactive: 10px wide, 40% white
- Active: 28px wide, solid white
- Animate width change (200ms ease)

#### Buttons (Step 3)
- Primary: White bg, primary text, "🦋 Sign in with Microsoft"
- Secondary: 15% white bg, 30% white border, white text, "Create Account"

---

### 3.2 Buddy List Screen (HOME - Default Tab)

This is the **primary screen** of the app. Not chats.

#### Layout Structure
```
┌─────────────────────────────────────────┐
│ ┌─────────────────────────────────────┐ │
│ │         GRADIENT HEADER             │ │
│ │  🦋  "Signed in as"                 │ │
│ │      ~*Display Name*~ ✏️   ● Online │ │
│ │  ┌─────────────────────────────┐    │ │
│ │  │ "Personal message here..."  │    │ │
│ │  └─────────────────────────────┘    │ │
│ └─────────────────────────────────────┘ │
│ ┌─────────────────────────────────────┐ │
│ │ ● 5/12 Online        [+ Add Group] │ │  ← Overlaps header
│ └─────────────────────────────────────┘ │
│                                         │
│ ┌─────────────────────────────────────┐ │
│ │ ▶ ⭐ Best Friends            (2/2) │ │  ← Collapsible
│ │ ├─────────────────────────────────┤ │
│ │ │ [Avatar] ~Sarah~ ♪        ● Onl │ │
│ │ │          "🎵 Listening to..."   │ │
│ │ ├─────────────────────────────────┤ │
│ │ │ [Avatar] Mike 🎮          ● Onl │ │
│ │ │          "Playing Valorant"     │ │
│ │ └─────────────────────────────────┘ │
│ └─────────────────────────────────────┘ │
│                                         │
│ ┌─────────────────────────────────────┐ │
│ │ ▶ 🎮 Gaming Crew             (2/3) │ │
│ │ └─ [Collapsed or expanded...]      │ │
│ └─────────────────────────────────────┘ │
│                                         │
│ ┌─────────────────────────────────────┐ │
│ │ ▶ 👨‍👩‍👧 Family                  (1/2) │ │
│ └─────────────────────────────────────┘ │
│                                         │
├─────────────────────────────────────────┤
│ [          TAB BAR                    ] │
└─────────────────────────────────────────┘
```

#### Header Section
- **Background:** Full header-gradient
- **Decorative elements:** 2-3 translucent circles (absolute positioned)
- **Content:**
  - Butterfly logo (32px, animated flutter)
  - "Signed in as" label (11px, uppercase, 80% white)
  - Display name (17px, bold, white) + ✏️ edit icon
  - Status badge (right aligned)
  - Personal message (if set): 15% white bg pill, italic text

#### Online Count Bar
- **Position:** Overlaps header by ~16px (negative margin)
- **Background:** bg-secondary with shadow-md
- **Content:**
  - Pulsing green dot + "X / Y Online" text
  - "+ Add Group" button (gradient, right aligned)

#### Contact Groups (Custom, User-Created)
Each group is a collapsible card:

```
┌─────────────────────────────────────────────────────────────┐
│ GROUP CARD COMPONENT                                        │
├─────────────────────────────────────────────────────────────┤
│ HEADER (always visible):                                    │
│ • Collapse arrow (▶/▼) - rotates 90° on toggle             │
│ • Group name with emoji (user-defined)                      │
│ • Online count badge: "2/5" format                          │
│                                                             │
│ COLLAPSED STATE:                                            │
│ • Header only, no border-bottom                             │
│                                                             │
│ EXPANDED STATE:                                             │
│ • Shows all contacts in group                               │
│ • Contacts sorted: online → away → busy → offline           │
│ • Offline contacts shown at 50% opacity                     │
│                                                             │
│ DEFAULT EXPAND LOGIC:                                       │
│ • Auto-expand if group has ≥1 online contact                │
│ • Auto-collapse if all contacts offline                     │
└─────────────────────────────────────────────────────────────┘
```

#### Contact Row
```
┌─────────────────────────────────────────────────────────────┐
│ [Avatar+Frame+Status]  Display Name              │         │
│         44px           "Personal message..."     │         │
│                        (truncate with ...)       │         │
└─────────────────────────────────────────────────────────────┘

Height: ~60px
Padding: 10px 12px
Divider: 1px border-light (except first item)
Tap state: bg-tertiary background
```

---

### 3.3 Recent Chats Screen (Secondary Tab)

**Note:** This is NOT the home screen. It's a secondary view for quick access to recent conversations.

#### Layout Structure
```
┌─────────────────────────────────────────┐
│         GRADIENT HEADER                 │
│  🦋  "Recent Chats"                     │
└─────────────────────────────────────────┘
│                                         │
│ ┌─────────────────────────────────────┐ │
│ │ [Avatar] ~Sarah~ ♪           now   │ │
│ │                               (2)  │ │  ← Unread badge
│ ├─────────────────────────────────────┤ │
│ │ [Avatar] Mike 🎮              5m   │ │
│ │                                    │ │
│ ├─────────────────────────────────────┤ │
│ │ [Avatar] Mom ❤️               2h   │ │
│ │                               (1)  │ │
│ └─────────────────────────────────────┘ │
│                                         │
│     OR if empty:                        │
│                                         │
│           💬                            │
│    "No recent conversations"            │
│    "Select a buddy to start chatting!"  │
│                                         │
└─────────────────────────────────────────┘
```

#### Key Differences from Buddy List
- **NO message previews** – just contact name + timestamp
- **NO grouping** – flat list sorted by recency
- Shows unread badge count
- Simpler row: Avatar, Name, Time, Badge

#### Chat Row
```
Height: ~70px
Content: Avatar (46px) | Name (bold if unread) | Time | Badge
Badge: gradient pill if unread > 0
```

---

### 3.4 Chat Screen

#### Layout Structure
```
┌─────────────────────────────────────────┐
│ ┌─────────────────────────────────────┐ │
│ │ ← [Avatar] Name              📞 📹 │ │  ← Gradient header
│ │           Status/message           │ │
│ └─────────────────────────────────────┘ │
│                                         │
│ ┌─────────────────────────────────────┐ │
│ │                                     │ │
│ │        Message bubbles              │ │
│ │        (scrollable)                 │ │
│ │                                     │ │
│ │              ┌──────────────┐       │ │
│ │              │ Sent message │ ──────│ │
│ │              └──────────────┘       │ │
│ │  ┌──────────────┐                   │ │
│ │──│ Received msg │                   │ │
│ │  └──────────────┘                   │ │
│ │                        2:34 PM      │ │
│ │                                     │ │
│ └─────────────────────────────────────┘ │
│                                         │
│ ┌─────────────────────────────────────┐ │  ← Optional
│ │         WINK PANEL                  │ │
│ └─────────────────────────────────────┘ │
│                                         │
│ ┌─────────────────────────────────────┐ │
│ │ ✨ │ [    Input field    ] 😊│ ➤/👊│ │
│ └─────────────────────────────────────┘ │
└─────────────────────────────────────────┘
```

#### Header
- Gradient background (primary → mid only, shorter than main screens)
- Back button: 15% white bg, rounded
- Avatar (38px) with status
- Contact name (bold, white)
- Status message or "Online"/"Offline" (80% white)
- Call/Video buttons (right)

#### Message Bubbles
```
┌─────────────────────────────────────────────────────────────┐
│ SENT MESSAGES (right aligned)                               │
├─────────────────────────────────────────────────────────────┤
│ Background: bubble-sent gradient                            │
│ Text: white                                                 │
│ Border radius: 18px 18px 4px 18px (small bottom-right)      │
│ Shadow: 0 2px 8px primary@30%                               │
│ Max width: 75%                                              │
├─────────────────────────────────────────────────────────────┤
│ RECEIVED MESSAGES (left aligned)                            │
├─────────────────────────────────────────────────────────────┤
│ Background: bubble-received (solid)                         │
│ Text: bubble-received-text                                  │
│ Border: 1px solid border                                    │
│ Border radius: 18px 18px 18px 4px (small bottom-left)       │
│ Shadow: shadow-sm                                           │
│ Max width: 75%                                              │
├─────────────────────────────────────────────────────────────┤
│ TIMESTAMP                                                   │
├─────────────────────────────────────────────────────────────┤
│ Position: Below bubble, aligned to bubble side              │
│ Size: micro (10px)                                          │
│ Color: text-muted                                           │
│ Margin-top: 3px                                             │
└─────────────────────────────────────────────────────────────┘
```

#### Wink Panel (Expandable)
```
┌─────────────────────────────────────────────────────────────┐
│ "✨ Send a Wink"                                      ✕     │
├─────────────────────────────────────────────────────────────┤
│  😘   🎸   💃   🎉                                          │
│  ❤️‍🔥   😎   🌈   ⚡                                          │
│  🦋   🎵   ☕   🔥                                          │
└─────────────────────────────────────────────────────────────┘

Grid: 4 columns
Cell size: 48x48px
Cell bg: bg-tertiary
Cell radius: 12px
Hover: scale to 1.15
Tap: scale to 0.9
```

#### Input Bar
- Background: bg-secondary
- Border-top: 1px solid border
- Padding: 12px horizontal, 28px bottom (safe area)

```
Components (left to right):
1. Wink toggle button (40px circle)
   - Inactive: bg-tertiary
   - Active: primary solid
   
2. Input container (flex grow)
   - bg-tertiary, rounded-full (24px)
   - Input field + emoji button
   
3. Send OR Nudge button (44px circle)
   - If text entered: Send button (gradient, ➤ icon)
   - If empty: Nudge button (accent→pink gradient, 👊)
```

#### Nudge Behavior
When nudge is triggered:
1. Play vibration (if enabled)
2. Entire screen shakes horizontally
3. Shake pattern: `[0, -15, 15, -15, 15, -10, 10, 0]` over 500ms
4. Optional: Play sound effect

---

### 3.5 Profile Screen (Edit Display Name, Status, Message)

#### Layout Structure
```
┌─────────────────────────────────────────┐
│ ┌─────────────────────────────────────┐ │
│ │         GRADIENT HEADER       ⚙️    │ │
│ │                                     │ │
│ │           [Large Avatar]            │ │
│ │              80-90px                │ │
│ └─────────────────────────────────────┘ │
│                                         │
│ ┌─────────────────────────────────────┐ │  ← Overlaps header
│ │ ✏️ Display Name                     │ │
│ │ ┌─────────────────────────────────┐ │ │
│ │ │ ~*Your Name*~ 🎵           ✏️  │ │ │  ← Tap to edit
│ │ └─────────────────────────────────┘ │ │
│ │ Tip: Add emojis, ~*symbols*~...    │ │
│ └─────────────────────────────────────┘ │
│                                         │
│ ┌─────────────────────────────────────┐ │
│ │ 🎭 Status                           │ │
│ │ ┌─────────────────────────────────┐ │ │
│ │ │ ● Online                    ▼   │ │ │  ← Dropdown
│ │ └─────────────────────────────────┘ │ │
│ │ [Online] [Away] [Busy] [Offline]   │ │  ← Expanded options
│ └─────────────────────────────────────┘ │
│                                         │
│ ┌─────────────────────────────────────┐ │
│ │ 💬 Personal Message                 │ │
│ │ ┌─────────────────────────────────┐ │ │
│ │ │ "🚀 Building something cool" ✏️│ │ │
│ │ └─────────────────────────────────┘ │ │
│ │ [🎵 Music] [🎮 Gaming] [💤 brb]... │ │  ← Preset pills
│ └─────────────────────────────────────┘ │
│                                         │
│ ┌───────┐ ┌───────┐ ┌───────┐          │
│ │  👥   │ │  💬   │ │  👊   │          │  ← Stats
│ │  48   │ │ 1.2k  │ │  89   │          │
│ │Buddies│ │ Msgs  │ │Nudges │          │
│ └───────┘ └───────┘ └───────┘          │
│                                         │
└─────────────────────────────────────────┘
```

#### Display Name Editor
```
┌─────────────────────────────────────────────────────────────┐
│ DISPLAY MODE (default):                                     │
├─────────────────────────────────────────────────────────────┤
│ Dashed border button showing current name                   │
│ Tap anywhere to enter edit mode                             │
│ ✏️ icon on right                                            │
├─────────────────────────────────────────────────────────────┤
│ EDIT MODE:                                                  │
├─────────────────────────────────────────────────────────────┤
│ Input field (auto-focused)                                  │
│ Solid primary border (2px)                                  │
│ "Save" button appears (gradient)                            │
│ Tip text below: encourage creativity                        │
└─────────────────────────────────────────────────────────────┘
```

#### Status Selector
```
Collapsed: Shows current status badge + dropdown arrow
Expanded: Shows all 4 status options as horizontal pills
          Each pill shows colored dot + label
          Selected pill has colored border + tinted bg
```

#### Personal Message Editor
Same pattern as Display Name:
- Display mode: Dashed button with current message (italic) or placeholder
- Edit mode: Input field + Save button + preset quick-select pills

#### Stats Grid
```
3-column grid
Card: bg-secondary, border, radius-md
Content: emoji (20px) → value (18px bold primary) → label (10px muted)
```

---

### 3.6 Settings Screen

#### Layout Structure
```
┌─────────────────────────────────────────┐
│ ┌─────────────────────────────────────┐ │
│ │ ←   "Settings"       GRADIENT HDR   │ │
│ └─────────────────────────────────────┘ │
│                                         │
│ ┌─────────────────────────────────────┐ │
│ │ ☀️  Dark Mode                  [●━] │ │  ← Toggle
│ ├─────────────────────────────────────┤ │
│ │ 🔔  Notifications              [━●] │ │
│ ├─────────────────────────────────────┤ │
│ │ 👊  Nudge Vibration            [━●] │ │
│ ├─────────────────────────────────────┤ │
│ │ 🔊  Message Sounds             [━●] │ │
│ ├─────────────────────────────────────┤ │
│ │ 🎵  Show Now Playing           [━●] │ │
│ ├─────────────────────────────────────┤ │
│ │ 🔒  Privacy                      → │ │  ← Navigation
│ ├─────────────────────────────────────┤ │
│ │ 🚫  Blocked Contacts             → │ │
│ ├─────────────────────────────────────┤ │
│ │ ❓  Help                         → │ │
│ └─────────────────────────────────────┘ │
│                                         │
│              🦋                         │
│        MSN Messenger                    │
│       Version 2026.1.0                  │
└─────────────────────────────────────────┘
```

#### Setting Row
```
Height: ~56px
Padding: 14px
Content: Icon (22px) | Label (15px, flex) | Toggle or Arrow
Divider: 1px border-light between rows
```

#### Toggle Switch
```
Width: 50px
Height: 30px
Track radius: 15px
Knob size: 26px
Knob inset: 2px

OFF state:
- Track: bg-tertiary
- Knob position: left (x: 2px)

ON state:
- Track: gradient (primary → secondary)
- Knob position: right (x: 22px)

Animation: Knob slides with spring physics
```

---

## 4. Component Library

### 4.1 Avatar Component

```
┌─────────────────────────────────────────────────────────────┐
│ AVATAR COMPONENT                                            │
├─────────────────────────────────────────────────────────────┤
│ Props:                                                      │
│ • name: string (for initials fallback)                      │
│ • size: number (default: 44)                                │
│ • status: 'online' | 'away' | 'busy' | 'offline' | null     │
│ • frameColor: color (optional, for custom frame)            │
│ • showStatus: boolean (default: true)                       │
│ • imageUrl: string (optional)                               │
├─────────────────────────────────────────────────────────────┤
│ Structure:                                                  │
│                                                             │
│   ┌─────────────────┐                                       │
│   │ ╔═════════════╗ │  ← Gradient frame (3px)               │
│   │ ║             ║ │                                       │
│   │ ║      A      ║ │  ← Initial or image                   │
│   │ ║             ║ │                                       │
│   │ ╚═════════════╝ │                                       │
│   │              ●──┼──── Status badge (30% of size)        │
│   └─────────────────┘                                       │
│                                                             │
│ Frame: gradient from frameColor (or primary) to secondary   │
│ Avatar radius: 25-28% of size                               │
│ Background: gradient primary@25% to secondary@25%           │
│ Initial: 40% of size, bold, primary color                   │
│                                                             │
│ Status badge:                                               │
│ • Position: bottom-right, slightly outside                  │
│ • Size: 30% of avatar size                                  │
│ • Border: 2.5px solid bg-secondary                          │
│ • Glow: only for 'online' status                            │
└─────────────────────────────────────────────────────────────┘
```

### 4.2 Status Badge Component

```
┌─────────────────────────────────────────────────────────────┐
│ STATUS BADGE COMPONENT                                      │
├─────────────────────────────────────────────────────────────┤
│ Props:                                                      │
│ • status: 'online' | 'away' | 'busy' | 'offline'            │
│ • showLabel: boolean (default: true)                        │
├─────────────────────────────────────────────────────────────┤
│ Render:                                                     │
│                                                             │
│   ● Online    (green dot + "Online" text)                   │
│   ● Away      (amber dot + "Away" text)                     │
│   ● Busy      (red dot + "Busy" text)                       │
│   ● Offline   (gray dot + "Offline" text)                   │
│                                                             │
│ Dot: 10px circle, status color                              │
│ Online dot: pulsing scale animation (1 → 1.2 → 1, 2s loop)  │
│ Online dot: box-shadow glow effect                          │
│ Label: 12px, bold, status color                             │
│ Gap: 5px between dot and label                              │
└─────────────────────────────────────────────────────────────┘
```

### 4.3 Window Card Component

```
┌─────────────────────────────────────────────────────────────┐
│ WINDOW CARD COMPONENT                                       │
├─────────────────────────────────────────────────────────────┤
│ Props:                                                      │
│ • title: string (optional)                                  │
│ • collapsible: boolean (default: false)                     │
│ • defaultOpen: boolean (default: true)                      │
│ • count: string (optional, e.g., "2/5")                     │
│ • onAdd: function (optional, shows + button)                │
├─────────────────────────────────────────────────────────────┤
│ Structure:                                                  │
│                                                             │
│   ┌───────────────────────────────────────┐                 │
│   │ ▶ Title                    (2/5) [+] │  ← Header        │
│   ├───────────────────────────────────────┤                 │
│   │                                       │                 │
│   │         Children content              │  ← Body         │
│   │                                       │                 │
│   └───────────────────────────────────────┘                 │
│                                                             │
│ Card: bg-secondary, border, radius-md (12px), shadow-md     │
│ Header: bg-tertiary, padding 10-12px                        │
│ Header border-bottom: only when expanded                    │
│                                                             │
│ Collapse arrow: 12px, rotates 0° → 90° on expand            │
│ Count badge: bg-bg, radius-sm, micro text                   │
│ Add button: 24px square, primary@20% bg, "+" icon           │
│                                                             │
│ Collapse animation:                                         │
│ • Height: animate from 0 to auto                            │
│ • Opacity: 0 to 1                                           │
│ • Duration: 200ms                                           │
└─────────────────────────────────────────────────────────────┘
```

### 4.4 Gradient Header Component

```
┌─────────────────────────────────────────────────────────────┐
│ GRADIENT HEADER COMPONENT                                   │
├─────────────────────────────────────────────────────────────┤
│ Background: header-gradient (135deg)                        │
│ Padding: 14px 16px 32px (extra bottom for overlap)          │
│ Position: relative                                          │
│ Overflow: hidden                                            │
│                                                             │
│ Decorative circles (absolute positioned):                   │
│ • Circle 1: top-right (-30, -30), 100px, 10% white          │
│ • Circle 2: bottom-center, 60px, 8% white                   │
│                                                             │
│ Children: positioned relative (above circles)               │
└─────────────────────────────────────────────────────────────┘
```

### 4.5 Nudge Button Component

```
┌─────────────────────────────────────────────────────────────┐
│ NUDGE BUTTON COMPONENT                                      │
├─────────────────────────────────────────────────────────────┤
│ Props:                                                      │
│ • onNudge: function                                         │
│ • size: number (default: 44)                                │
├─────────────────────────────────────────────────────────────┤
│ Appearance:                                                 │
│ • Circle: size px                                           │
│ • Background: nudge-gradient (accent → pink)                │
│ • Shadow: 0 4px 12px accent@50%                             │
│ • Icon: 👊 emoji, 45% of size                               │
├─────────────────────────────────────────────────────────────┤
│ Animations:                                                 │
│                                                             │
│ IDLE (looping):                                             │
│ • Rotate wiggle: [0, -8, 8, -8, 8, 0] degrees               │
│ • Duration: 600ms                                           │
│ • Repeat delay: 3 seconds                                   │
│                                                             │
│ HOVER:                                                      │
│ • Scale: 1.1                                                │
│                                                             │
│ TAP:                                                        │
│ • Scale: 0.9 (spring back)                                  │
└─────────────────────────────────────────────────────────────┘
```

### 4.6 Butterfly Logo Component

```
┌─────────────────────────────────────────────────────────────┐
│ BUTTERFLY LOGO COMPONENT                                    │
├─────────────────────────────────────────────────────────────┤
│ Props:                                                      │
│ • size: number (default: 48)                                │
│ • animated: boolean (default: true)                         │
├─────────────────────────────────────────────────────────────┤
│ SVG Structure:                                              │
│                                                             │
│        ○   ○     ← Antenna tips (gradient circles)          │
│         \ /                                                 │
│    ╭─────●─────╮  ← Upper wings (gradient fills)            │
│   ╱             ╲                                           │
│   ╲             ╱                                           │
│    ╰─────●─────╯  ← Lower wings (lighter gradient)          │
│          │                                                  │
│          ●       ← Body (dark ellipse)                      │
│                                                             │
│ Colors:                                                     │
│ • Left wing: gradient #00DD99 → #00A5E0                     │
│ • Right wing: gradient #00A5E0 → #0078D4                    │
│ • Lower wings: #33CC99 → #00B377, 85% opacity               │
│ • Body: text color (dark)                                   │
│ • Antenna tips: primary and secondary                       │
│                                                             │
│ Animation (if animated):                                    │
│ • Gentle rotation: [0, -2, 2, 0] degrees                    │
│ • Duration: 3 seconds, infinite loop                        │
│ • Ease: easeInOut                                           │
└─────────────────────────────────────────────────────────────┘
```

---

## 5. Animation Specifications

### 5.1 Page Transitions

```
┌─────────────────────────────────────────────────────────────┐
│ SLIDE TRANSITION (Push/Pop)                                 │
├─────────────────────────────────────────────────────────────┤
│ Push (entering):                                            │
│ • Initial: opacity 0, translateX(20px)                      │
│ • Final: opacity 1, translateX(0)                           │
│ • Duration: 250ms                                           │
│ • Easing: ease-out                                          │
│                                                             │
│ Pop (exiting):                                              │
│ • Initial: opacity 1, translateX(0)                         │
│ • Final: opacity 0, translateX(-20px)                       │
│ • Duration: 200ms                                           │
│ • Easing: ease-in                                           │
├─────────────────────────────────────────────────────────────┤
│ TAB SWITCH                                                  │
├─────────────────────────────────────────────────────────────┤
│ • Instant swap (no animation between tabs)                  │
│ • Content within tabs uses stagger animations               │
└─────────────────────────────────────────────────────────────┘
```

### 5.2 List Animations

```
┌─────────────────────────────────────────────────────────────┐
│ STAGGER CONTAINER                                           │
├─────────────────────────────────────────────────────────────┤
│ Children stagger delay: 60ms between each                   │
├─────────────────────────────────────────────────────────────┤
│ SLIDE UP (for list items)                                   │
├─────────────────────────────────────────────────────────────┤
│ • Initial: opacity 0, translateY(20-30px)                   │
│ • Final: opacity 1, translateY(0)                           │
│ • Duration: 400ms                                           │
│ • Easing: ease-out                                          │
├─────────────────────────────────────────────────────────────┤
│ MESSAGE BUBBLES (chat)                                      │
├─────────────────────────────────────────────────────────────┤
│ • Initial: opacity 0, translateY(15px), scale(0.95)         │
│ • Final: opacity 1, translateY(0), scale(1)                 │
│ • Stagger: 40ms between messages                            │
└─────────────────────────────────────────────────────────────┘
```

### 5.3 Micro-interactions

```
┌─────────────────────────────────────────────────────────────┐
│ BUTTON TAP                                                  │
├─────────────────────────────────────────────────────────────┤
│ • Scale: 1 → 0.95 → 1 (or 0.9 for icon buttons)             │
│ • Duration: 100ms down, 200ms up                            │
│ • Type: spring (damping: 15, stiffness: 300)                │
├─────────────────────────────────────────────────────────────┤
│ TOGGLE SWITCH                                               │
├─────────────────────────────────────────────────────────────┤
│ • Knob position: spring animation                           │
│ • Track color: instant change                               │
├─────────────────────────────────────────────────────────────┤
│ COLLAPSE/EXPAND                                             │
├─────────────────────────────────────────────────────────────┤
│ • Height: 0 ↔ auto                                          │
│ • Opacity: 0 ↔ 1                                            │
│ • Duration: 200ms                                           │
│ • Arrow rotation: 0° ↔ 90°                                  │
├─────────────────────────────────────────────────────────────┤
│ BADGE APPEAR                                                │
├─────────────────────────────────────────────────────────────┤
│ • Initial: scale(0)                                         │
│ • Final: scale(1)                                           │
│ • Type: spring (bounce)                                     │
├─────────────────────────────────────────────────────────────┤
│ ONLINE STATUS PULSE                                         │
├─────────────────────────────────────────────────────────────┤
│ • Scale: [1, 1.2, 1]                                        │
│ • Duration: 2 seconds                                       │
│ • Repeat: infinite                                          │
└─────────────────────────────────────────────────────────────┘
```

### 5.4 Special Animations

```
┌─────────────────────────────────────────────────────────────┐
│ NUDGE EFFECT (Screen Shake)                                 │
├─────────────────────────────────────────────────────────────┤
│ Target: Entire chat screen container                        │
│ Property: translateX                                        │
│ Keyframes: [0, -15, 15, -15, 15, -10, 10, 0] px             │
│ Duration: 500ms                                             │
│ Trigger: Nudge button tap or received nudge                 │
│ Haptic: Medium impact (if available)                        │
├─────────────────────────────────────────────────────────────┤
│ NUDGE BUTTON IDLE WIGGLE                                    │
├─────────────────────────────────────────────────────────────┤
│ Property: rotate                                            │
│ Keyframes: [0, -8, 8, -8, 8, 0] degrees                     │
│ Duration: 600ms                                             │
│ Repeat: infinite                                            │
│ Repeat delay: 3 seconds                                     │
├─────────────────────────────────────────────────────────────┤
│ BUTTERFLY FLUTTER                                           │
├─────────────────────────────────────────────────────────────┤
│ Property: rotate                                            │
│ Keyframes: [0, -2, 2, 0] degrees                            │
│ Duration: 3 seconds                                         │
│ Repeat: infinite                                            │
│ Easing: easeInOut                                           │
├─────────────────────────────────────────────────────────────┤
│ WINK BUTTON HOVER                                           │
├─────────────────────────────────────────────────────────────┤
│ Scale: 1 → 1.15                                             │
│ Optional: slight rotate wiggle                              │
├─────────────────────────────────────────────────────────────┤
│ ONBOARDING FLOATING BUBBLES                                 │
├─────────────────────────────────────────────────────────────┤
│ translateY: [0, -20, 0] px                                  │
│ translateX: [0, ±10, 0] px (alternating)                    │
│ Duration: 4-6 seconds (varied per bubble)                   │
│ Repeat: infinite                                            │
│ Stagger delay: 0.3s between bubbles                         │
└─────────────────────────────────────────────────────────────┘
```

---

## 6. State Management

### 6.1 Global App State

```
┌─────────────────────────────────────────────────────────────┐
│ APP STATE STRUCTURE                                         │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│ {                                                           │
│   // Theme                                                  │
│   isDarkMode: boolean,                                      │
│                                                             │
│   // Auth                                                   │
│   isOnboarded: boolean,                                     │
│   isAuthenticated: boolean,                                 │
│                                                             │
│   // Navigation                                             │
│   activeTab: 'buddies' | 'chats' | 'me',                    │
│   currentScreen: 'main' | 'chat' | 'settings',              │
│   selectedContact: Contact | null,                          │
│                                                             │
│   // User Profile (editable!)                               │
│   user: {                                                   │
│     displayName: string,   // e.g., "~*Your Name*~ 🎵"      │
│     status: PresenceStatus,                                 │
│     message: string,       // personal message              │
│     email: string,                                          │
│   },                                                        │
│                                                             │
│   // Contacts                                               │
│   groups: Group[],         // user-created groups           │
│   recentChats: Chat[],                                      │
│                                                             │
│   // Settings                                               │
│   settings: {                                               │
│     notifications: boolean,                                 │
│     nudgeVibration: boolean,                                │
│     messageSounds: boolean,                                 │
│     showNowPlaying: boolean,                                │
│   },                                                        │
│ }                                                           │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

### 6.2 Data Models

```typescript
// Type definitions (language-agnostic representation)

PresenceStatus = 'online' | 'away' | 'busy' | 'offline'

Contact = {
  id: string
  name: string           // their display name (can be fancy!)
  status: PresenceStatus
  message: string        // their personal message
  frameColor?: string    // optional custom avatar frame color
}

Group = {
  id: string
  name: string           // e.g., "⭐ Best Friends"
  contacts: Contact[]
  isExpanded: boolean    // UI state
}

Chat = {
  id: string
  contactId: string
  lastMessageTime: Date
  unreadCount: number
}

Message = {
  id: string
  text: string
  senderId: string       // 'me' or contact id
  timestamp: Date
  type: 'text' | 'wink' | 'nudge'
  winkEmoji?: string     // if type is 'wink'
}
```

### 6.3 UI State

```
┌─────────────────────────────────────────────────────────────┐
│ SCREEN-SPECIFIC UI STATE                                    │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│ BuddyListScreen:                                            │
│ • expandedGroups: string[]  // ids of expanded groups       │
│                                                             │
│ ProfileScreen:                                              │
│ • editingName: boolean                                      │
│ • editingMessage: boolean                                   │
│ • showStatusPicker: boolean                                 │
│ • tempName: string         // draft while editing           │
│ • tempMessage: string                                       │
│                                                             │
│ ChatScreen:                                                 │
│ • messageInput: string                                      │
│ • showWinkPanel: boolean                                    │
│ • isNudgeActive: boolean   // for shake animation           │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

## 7. Interaction Patterns

### 7.1 Tap Targets

```
Minimum touch target: 44x44px (iOS standard)
Recommended: 48x48px for primary actions

Specific targets:
• Tab bar items: 80px wide minimum
• List rows: full width, 56-70px height
• Icon buttons: 36-44px
• Toggle switches: 50x30px
```

### 7.2 Gestures

```
┌─────────────────────────────────────────────────────────────┐
│ SUPPORTED GESTURES                                          │
├─────────────────────────────────────────────────────────────┤
│ TAP:                                                        │
│ • All buttons, list items, interactive elements             │
│ • Visual feedback: scale or background change               │
│                                                             │
│ LONG PRESS:                                                 │
│ • Contact row → Show quick actions (message, call, etc.)    │
│ • Message bubble → Show options (copy, delete, etc.)        │
│                                                             │
│ SWIPE (optional):                                           │
│ • Edge swipe right to go back (native behavior)             │
│ • Swipe on chat row → Quick actions                         │
│                                                             │
│ SCROLL:                                                     │
│ • Standard momentum scrolling                               │
│ • Bounce at edges (iOS) or glow (Android)                   │
└─────────────────────────────────────────────────────────────┘
```

### 7.3 Form Behaviors

```
┌─────────────────────────────────────────────────────────────┐
│ TEXT INPUT                                                  │
├─────────────────────────────────────────────────────────────┤
│ Display Name field:                                         │
│ • Max length: 64 characters                                 │
│ • Allowed: letters, numbers, emojis, symbols (~*_-♪★)       │
│ • Auto-focus when entering edit mode                        │
│ • Submit: tap Save or press Return                          │
│                                                             │
│ Personal Message field:                                     │
│ • Max length: 128 characters                                │
│ • Same character rules as display name                      │
│ • Placeholder: "What's on your mind?"                       │
│                                                             │
│ Chat input field:                                           │
│ • Multi-line capable (expands to ~4 lines max)              │
│ • Send on Return (mobile) or Send button                    │
│ • Emoji button opens system emoji picker                    │
└─────────────────────────────────────────────────────────────┘
```

### 7.4 Loading & Empty States

```
┌─────────────────────────────────────────────────────────────┐
│ EMPTY STATES                                                │
├─────────────────────────────────────────────────────────────┤
│ No recent chats:                                            │
│ • Icon: 💬 (large)                                          │
│ • Title: "No recent conversations"                          │
│ • Subtitle: "Select a buddy to start chatting!"             │
│                                                             │
│ No contacts in group:                                       │
│ • Show collapsed group header only                          │
│ • Or: "No contacts in this group" message                   │
│                                                             │
│ All contacts offline:                                       │
│ • Group auto-collapses                                      │
│ • Online count shows "0/X"                                  │
├─────────────────────────────────────────────────────────────┤
│ LOADING STATES                                              │
├─────────────────────────────────────────────────────────────┤
│ Initial load:                                               │
│ • Animated butterfly logo centered                          │
│ • "Connecting..." text below                                │
│                                                             │
│ Sending message:                                            │
│ • Bubble appears immediately (optimistic)                   │
│ • Subtle "sending" indicator if delayed                     │
└─────────────────────────────────────────────────────────────┘
```

---

## 8. Accessibility Requirements

### 8.1 Color Contrast

```
Minimum contrast ratios (WCAG AA):
• Normal text: 4.5:1
• Large text (18px+): 3:1
• UI components: 3:1

Ensure all text passes in both light and dark modes.
Presence colors must be distinguishable by more than color alone
(shape, label, position).
```

### 8.2 Screen Reader Support

```
┌─────────────────────────────────────────────────────────────┐
│ SEMANTIC LABELS                                             │
├─────────────────────────────────────────────────────────────┤
│ Avatar + Status:                                            │
│ "[Name], [Status]"                                          │
│ Example: "Sarah Chen, Online"                               │
│                                                             │
│ Contact row:                                                │
│ "[Name], [Status], [Personal message if any]"               │
│ Example: "Mike, Away, Playing Valorant"                     │
│                                                             │
│ Unread badge:                                               │
│ "[X] unread messages"                                       │
│                                                             │
│ Group header:                                               │
│ "[Group name], [X] of [Y] online, [expanded/collapsed]"     │
│                                                             │
│ Toggle switch:                                              │
│ "[Setting name], [on/off], toggle"                          │
│                                                             │
│ Nudge button:                                               │
│ "Send a nudge"                                              │
└─────────────────────────────────────────────────────────────┘
```

### 8.3 Motion & Reduced Motion

```
Respect prefers-reduced-motion:

If reduced motion preferred:
• Disable floating bubble animations
• Disable butterfly flutter
• Disable nudge button wiggle
• Keep nudge shake but reduce intensity
• Use instant transitions instead of slides
• Keep functional animations (toggles, collapses)
```

### 8.4 Touch Accessibility

```
• All interactive elements: 44px minimum
• Adequate spacing between tap targets (8px minimum)
• Clear focus states for keyboard navigation
• Support for system font size scaling
```

---

## Appendix A: Asset Checklist

```
ICONS/GRAPHICS:
☐ Butterfly logo (SVG, multiple sizes)
☐ Tab bar icons (or use emoji)
☐ Back arrow
☐ Settings gear
☐ Edit pencil
☐ Call/Video icons
☐ Send arrow
☐ Plus/Add icon

SOUNDS (optional):
☐ Message received
☐ Message sent
☐ Nudge sound
☐ Online status change

HAPTICS:
☐ Nudge: medium impact
☐ Button tap: light impact
☐ Toggle: light impact
```

---

## Appendix B: Platform Considerations

### iOS
- Use SF Pro Display font
- Safe areas: top (47px notch), bottom (34px home indicator)
- Haptic feedback via UIFeedbackGenerator
- Respect Dynamic Type for accessibility

### Android
- Use Roboto or system font
- Safe areas vary by device
- Haptic feedback via VibrationEffect
- Respect system font scale

### Cross-platform (Flutter/React Native)
- Use platform-adaptive safe areas
- Conditional font stacks
- Abstract haptic APIs

---

## Appendix C: Sample Display Names

For testing and placeholder content:

```
~*Sarah*~ ♪
Mike 🎮
xX_Alex_Xx
Emma ★彡
♫ Music Lover ♫
→ David ←
[AFK] Jake
✿ Lisa ✿
《 Tom 》
• Chris •
```

---

*Document Version: 1.0*
*Last Updated: December 2024*
*Design System: MSN Messenger 2026 Nostalgic Edition*
