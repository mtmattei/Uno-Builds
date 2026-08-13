#!/usr/bin/env python3
"""Generate the human-review packet for a gold graph (HANDOFF item C).

Every answer key in this kit was authored and calibrated by the same agent
lineage. One independent human review removes the last circularity - but a
reviewer should be spending attention on judgment calls, not on grepping XAML
for string literals. So this script does the mechanical half first:

  * lays out every node and edge with its evidence, rationale, and uno mapping;
  * checks each `properties.uno` value against the real source files, flagging
    any identifier that does not literally appear where the node's evidence
    says it came from;
  * lists the checks that only a human can make, per the altitude contract.

The verdict columns are left blank on purpose. Do not edit gold from here:
record findings, then apply accepted fixes through tools/build_graphs.py so
the graph stays generated rather than hand-patched.

Usage:
  python3 tools/build_review_packet.py 05-orbital-settings [--source-root .]
"""

from __future__ import annotations

import argparse
import json
import re
import sys
from pathlib import Path

# uno mapping keys whose values are identifiers that must appear verbatim in
# the source. Keys like "type" (a WinUI control name) or "mechanism" (prose)
# are excluded: they are classifications, not quotations.
QUOTED_UNO_KEYS = {
    "xName",
    "styleKey",
    "resourceKey",
    "fontResourceKey",
    "class",
    "property",
    "member",
    "iconGlyph",
}


def kit_root() -> Path:
    return Path(__file__).resolve().parents[1]


def load_graph(eval_name: str) -> tuple[dict, Path]:
    path = kit_root() / "evals" / eval_name / "gold.graph.json"
    if not path.exists():
        sys.exit(f"No gold graph at {path}")
    with path.open(encoding="utf-8") as f:
        return json.load(f), path


def collect_sources(graph: dict, source_root: Path) -> dict[str, str]:
    """Read every source file the graph cites by concrete path, once."""
    texts: dict[str, str] = {}
    for node in graph.get("nodes", []):
        src = (node.get("evidence") or {}).get("source") or {}
        rel = src.get("path")
        if not rel or rel in texts:
            continue
        path = source_root / rel
        if path.is_dir():
            # A gold may cite a folder ("Caffe/Caffe/Controls") when a group of
            # files backs one node. Concatenate the sources inside it so the
            # citation still verifies.
            texts[rel] = "\n".join(
                p.read_text(encoding="utf-8", errors="replace")
                for p in sorted(path.rglob("*"))
                if p.is_file() and p.suffix.lower() in (".xaml", ".cs")
            )
        elif path.is_file():
            texts[rel] = path.read_text(encoding="utf-8", errors="replace")
        else:
            texts[rel] = ""
    return texts


def collect_app_corpus(texts: dict[str, str], source_root: Path) -> str:
    """Read the whole app the cited files belong to.

    Nodes may cite a design system by glob label ("Orbital Styles/*.xaml")
    rather than a path, which leaves nothing to verify against. Widening to
    the app root separates a value that exists but is cited loosely (a
    citation defect) from one that exists nowhere (a fabrication).
    """
    roots = {Path(rel).parts[0] for rel in texts if Path(rel).parts}
    corpus: list[str] = []
    for root in roots:
        base = source_root / root
        if not base.is_dir():
            continue
        for path in base.rglob("*"):
            if path.suffix.lower() not in (".xaml", ".cs"):
                continue
            if any(part in ("bin", "obj") for part in path.parts):
                continue
            corpus.append(path.read_text(encoding="utf-8", errors="replace"))
    return "\n".join(corpus)


