# Fixture: Orders states

Use four visual representations of the **same Orders screen**:

1. Loading — content area displays a progress indicator.
2. Empty — content area displays an empty-state message/illustration.
3. Populated — content area displays an orders list.
4. Error — content area displays an error treatment with a `Retry` button.

The visual does not expose the exact retry implementation.

## What this eval tests

- one conceptual screen;
- four `state` nodes;
- `has-state`;
- avoidance of duplicate screen concepts;
- unresolved behavior.
