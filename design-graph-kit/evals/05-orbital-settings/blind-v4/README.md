# Blind Replication v4 — validating the v0.4 Uno-first kit

Five fresh-context blind agents against kit v0.4 (Uno mapping layer +
all v0.2/v0.3 semantic rules), with optional Uno docs MCP access for the
mapping layer. Scored with scorer 0.3.0 against gold v1.3 (46 mapping
triples). All five validated; zero hallucination flags.

## Headline — run-to-run stability is now strong

| Metric | v0.1 | v0.2 | **v0.4** |
|---|---:|---:|---:|
| mean pairwise macro F1 | 0.497 | 0.563 | **0.758** (best pair 0.91) |
| mean pairwise node-id F1 | 0.707 | 0.814 | **0.929** |
| nodes per run | 85–90 | 68–81 | **57–61** (gold 47) |
| drift tail (singleton ids) | 72 | 30 | **9** |
| ids identical in all 5 runs | 48 | 53 | **52** (of ~57 per run) |
| hallucinations (20 blind runs total) | 0 | 0 | **0** |

Five independent blind runs now produce near-clone graphs: identical sizes,
92–97% pairwise node-id overlap, a drift tail of nine ids (mostly slug-length
variants like `recents-db` vs `recents-database`).

## The mapping layer works blind

- Strict triple-level (`node type` + field + value) recovery vs gold:
  **F1 0.64–0.66** in every run.
- **Verbatim value recovery: recall 0.86–0.90**, F1 0.774–0.787 — the runs
  recover almost every declared resource key, x:Name, style key, and control
  type exactly, blind, in a five-run band of ±0.007. Copy-don't-coin holds.
- The gap between the two numbers is mostly *classification variance*, not
  loss: e.g. runs put action-button `xName`s on component-typed instance
  nodes where gold uses `control` nodes, or model the Cleared dialog as a
  `state` carrying `type: ContentDialog` where gold uses a `component`.
  Genuine omissions are small (icon glyphs, two style keys).

## What remains vs gold — and it changed character

vs-gold macro is 0.32 (v0.2: 0.26), but the residual is no longer generator
instability. Pairwise edge agreement between runs reaches 0.95 while
vs-gold edge F1 sits at ~0.17: the five runs **agree with each other** on a
token-wiring altitude that is systematically richer than gold's — ~53
`uses-token` edges (typography/color per content-style consumer) vs gold's
11, and they consistently include the text-emphasis brushes
(`OrbitalText50Brush`/`72`) that gold's minimal token set omits.

That is a **consensus-vs-answer-key calibration question**, not a defect to
rule away: five independent generators keep voting that the screen's token
wiring includes per-style typography/color consumption. Options for the next
iteration, to be decided deliberately: (a) gold v1.4 adopts the consensus
wiring; (b) a rule pins typography tokens to canonical style-consumer
concepts only. Per `first-test.md`, this should be an explicit answer-key
decision, not a quiet change.

Two small true recall gaps to keep: icon glyph capture (0/5 runs carried
glyphs) and one run dropping the clear-recents trigger (4/5 carried both
behavioral edges; the fifth kept save→saved only).

## Verdict

The Uno-first format survives blind replication on its first outing:
stability doubled again, the mapping layer transports declared Uno identity
nearly losslessly, and honesty discipline is intact across all twenty blind
runs to date. The kit's remaining open item is an answer-key calibration
(token-wiring altitude), then a **second eval screen from a different app**
to test that none of this is Orbital-specific.

## Reproduce

```bash
python scripts/score_graph.py evals/05-orbital-settings/gold.graph.json \
  evals/05-orbital-settings/blind-v4/run1.graph.json   # …run5
```