def check_uno_values(
    graph: dict, texts: dict[str, str], app_corpus: str
) -> tuple[list[dict], list[dict], list[dict]]:
    """Classify quoted uno values that are not literal quotations.

    Three outcomes, in descending severity:

      fabricated - the identifier appears nowhere in the application. A real
                   copy-don't-coin violation.
      compound   - the string is not literal, but every identifier inside it
                   exists (e.g. "HasSelection / IsSelected", or a name with a
                   parenthetical gloss). The mapping is right; the field is
                   carrying prose, which defeats machine checking.
      miscited   - the value exists verbatim in the app, but not in any file
                   the node's own evidence points at. A provenance defect.
    """
    fabricated: list[dict] = []
    compound: list[dict] = []
    miscited: list[dict] = []
    cited_text = "\n".join(texts.values())

    for node in graph.get("nodes", []):
        uno = ((node.get("properties") or {}).get("uno")) or {}
        src = (node.get("evidence") or {}).get("source") or {}
        for key, value in uno.items():
            if key not in QUOTED_UNO_KEYS or not isinstance(value, str):
                continue
            # Glyphs are written as &#xE74D; in XAML but as the codepoint here.
            needle = value
            if key == "iconGlyph":
                m = re.search(r"[0-9A-Fa-f]{4}", value)
                needle = m.group(0) if m else value
            if not needle or needle in cited_text:
                continue

            row = {
                "node": node["id"],
                "key": key,
                "value": value,
                "cited": src.get("path") or src.get("label") or "-",
            }

            if needle in app_corpus:
                miscited.append(row)
                continue

            # Not literal anywhere. Decide whether it is prose wrapping real
            # identifiers, or genuinely invented.
            parts = [p for p in re.split(r"[/(),]| - ", needle) if p.strip()]
            idents = [p.strip() for p in parts if re.fullmatch(r"[A-Za-z_][\w.]*", p.strip())]
            if idents and all(i in app_corpus for i in idents):
                row["parts"] = ", ".join(idents)
                compound.append(row)
            else:
                fabricated.append(row)

    return fabricated, compound, miscited


def md_escape(text: str) -> str:
    return str(text).replace("|", "\\|").replace("\n", " ")


