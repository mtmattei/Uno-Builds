#!/usr/bin/env python3
"""Send a cross-model review bundle to a non-Claude model and save the reply.

The point of a cross-model review is breaking lineage correlation: this kit's
golds, rules and checkers all came from one model family, so a checker from
that family inherits its blind spots instead of catching them. Reviewing with
another Claude model would not fix that. A different vendor would.

Two providers, neither needing an API key:

  codex   - the Codex CLI, authenticated through its own sign-in. Runs with
            `-s read-only` so the reviewer can consult the real repository
            while being unable to modify it. Default, because it can verify
            the bundle against source rather than trusting it.
  vertex  - Gemini through Vertex AI, using the gcloud CLI's credentials.
            Needs `gcloud auth login` if the token has expired; that step is
            interactive and cannot be scripted from here.

Usage:
  python3 tools/run_cross_review.py 05-orbital-settings
  python3 tools/run_cross_review.py 05-orbital-settings --provider vertex
  python3 tools/run_cross_review.py 05-orbital-settings --model gpt-5.1-codex
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


def run_codex(bundle: Path, model: str | None, timeout: int) -> str:
    """Review via the Codex CLI, read-only, prompt on stdin.

    Read-only rather than sandboxed-off: the reviewer is *encouraged* to open
    the real files and check the bundle against them, and structurally unable
    to change anything while doing it.
    """
    exe = "codex.cmd" if sys.platform == "win32" else "codex"
    cmd = [exe, "exec", "-s", "read-only", "-C", str(kit_root().parent)]
    if model:
        cmd += ["-m", model]
    cmd.append("-")  # read the prompt from stdin

    prompt = bundle.read_text(encoding="utf-8")
    print(f"sending {len(prompt) // 4:,} tokens to codex ({model or 'default model'})...")
    try:
        out = subprocess.run(cmd, input=prompt, capture_output=True, text=True,
                             timeout=timeout, encoding="utf-8", errors="replace")
    except FileNotFoundError:
        sys.exit("codex not found on PATH. Install it, or use --provider vertex.")
    except subprocess.TimeoutExpired:
        sys.exit(f"codex timed out after {timeout}s. Raise --timeout or review a smaller eval.")
    if out.returncode != 0:
        sys.exit(f"codex exited {out.returncode}:\n{(out.stderr or '')[:2000]}")
    return out.stdout


def main() -> int:
    ap = argparse.ArgumentParser(description=__doc__,
                                 formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("eval_name")
    ap.add_argument("--provider", choices=("codex", "vertex"), default="codex")
    ap.add_argument("--model", default=None,
                    help="provider default if unset (vertex: %s)" % DEFAULT_MODEL)
    ap.add_argument("--location", default=DEFAULT_LOCATION)
    ap.add_argument("--project", default=None, help="defaults to the active gcloud project")
    ap.add_argument("--bundle", default=None)
    ap.add_argument("--out", default=None)
    ap.add_argument("--timeout", type=int, default=1800)
    args = ap.parse_args()

    eval_dir = kit_root() / "evals" / args.eval_name
    bundle = Path(args.bundle) if args.bundle else eval_dir / "cross-model-review-bundle.md"
    if not bundle.exists():
        sys.exit(f"No bundle at {bundle}. Run: python3 tools/build_cross_review.py {args.eval_name}")

    if args.provider == "codex":
        text = run_codex(bundle, args.model, args.timeout)
        reviewer = f"codex ({args.model or 'default model'})"
        out = Path(args.out) if args.out else eval_dir / "cross-model-review-codex.md"
        header = (f"# Cross-model review — {args.eval_name}\n\n"
                  f"**Reviewer:** {reviewer}, read-only against the repo  ·  "
                  f"**Bundle:** `{bundle.name}`\n\n"
                  "Produced to break lineage correlation: this kit's golds and checkers were "
                  "all authored by one model family, so its blind spots are invisible to its "
                  "own tooling. Findings below are unedited.\n\n---\n\n")
        out.write_text(header + text, encoding="utf-8")
        print(f"wrote {out}  ({len(text) // 4:,} tokens of findings)")
        return 0

    model = args.model or DEFAULT_MODEL
    project = args.project or gcloud("config", "get-value", "project")
    if not project or project == "(unset)":
        sys.exit("No gcloud project set. Run:  gcloud config set project <id>")
    token = gcloud("auth", "print-access-token")

    prompt = bundle.read_text(encoding="utf-8")
    url = (f"https://{args.location}-aiplatform.googleapis.com/v1/projects/{project}"
           f"/locations/{args.location}/publishers/google/models/{model}:generateContent")
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

    print(f"sending {len(prompt) // 4:,} tokens to {model} (project {project})...")
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

    out = Path(args.out) if args.out else eval_dir / f"cross-model-review-{model}.md"
    header = (f"# Cross-model review — {args.eval_name}\n\n"
              f"**Reviewer:** {model} (via Vertex AI, project {project})  ·  "
              f"**Bundle:** `{bundle.name}`\n\n"
              "Produced to break lineage correlation: this kit's golds and checkers were "
              "all authored by one model family, so its blind spots are invisible to its "
              "own tooling. Findings below are unedited.\n\n---\n\n")
    out.write_text(header + text, encoding="utf-8")
    print(f"wrote {out}  ({len(text) // 4:,} tokens of findings)")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
