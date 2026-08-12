# Fixture: Dashboard

Use a dashboard with a `Metrics` region containing three visually identical cards:
- Revenue / `$42,180`
- Orders / `384`
- Customers / `128`

All cards:
- have the same child structure;
- use 12 px corner radius;
- are separated by 16 px;
- are intended to exercise repeated-component detection.

## What this eval tests

- component consolidation;
- `instance-of`;
- shared token use;
- stable naming.
