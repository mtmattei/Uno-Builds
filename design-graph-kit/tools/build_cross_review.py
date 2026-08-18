#!/usr/bin/env python3
"""Build a self-contained cross-model review bundle for a gold graph.

Every gold, rule and checker in this kit came from one agent lineage. A
systematic blind spot in that lineage is invisible to itself, and a checker
written by the same lineage inherits the blind spot rather than catching it -
which is not hypothetical: three tools in this kit were confidently wrong until
data contradicted them.

This packages everything an *outside* model needs to review a gold in one file:
the task, the altitude rules, the gold graph, and every source file the gold
cites. One file, no repo access, no tooling - so it can be handed to any model
from any vendor.

Usage:
  python3 tools/build_cross_review.py 05-orbital-settings
  python3 tools/build_cross_review.py 05-orbital-settings --out bundle.md
"""

from __future__ import annotations

import argparse
import json
import re
import sys
from pathlib import Path

TASK = """\
# Task: independently review an AI-authored answer key

You are reviewing a **gold graph** - a hand-authored answer key that other AI
runs are scored against. It was authored by a different AI model lineage, and
so was every automated checker applied to it. Your value here is precisely that
you are outside that lineage: you are looking for what it systematically
missed, not confirming what it got right.

## What has already been machine-verified (do not re-do this)

Every identifier the gold quotes from source - resource keys, `x:Name`s, style
keys, class names - has been checked to exist verbatim in the source files
below. The result was **zero fabrications**. Precision is not the question.

## What no checker in that lineage can catch, and what you should hunt

1. **Recall defects - things that should be in the graph and are not.**
   This is the priority. Automated checks validate what *is* present; nothing
   can flag an omission without independently modeling the screen. A previous
   version of this answer key omitted an entire reusable component and its
   search affordance, and every precision check passed it.
2. **Altitude errors** - modeling at the wrong level: layout wrappers promoted
   to meaningful nodes, or meaningful structure flattened away.
3. **Evidence that does not support its claim** - a node whose rationale does
   not follow from the cited file, or an `unresolved` item that is actually
   decidable from the source provided.
4. **Wrong-but-plausible semantics** - a node typed or named in a way that
   reads fine but misrepresents what the source does.

## Required output

For each finding:

- **Severity**: critical (answer key is wrong) / major (defensible but likely
  wrong) / minor (hygiene)
- **Location**: the node or edge id, and the source `file:line` that proves it
- **Claim**: what the gold asserts
- **Reality**: what the source actually shows
- **Fix**: the specific change you would make

Rules for your report:

- Cite `file:line` for every finding. A finding without source evidence is
  noise.
- Do not list what is correct. No summary of strengths. Findings only.
- If you find no defect in a category, say so in one line and move on.
- Flag your own uncertainty explicitly rather than hedging language.

## The open question you must rule on

The kit is undecided about `region` nodes. This gold contains **{region_count}**
of them, while the page it models is built from {container_count} nested layout
containers. Ten independent runs of other screens emitted 6-10 region nodes
each, unprompted.

The proposed rule is: *emit a `region` when the source declares a structural
grouping that owns layout or state and is not itself a reusable component; do
not emit one for every layout panel. The test is whether removing the grouping
would change the screen's meaning or only its arrangement.*

**Rule on it.** Should this gold contain region nodes? If yes, which ones, and
what is your reasoning? If no, explain why the arrangement carries no meaning
worth modeling. Answer directly - a hedge here is worth nothing.
"""


def kit_root() -> Path:
    return Path(__file__).resolve().parents[1]


def read_source(source_root: Path, rel: str) -> str:
    path = source_root / rel
    if path.is_dir():
        parts = []
        for p in sorted(path.rglob("*")):
            if p.is_file() and p.suffix.lower() in (".xaml", ".cs"):
                parts.append(f"--- {p.relative_to(source_root).as_posix()} ---\n"
                             + p.read_text(encoding="utf-8", errors="replace"))
        return "\n\n".join(parts)
    if path.is_file():
        return path.read_text(encoding="utf-8", errors="replace")
    return ""


def numbered(text: str) -> str:
    """Line numbers, so the reviewer can cite file:line as demanded."""
    return "\n".join(f"{i:5d}  {line}" for i, line in enumerate(text.splitlines(), 1))


