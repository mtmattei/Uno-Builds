# Session Usage / Cost Capture

The coding agent must not estimate token usage or dollar cost.

After each Claude Code session ends, capture/export the provider/Claude Code/harness usage information that is actually available and save it beside that run as `results/external-session-usage.txt` or JSON/CSV if your tooling exports structured data.

Capture when available:

- exact Claude model;
- input tokens;
- output tokens;
- cache-read tokens;
- cache-write tokens;
- other billed token categories;
- actual reported session cost in USD;
- session start/end timestamps.

If the host exposes a core-checkpoint usage snapshot during the Uno session, capture it immediately after `core_complete` before Desktop-head-specific work. This allows Desktop incremental tokens/cost to be calculated. If no checkpoint usage is available, report Desktop incremental token/cost as unavailable rather than estimating it.

When actual billed cost is available, prefer it over recomputing cost from token counts. If you calculate an estimate because billed cost is unavailable, record the exact pricing source/date/model and label the value **estimated**.
