#!/usr/bin/env python3
"""Stability analysis over the blind replication runs.

- scores each blind run against the gold graph (macro F1 + hallucination proxy)
- pairwise macro F1 across runs (drift between fresh-context generations)
- concept-stability table: which canonical node ids appear in how many runs
"""
import itertools, json, pathlib, subprocess, sys

KIT = pathlib.Path("/home/user/Uno-Builds/design-graph-kit")
import sys as _sys
_EVAL = _sys.argv[2] if len(_sys.argv) > 2 else "05-orbital-settings"
BLIND = KIT / ("evals/" + _EVAL + "/" + (_sys.argv[1] if len(_sys.argv) > 1 else "blind"))
GOLD = KIT / ("evals/" + _EVAL + "/gold.graph.json")

runs = sorted(BLIND.glob("run*.graph.json"))
if len(runs) < 2:
    sys.exit(f"need >=2 runs, found {len(runs)}")

def score(gold, pred):
    out = subprocess.run(
        [sys.executable, str(KIT / "scripts/score_graph.py"), str(gold), str(pred), "--json"],
        capture_output=True, text=True, check=True)
    return json.loads(out.stdout)

print("== vs gold ==")
vs_gold = {}
for r in runs:
    m = score(GOLD, r)
    vs_gold[r.name] = m
    concept = m.get('node_concept', {}).get('f1', float('nan'))
    print(f"{r.name}: macro={m['macro_f1']:.4f} nodes={m['node_id']['f1']:.3f} "
          f"concept={concept:.3f} uno={m.get('uno_mapping',{}).get('f1',float('nan')):.3f} edges={m['edge']['f1']:.3f} unres={m['unresolved']['f1']:.3f} "
          f"halluc={m['severe_hallucination_proxy']} "
          f"(n={m['node_id']['generated_count']}, e={m['edge']['generated_count']})")

print("\n== pairwise (drift between runs) ==")
pair_scores = []
for a, b in itertools.combinations(runs, 2):
    m = score(a, b)
    pair_scores.append(m["macro_f1"])
    print(f"{a.name} <> {b.name}: macro={m['macro_f1']:.4f} nodes={m['node_id']['f1']:.3f} concept={m.get('node_concept',{}).get('f1',float('nan')):.3f} edges={m['edge']['f1']:.3f}")
mean_pair = sum(pair_scores) / len(pair_scores)
mean_gold = sum(v["macro_f1"] for v in vs_gold.values()) / len(vs_gold)
print(f"\nmean vs-gold macro F1: {mean_gold:.4f}")
print(f"mean pairwise macro F1: {mean_pair:.4f} (min {min(pair_scores):.4f}, max {max(pair_scores):.4f})")

print("\n== concept stability (node id appearance across runs) ==")
from collections import Counter
counts = Counter()
for r in runs:
    g = json.loads(r.read_text())
    for n in g.get("nodes", []):
        counts[n["id"]] += 1
n_runs = len(runs)
in_all = sorted(i for i, c in counts.items() if c == n_runs)
partial = sorted((i, c) for i, c in counts.items() if 1 < c < n_runs)
singles = sorted(i for i, c in counts.items() if c == 1)
print(f"in all {n_runs} runs: {len(in_all)}")
print(f"partial ({2}..{n_runs-1} runs): {len(partial)}")
for i, c in partial:
    print(f"  {c}/{n_runs}: {i}")
print(f"in exactly 1 run: {len(singles)}")
for i in singles:
    print(f"  1/{n_runs}: {i}")