def build(eval_name: str, source_root: Path) -> str:
    eval_dir = kit_root() / "evals" / eval_name
    gold_path = eval_dir / "gold.graph.json"
    if not gold_path.exists():
        sys.exit(f"No gold at {gold_path}")
    gold = json.loads(gold_path.read_text(encoding="utf-8"))

    cited: list[str] = []
    for node in gold.get("nodes", []):
        rel = ((node.get("evidence") or {}).get("source") or {}).get("path")
        if rel and rel not in cited:
            cited.append(rel)

    # Some nodes cite a design system by glob label ("Orbital Styles/*.xaml")
    # rather than a path, so the style dictionaries backing every token claim
    # would be missing from the bundle - leaving the reviewer unable to check
    # the tokens at all. Pull in whatever the fixture names as source too.
    fixture_path = eval_dir / "fixture.md"
    if fixture_path.exists():
        fixture_text = fixture_path.read_text(encoding="utf-8", errors="replace")
        for m in re.finditer(r"`([^`\n]+\.(?:xaml|cs))`", fixture_text):
            rel = m.group(1).strip()
            if rel not in cited and (source_root / rel).is_file():
                cited.append(rel)

    # A XAML file's behavior lives in its code-behind, so a reviewer asked to
    # check behavioral claims needs both halves.
    for rel in list(cited):
        if rel.endswith(".xaml"):
            sibling = rel + ".cs"
            if sibling not in cited and (source_root / sibling).is_file():
                cited.append(sibling)

    # The region question must state this gold's ACTUAL counts. An earlier
    # version hardcoded "zero regions", which was false for a gold that had two
    # - a reviewer caught the false premise and had to spend a finding on it.
    region_count = sum(1 for n in gold.get("nodes", []) if n.get("type") == "region")
    container_re = re.compile(r"<(Grid|StackPanel|utu:AutoLayout|RelativePanel|Canvas|ScrollViewer)[\s>]")
    container_count = sum(len(container_re.findall(read_source(source_root, rel)))
                          for rel in cited if rel.endswith(".xaml"))

    out: list[str] = [
        TASK.replace("{region_count}", str(region_count))
            .replace("{container_count}", str(container_count)),
        "",
    ]
    w = out.append

    w("---")
    w("")
    w(f"# The answer key under review: `{eval_name}`")
    w("")
    w(f"{len(gold.get('nodes', []))} nodes · {len(gold.get('edges', []))} edges · "
      f"{len(gold.get('unresolved', []))} unresolved items")
    w("")

    fixture = eval_dir / "fixture.md"
    if fixture.exists():
        w("## What this screen is (the eval's own description)")
        w("")
        w(fixture.read_text(encoding="utf-8", errors="replace"))
        w("")

    for ref in ("references/ontology.md", "references/inference-rules.md",
                "references/token-rules.md"):
        path = kit_root() / ref
        if path.exists():
            w(f"## Rules the gold is supposed to follow — `{ref}`")
            w("")
            w("```markdown")
            w(path.read_text(encoding="utf-8", errors="replace"))
            w("```")
            w("")

    w("## The gold graph")
    w("")
    w("```json")
    w(json.dumps(gold, indent=2))
    w("```")
    w("")

    w("## Source files the gold cites")
    w("")
    w("Line-numbered so you can cite `file:line`.")
    w("")
    for rel in cited:
        body = read_source(source_root, rel)
        if not body:
            w(f"### `{rel}` — NOT FOUND")
            w("")
            continue
        lang = "xml" if rel.endswith(".xaml") else "csharp"
        w(f"### `{rel}`")
        w("")
        w(f"```{lang}")
        w(numbered(body))
        w("```")
        w("")

    return "\n".join(out)


def main() -> int:
    ap = argparse.ArgumentParser(description=__doc__,
                                 formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("eval_name")
    ap.add_argument("--source-root", default=str(kit_root().parent))
    ap.add_argument("--out", default=None)
    args = ap.parse_args()

    text = build(args.eval_name, Path(args.source_root))
    out = Path(args.out) if args.out else (
        kit_root() / "evals" / args.eval_name / "cross-model-review-bundle.md")
    out.write_text(text, encoding="utf-8")
    kb = len(text.encode("utf-8")) / 1024
    print(f"wrote {out}  ({kb:.0f} KB, ~{len(text)//4:,} tokens)")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
