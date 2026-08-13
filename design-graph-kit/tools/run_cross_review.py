#!/usr/bin/env python3
"""Send a cross-model review bundle to Gemini via Vertex AI and save the reply.

The point of a cross-model review is breaking lineage correlation: this kit's
golds, rules and checkers all came from one model family, so a checker from
that family inherits its blind spots instead of catching them. Reviewing with
another Claude model would not fix that. Gemini would.

Auth: uses the gcloud CLI's own credentials, so there is no API key to manage.
If the token has expired, run `gcloud auth login` first - that step is
interactive and cannot be scripted from here.

Usage:
  python3 tools/run_cross_review.py 05-orbital-settings
  python3 tools/run_cross_review.py 05-orbital-settings --model gemini-2.5-pro
"""

from __future__ import annotations

import argparse
import json
import subprocess
import sys
import urllib.error
import urllib.request
from pathlib import Path

DEFAULT_MODEL = "gemini-2.5-pro"
DEFAULT_LOCATION = "us-central1"


def kit_root() -> Path:
    return Path(__file__).resolve().parents[1]


def gcloud(*args: str) -> str:
    exe = "gcloud.cmd" if sys.platform == "win32" else "gcloud"
    try:
        out = subprocess.run([exe, *args], capture_output=True, text=True, check=True)
    except FileNotFoundError:
        sys.exit("gcloud not found on PATH.")
    except subprocess.CalledProcessError as exc:
        detail = (exc.stderr or "").strip()
        if "invalid_grant" in detail or "expired or revoked" in detail:
            sys.exit("gcloud credentials have expired. Run:  gcloud auth login")
        sys.exit(f"gcloud {' '.join(args)} failed:\n{detail}")
    return out.stdout.strip()


def main() -> int:
    ap = argparse.ArgumentParser(description=__doc__,
                                 formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("eval_name")
    ap.add_argument("--model", default=DEFAULT_MODEL)
    ap.add_argument("--location", default=DEFAULT_LOCATION)
    ap.add_argument("--project", default=None, help="defaults to the active gcloud project")
    ap.add_argument("--bundle", default=None)
    ap.add_argument("--out", default=None)
    args = ap.parse_args()

    eval_dir = kit_root() / "evals" / args.eval_name
    bundle = Path(args.bundle) if args.bundle else eval_dir / "cross-model-review-bundle.md"
    if not bundle.exists():
        sys.exit(f"No bundle at {bundle}. Run: python3 tools/build_cross_review.py {args.eval_name}")

    project = args.project or gcloud("config", "get-value", "project")
    if not project or project == "(unset)":
        sys.exit("No gcloud project set. Run:  gcloud config set project <id>")
    token = gcloud("auth", "print-access-token")

    prompt = bundle.read_text(encoding="utf-8")
    url = (f"https://{args.location}-aiplatform.googleapis.com/v1/projects/{project}"
           f"/locations/{args.location}/publishers/google/models/{args.model}:generateContent")
    payload = {
        "contents": [{"role": "user", "parts": [{"text": prompt}]}],
        # Low temperature: this is an audit, not a brainstorm.
        "generationConfig": {"temperature": 0.2, "maxOutputTokens": 16384},
    }

    req = urllib.request.Request(
        url,
        data=json.dumps(payload).encode("utf-8"),
        headers={"Authorization": f"Bearer {token}", "Content-Type": "application/json"},
    )

    print(f"sending {len(prompt) // 4:,} tokens to {args.model} (project {project})...")
    try:
        with urllib.request.urlopen(req, timeout=600) as resp:
            body = json.loads(resp.read().decode("utf-8"))
    except urllib.error.HTTPError as exc:
        detail = exc.read().decode("utf-8", errors="replace")[:2000]
        if exc.code == 403:
            detail += ("\n\nHint: the Vertex AI API may not be enabled for this project:\n"
                       "  gcloud services enable aiplatform.googleapis.com")
        sys.exit(f"Vertex AI returned {exc.code}:\n{detail}")

    try:
        text = body["candidates"][0]["content"]["parts"][0]["text"]
    except (KeyError, IndexError):
        sys.exit(f"Unexpected response shape:\n{json.dumps(body)[:2000]}")

    out = Path(args.out) if args.out else eval_dir / f"cross-model-review-{args.model}.md"
    header = (f"# Cross-model review — {args.eval_name}\n\n"
              f"**Reviewer:** {args.model} (via Vertex AI, project {project})  ·  "
              f"**Bundle:** `{bundle.name}`\n\n"
              "Produced to break lineage correlation: this kit's golds and checkers were "
              "all authored by one model family, so its blind spots are invisible to its "
              "own tooling. Findings below are unedited.\n\n---\n\n")
    out.write_text(header + text, encoding="utf-8")
    print(f"wrote {out}  ({len(text) // 4:,} tokens of findings)")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
