#!/usr/bin/env python3
"""Validate all gold graphs and optionally score generated graphs when present."""

from __future__ import annotations
import subprocess
from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[1]
EVALS = ROOT / "evals"

def run(cmd):
    print("$", " ".join(map(str, cmd)))
    return subprocess.run(cmd, cwd=ROOT).returncode

def main():
    rc = 0
    for case in sorted(p for p in EVALS.iterdir() if p.is_dir()):
        gold = case / "gold.graph.json"
        generated = case / "generated.graph.json"
        if gold.exists():
            rc |= run([sys.executable, "scripts/validate_graph.py", str(gold)])
        if generated.exists():
            rc |= run([sys.executable, "scripts/validate_graph.py", str(generated)])
            rc |= run([sys.executable, "scripts/score_graph.py", str(gold), str(generated)])
        print()
    return rc

if __name__ == "__main__":
    raise SystemExit(main())
