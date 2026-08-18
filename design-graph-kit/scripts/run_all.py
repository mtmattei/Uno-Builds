#!/usr/bin/env python3
"""Validate all gold graphs and optionally score generated graphs when present.

Also guards against absolute paths in the tools, which has bitten this kit four
times: builders and analysis scripts were written with a `/home/user/...`
sandbox path, so on another machine they either failed outright or - worse -
silently wrote to a stray directory, printed a success line, and left the real
gold untouched. A validator run afterwards then passed, because it was checking
the unchanged file. `check_tool_paths` fails loudly instead.
"""

from __future__ import annotations
import re
import subprocess
from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[1]
EVALS = ROOT / "evals"

# Absolute POSIX roots and Windows drive paths, as string literals in source.
_ABS_PATH = re.compile(r'["\'](?:/(?:home|Users|tmp|var|mnt)/|[A-Za-z]:[\\/])[^"\']*["\']')


def check_tool_paths() -> int:
    """Fail if any tool or script hardcodes an absolute filesystem path."""
    offenders: list[str] = []
    for folder in ("tools", "scripts"):
        for path in sorted((ROOT / folder).glob("*.py")):
            for lineno, line in enumerate(path.read_text(encoding="utf-8").splitlines(), 1):
                stripped = line.strip()
                if stripped.startswith("#") or "noqa: abspath" in stripped:
                    continue
                if _ABS_PATH.search(line):
                    offenders.append(f"{folder}/{path.name}:{lineno}: {stripped[:88]}")
    if offenders:
        print("FAIL: hardcoded absolute paths (these break on any other machine):")
        for o in offenders:
            print("  -", o)
        print("  Use Path(__file__).resolve().parents[N] instead.")
        return 1
    print("PASS: no hardcoded absolute paths in tools/ or scripts/")
    return 0


def run(cmd):
    print("$", " ".join(map(str, cmd)))
    return subprocess.run(cmd, cwd=ROOT).returncode

def main():
    rc = check_tool_paths()
    print()
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
