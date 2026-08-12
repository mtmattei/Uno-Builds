#!/usr/bin/env python3
"""Basic deterministic comparison of a generated Design Graph to a gold graph.

This is intentionally not a replacement for semantic human review.
It measures exact/canonical overlap so regressions and stability drift are visible.
"""

from __future__ import annotations
import argparse
import json
from pathlib import Path


def load(path: Path):
    with path.open("r", encoding="utf-8") as f:
        return json.load(f)


def f1(gold: set, pred: set):
    if not gold and not pred:
        return 1.0, 1.0, 1.0
    precision = len(gold & pred) / len(pred) if pred else 0.0
    recall = len(gold & pred) / len(gold) if gold else 1.0
    score = (2 * precision * recall / (precision + recall)) if (precision + recall) else 0.0
    return precision, recall, score


def node_signature(n):
    return (
        n.get("id"),
        n.get("type"),
        n.get("role"),
        n.get("semanticRole"),
        n.get("category"),
    )


def edge_signature(e):
    return (e.get("from"), e.get("relation"), e.get("to"))


def unresolved_signature(u):
    return (u.get("question"), tuple(sorted(u.get("relatedIds", []))))


def round4(x):
    return round(float(x), 4)


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("gold", type=Path)
    parser.add_argument("generated", type=Path)
    parser.add_argument("--json", action="store_true", help="Emit machine-readable JSON")
    args = parser.parse_args()

    gold = load(args.gold)
    pred = load(args.generated)

    metrics = {}

    dimensions = {
        "node_id": (
            {n["id"] for n in gold.get("nodes", [])},
            {n["id"] for n in pred.get("nodes", [])},
        ),
        "node_signature": (
            {node_signature(n) for n in gold.get("nodes", [])},
            {node_signature(n) for n in pred.get("nodes", [])},
        ),
        "edge": (
            {edge_signature(e) for e in gold.get("edges", [])},
            {edge_signature(e) for e in pred.get("edges", [])},
        ),
        "unresolved": (
            {unresolved_signature(u) for u in gold.get("unresolved", [])},
            {unresolved_signature(u) for u in pred.get("unresolved", [])},
        ),
    }

    f1s = []
    for name, (g, p) in dimensions.items():
        precision, recall, score = f1(g, p)
        metrics[name] = {
            "precision": round4(precision),
            "recall": round4(recall),
            "f1": round4(score),
            "gold_count": len(g),
            "generated_count": len(p),
        }
        f1s.append(score)

    metrics["macro_f1"] = round4(sum(f1s) / len(f1s))

    # A simple hallucination proxy: behavioral edges absent from gold.
    gold_behavior = {
        edge_signature(e) for e in gold.get("edges", [])
        if e.get("relation") in {"navigates-to", "triggers"}
    }
    pred_behavior = {
        edge_signature(e) for e in pred.get("edges", [])
        if e.get("relation") in {"navigates-to", "triggers"}
    }
    unsupported_behavior = sorted(pred_behavior - gold_behavior)
    metrics["unsupported_behavior_edges"] = unsupported_behavior
    metrics["severe_hallucination_proxy"] = bool(unsupported_behavior)

    if args.json:
        print(json.dumps(metrics, indent=2))
    else:
        print(f"Macro F1: {metrics['macro_f1']:.4f}")
        for name in ("node_id", "node_signature", "edge", "unresolved"):
            m = metrics[name]
            print(
                f"{name:16} "
                f"P={m['precision']:.4f} R={m['recall']:.4f} F1={m['f1']:.4f} "
                f"(gold={m['gold_count']}, generated={m['generated_count']})"
            )
        if unsupported_behavior:
            print("WARNING: unsupported behavior edges:")
            for edge in unsupported_behavior:
                print("  -", edge)


if __name__ == "__main__":
    main()
