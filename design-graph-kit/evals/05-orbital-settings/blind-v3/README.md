# Blind Replication v3 — archived (agnostic v0.3 rules)

Five fresh-context blind runs against kit v0.3 completed here, but the full
scoring/comparison was **paused before analysis**: the kit pivoted Uno-first
(v0.4, `docs/architecture.md`) and the next validation round should exercise
the `properties.uno` mapping layer, which these runs predate.

The artifacts are kept as the agnostic-v0.3 archive. Headline from the run
reports (unscored): sizes converged hard (61–64 nodes vs v2's 68–81, gold
47), every run has exactly 15 `instance-of` and the 2 real `triggers`
edges, and `uses-token` dropped to 51–60 (from v2's 66–71; gold 11) — the
v0.3 attachment rule reduced but did not eliminate token-edge inflation.

Next validation round: blind 5× under v0.4 (Uno-first), scoring both the
semantic layer (as before) and the new mapping layer (exact
resourceKey/xName/type recovery against gold v1.3).
