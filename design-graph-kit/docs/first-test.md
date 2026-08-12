# First Test: Exact Walkthrough

Use this after unpacking the kit.

## 1. Install the validator dependency

### PowerShell

```powershell
cd design-graph-kit
py -m venv .venv
.\.venv\Scripts\Activate.ps1
python -m pip install -r requirements.txt
```

### macOS/Linux

```bash
cd design-graph-kit
python3 -m venv .venv
source .venv/bin/activate
python -m pip install -r requirements.txt
```

## 2. Verify the kit

```bash
python scripts/run_all.py
```

All four gold graphs should pass.

## 3. Pick one real design

Start with a single settings/profile screen.

Avoid:
- a whole application;
- animation;
- complex navigation;
- a highly experimental canvas.

The first test is about semantic extraction, not coverage.

## 4. Create the gold graph yourself

Before asking the model to generate anything:

1. Copy `evals/01-settings/`.
2. Replace the fixture description with your real design description/source notes.
3. Edit `gold.graph.json` by hand to represent what you believe is correct.
4. Validate it.

This prevents the model's output from defining the answer key.

## 5. Generate without the Skill

Supply your model with:
- the real design;
- `schema/design-graph.schema.json`;
- `references/ontology.md`;
- `references/inference-rules.md`;
- `references/token-rules.md`;
- `prompts/design-understanding.md`.

Save exactly the JSON response as:

`generated.graph.json`

## 6. Validate

```bash
python scripts/validate_graph.py path/to/generated.graph.json
```

Fix format/schema issues separately from semantic issues. Do not quietly change the gold graph to match the model.

## 7. Deterministic comparison

```bash
python scripts/score_graph.py path/to/gold.graph.json path/to/generated.graph.json
```

The exact score is not the whole evaluation. It is mainly useful for:
- regression;
- naming drift;
- missing/extra concepts;
- repeatability.

## 8. Human review

Use `docs/manual-scorecard.md`.

The most important failure to catch is unsupported hidden behavior.

## 9. Repeat five times

Generate the same graph five times with no intentional prompt changes.

Look for:
- renamed canonical concepts;
- component consolidation changing between runs;
- state modeling changing;
- invented behavior appearing intermittently.

## 10. Revise the system, not the answer key

If multiple runs fail in the same way:
- improve the Skill/prompt;
- clarify an ontology definition;
- tighten an inference rule;
- only expand the schema when representation is genuinely missing.

## 11. Test the Skill

Once manual prompting is stable, run the same inputs using `SKILL.md`.

The Skill should meet or beat the manual prompt.

## 12. Run the value experiment

Baseline:

`Design -> Uno implementation`

Treatment:

`Design -> Design Graph -> Uno implementation`

Keep everything else the same.

Compare:
- visual parity;
- component reuse;
- state coverage;
- token consistency;
- semantic structure;
- code quality;
- correction prompts;
- hallucinations.

## 13. Decide whether to proceed

Proceed to product integration only if the graph gives a meaningful downstream advantage.

## 14. Round-trip test

If the graph helps implementation:

`Design -> Graph A -> implementation -> analyze implementation/runtime -> Graph B`

Compare Graph A and Graph B to find semantic parity defects that a pixel diff alone may miss.
