# Morning Drive Card

A phone-width card a car app would show first thing in the morning. It answers one question at a glance: **when do I leave for school drop-off?** Everything else on the card (fuel range, cabin temperature, tires, where the car is parked) supports that answer.

Open `morning-card.html` in a browser. The tuning panel under the card ("Dial it in") maps every slider to a state value or a CSS variable, so the states and the feel can be dialed in live.

Built with the `frontend-design` skill layered with the `frontend-design-unique` ledger protocol, and the Interface Craft workflow by Josh Puckett (CSS-variable theming, storyboard motion, live tuning dials, a critique pass before shipping).

## Architecture Brief

- Structure: one self-contained HTML file. The card is `article.mc`; the tuning panel is `aside.dials` and is not part of the component.
- State model: a plain object `S` (clock, bell, base drive, traffic delay, walk-in time, range, cabin, tire, state, motion speed). `derive()` computes drive, leave-by, slack and arrival. `render()` is the single writer to the DOM.
- Data flow: dials write to `S` or to CSS variables on `:root`, then call `render()`. The card never reads from the dials.
- Services: none. Real data would come from the vehicle telemetry and a routing API; the shape of `S` is the contract.
- Platform constraints: phone width first (390 px), works at desktop width. Fonts load from Google Fonts with a real fallback stack.
- Validation: screenshots of live, dark, late, warn, offline, loading and empty states at 390 px; console clean.

## Design Brief

- Direction: instrument-clean. Cool, windshield-white ground with a blue-biased ink, one cobalt accent, and separate semantic amber and red for traffic and warnings.
- Signature device: the slack ruler. A minute-tick time ruler with the current time as a needle, the drive as a solid block (traffic delay as its own amber segment), and the bell as a dashed hard stop. The gap between the needle and the block is the slack, and it is labeled in minutes.
- Type: Barlow Semi Condensed for numerals (road-signage lineage), Barlow for text. Scale: 11 / 12.5 / 13 / 14 / 15 / 22 / 72.
- Spacing: 20 px card padding, 16 px between sections, 12 x 16 px instrument grid.
- Hierarchy: header (day, car, weather), the answer (kicker, leave-by time, one-line route sentence), the ruler, four instruments, two actions.
- Theme: full light and dark token sets. Dark redefines tokens only.
- Responsive: single column at every width; ruler and fuel ruler redraw on resize.

## Interaction Brief

- Flows: read the time, press "Open route", or press "Warm cabin to 20°" and watch the cabin value climb.
- Headline logic: slack of 2 min or more shows "Leave by 7:33"; under 2 min shows "Leave now"; negative slack shows "Running late" with minutes late and the arrival if you leave now.
- Empty state: no drop-off (journée pédagogique). The ruler and route action hide; the car instruments stay.
- Loading state: skeleton for the answer, the ruler shows ticks and the bell only.
- Error state: offline banner with a Retry link, stale values dimmed, the route sentence says traffic is from the last update.
- Motion: one orchestrated entrance. Ticks fade in left to right (7 ms stagger), the drive block grows from its start (560 ms, ease-out quart), the needle slides in. Nothing else animates on load. The heat button answers a press with a state change and a small ring. `prefers-reduced-motion` skips all of it.
- Accessibility: visible focus rings, the ruler has a title, the offline banner is a status region, the heat button uses `aria-pressed`.
- Verification: drag Clock past 7:33 to see the late state; set Fuel range under 90 and Front-left tire under 30 to see warnings; switch state chips.

## Unresolved Questions

- Should leave-by include a configurable walk-in buffer per school, or come from the routing API?
- Fuel range under 90 km suggests a station on the route; that needs a real POI lookup.
- Port to Uno Platform XAML (a `MorningCard` control) if this direction is approved.