def build(eval_name: str, source_root: Path) -> str:
    graph, gold_path = load_graph(eval_name)
    texts = collect_sources(graph, source_root)
    app_corpus = collect_app_corpus(texts, source_root)
    fabricated, compound, miscited = check_uno_values(graph, texts, app_corpus)

    nodes = graph.get("nodes", [])
    edges = graph.get("edges", [])
    unresolved = graph.get("unresolved", [])

    missing_sources = [rel for rel, body in texts.items() if not body]

    out: list[str] = []
    w = out.append

    w(f"# Gold review — `{eval_name}`")
    w("")
    w("**Reviewer:** _(name)_  ·  **Date:** _(YYYY-MM-DD)_  ·  **Gold:** "
      f"`{gold_path.relative_to(kit_root())}` ({len(nodes)} nodes / {len(edges)} edges / "
      f"{len(unresolved)} unresolved)")
    w("")
    w("> Independent review breaking the same-author circularity: every gold in this")
    w("> kit was authored and calibrated by one agent lineage. Record findings here;")
    w("> **do not edit gold directly** - accepted fixes go through")
    w("> `tools/build_graphs.py`, then `scripts/validate_graph.py`.")
    w("")

    w("## Read in this order")
    w("")
    w(f"1. `evals/{eval_name}/fixture.md` — the source list this gold was authored from")
    w("2. The actual source files it names (listed below)")
    w("3. `SKILL.md` — Pass 8 ID grammar and naming vocabulary")
    w("4. `references/ontology.md` — node/edge definitions, state scope and attachment")
    w(f"5. `evals/{eval_name}/README.md` — the altitude contract for this eval")
    w("")

    w("## Sources cited by this gold")
    w("")
    w("| Source file | Found | Nodes citing it |")
    w("|---|---|---|")
    counts: dict[str, int] = {}
    for node in nodes:
        rel = (node.get("evidence") or {}).get("source", {}).get("path")
        if rel:
            counts[rel] = counts.get(rel, 0) + 1
    for rel in sorted(counts):
        w(f"| `{rel}` | {'yes' if texts.get(rel) else '**NO**'} | {counts[rel]} |")
    w("")
    if missing_sources:
        w("> **Warning:** the files marked NO were not found under the source root, so")
        w("> the automated checks below could not verify anything attributed to them.")
        w("")

    w("## Automated pre-pass — identifiers not found in the cited source")
    w("")
    w("Every `properties.uno` value that must be a verbatim quotation from source")
    w("(names, style keys, resource keys, classes, members, glyphs) was checked")
    w("against the text of every source file this gold cites. The copy-don't-coin")
    w("contract says each one should appear literally.")
    w("")
    if not texts or all(not t for t in texts.values()):
        w("_Skipped: no source files were readable under the given source root._")
    else:
        if fabricated:
            w(f"### Not found anywhere in the application ({len(fabricated)}) — treat as fabrication")
            w("")
            w("These identifiers appear in no `.xaml` or `.cs` file of the app. Under")
            w("copy-don't-coin an invented identifier is a defect, not a near-miss.")
            w("")
            w("| Node | uno key | Value | Cited source |")
            w("|---|---|---|---|")
            for f in fabricated:
                w(f"| `{f['node']}` | `{f['key']}` | `{md_escape(f['value'])}` | `{md_escape(f['cited'])}` |")
        else:
            w("**No fabricated identifiers.** Every quoted uno value exists in the application.")
        w("")
        if compound:
            w(f"### Prose in a field that should hold one identifier ({len(compound)})")
            w("")
            w("Every identifier inside these strings exists in the app, so the mapping")
            w("is right — but the field packs two names, or a name plus a gloss, which")
            w("means nothing downstream can consume it as a quotation. Decide whether")
            w("the value should be split, or the field's contract loosened.")
            w("")
            w("| Node | uno key | Value | Identifiers found | Cited as |")
            w("|---|---|---|---|---|")
            for f in compound:
                w(f"| `{f['node']}` | `{f['key']}` | `{md_escape(f['value'])}` | "
                  f"`{md_escape(f.get('parts',''))}` | `{md_escape(f['cited'])}` |")
            w("")
        if miscited:
            w(f"### Real, but not provable from the node's own citation ({len(miscited)})")
            w("")
            w("The value exists in the app, but the node's `evidence.source` does not")
            w("point at a file containing it — commonly because the source is given as")
            w("a glob label rather than a path. The mapping is right; the provenance")
            w("is unverifiable, which is what makes this worth a reviewer's attention.")
            w("")
            w("| Node | uno key | Value | Cited as |")
            w("|---|---|---|---|")
            for f in miscited:
                w(f"| `{f['node']}` | `{f['key']}` | `{md_escape(f['value'])}` | `{md_escape(f['cited'])}` |")
            w("")

    w("## Structural facts")
    w("")
    w("Stated as counts, not judgements — the altitude contract decides which of")
    w("these are correct, and that call belongs to the reviewer.")
    w("")
    type_counts: dict[str, int] = {}
    for node in nodes:
        type_counts[node.get("type", "?")] = type_counts.get(node.get("type", "?"), 0) + 1
    w("| Node type | In gold |")
    w("|---|---|")
    for t in sorted(type_counts, key=lambda k: -type_counts[k]):
        w(f"| `{t}` | {type_counts[t]} |")
    for t in ("screen", "region", "component", "control", "content", "asset", "token", "state"):
        if t not in type_counts:
            w(f"| `{t}` | **0** |")
    w("")
    edge_counts: dict[str, int] = {}
    for e in edges:
        edge_counts[e.get("relation", "?")] = edge_counts.get(e.get("relation", "?"), 0) + 1
    w("| Relation | In gold |")
    w("|---|---|")
    for t in sorted(edge_counts, key=lambda k: -edge_counts[k]):
        w(f"| `{t}` | {edge_counts[t]} |")
    w("")
    # Layout containers in the cited XAML, as a counterweight to the node counts:
    # a screen whose source nests many panels but whose graph has no region
    # nodes may be respecting the altitude contract, or may be losing structure.
    container_re = re.compile(r"<(Grid|StackPanel|utu:AutoLayout|RelativePanel|Canvas)[\s>]")
    containers = sum(len(container_re.findall(body)) for body in texts.values())
    if containers:
        w(f"The cited XAML declares **{containers}** layout containers "
          f"(`Grid`/`StackPanel`/`AutoLayout`/…) across {len([t for t in texts.values() if t])} file(s).")
        w("Compare against the `region` count above when judging check 6.")
        w("")

    w("## Checks only a human can make")
    w("")
    w("For each, mark **ok** / **finding** and add a line of evidence.")
    w("")
    w("| # | Check | Verdict | Note |")
    w("|---|---|---|---|")
    w("| 1 | Every node and edge is evidence-backed by the named source — spot-check rationales against the XAML/C# | | |")
    w("| 2 | Every source-backed component reference is expanded (the PageHeader lesson) — no declared `UserControl` left as an opaque region | | |")
    w("| 3 | Altitude respected: no style-level hover/pressed/disabled states, tokens screen-scoped, canonical internals as properties not per-instance nodes | | |")
    w("| 4 | `properties.uno` values copied exactly from source (keys, x:Names, types) | | |")
    w("| 5 | `unresolved` items are genuinely undecidable from the source, not laziness | | |")
    w("| 6 | Structure the graph omits: does any real arrangement in the source go unrepresented? | | |")
    w("")

    w("## Nodes")
    w("")
    w("| Node id | Type | Name | Evidence | Conf | Uno mapping | Verdict | Note |")
    w("|---|---|---|---|---|---|---|---|")
    for node in nodes:
        ev = node.get("evidence") or {}
        uno = ((node.get("properties") or {}).get("uno")) or {}
        uno_s = ", ".join(f"{k}={v}" for k, v in uno.items()) if uno else "—"
        w(
            f"| `{node['id']}` | {node.get('type','')} | {md_escape(node.get('name') or node.get('text') or '')} "
            f"| {ev.get('kind','')} | {ev.get('confidence','')} | {md_escape(uno_s)} | | |"
        )
    w("")

    w("## Edges")
    w("")
    w("Behavioral edges (`triggers`, `navigates-to`) carry the most risk: they are")
    w("the ones a graph must never invent. They are listed first.")
    w("")
    behavioral = [e for e in edges if e.get("relation") in ("triggers", "navigates-to")]
    structural = [e for e in edges if e.get("relation") not in ("triggers", "navigates-to")]
    w("| Relation | From | To | Evidence | Verdict | Note |")
    w("|---|---|---|---|---|---|")
    for e in behavioral + structural:
        ev = e.get("evidence") or {}
        w(f"| **{e.get('relation','')}** | `{e.get('from','')}` | `{e.get('to','')}` | {ev.get('kind','')} | | |")
    w("")

    if unresolved:
        w("## Unresolved items")
        w("")
        w("| Question | Related ids | Genuinely undecidable? | Note |")
        w("|---|---|---|---|")
        for u in unresolved:
            rel = ", ".join(f"`{r}`" for r in u.get("relatedIds", []))
            w(f"| {md_escape(u.get('question',''))} | {rel} | | |")
        w("")

    w("## Verdict")
    w("")
    w("_Overall assessment, and whether the gold is fit to remain the answer key._")
    w("")
    w("Findings accepted: _(list)_")
    w("Findings rejected: _(list, with reasons)_")
    w("")
    return "\n".join(out) + "\n"


def main() -> int:
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("eval_name", help="eval folder name, e.g. 05-orbital-settings")
    ap.add_argument(
        "--source-root",
        default=str(kit_root().parent),
        help="root the graph's source paths are relative to (default: the repo containing the kit)",
    )
    ap.add_argument("--out", default=None, help="output path (default: evals/<eval>/gold-review.md)")
    args = ap.parse_args()

    text = build(args.eval_name, Path(args.source_root))
    out = Path(args.out) if args.out else kit_root() / "evals" / args.eval_name / "gold-review.md"
    out.write_text(text, encoding="utf-8")
    print(f"wrote {out}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
