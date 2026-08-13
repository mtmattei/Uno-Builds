#!/usr/bin/env python3
"""Design-first pilot audit: honesty checks + structural recall vs the
source-backed gold. The agents saw only brief.md — never the Orbital source —
so:
  1. behavioral edges (triggers/navigates-to) must be ZERO;
  2. no fabricated declared identifiers: any 'Orbital*' resource key /
     style key / x:Name in properties.uno would be a hallucination
     (unknowable from the brief); uno.type suggestions are fine;
  3. evidence kinds must never claim declared for behavior/mapping.
Then scores structure vs gold v1.4 (id/concept recall on the observable
subset is the interesting number; uno_mapping is expected ~0 by design).
"""
import json, glob, re, subprocess, sys, pathlib

KIT = pathlib.Path("/home/user/Uno-Builds/design-graph-kit")
GOLD = KIT / "evals/05-orbital-settings/gold.graph.json"

for f in sorted(glob.glob(str(KIT / "evals/05-orbital-settings/design-first/run*.graph.json"))):
    g = json.load(open(f))
    name = f.split("/")[-1]
    beh = [e for e in g["edges"] if e["relation"] in ("triggers", "navigates-to")]
    fabricated = []
    for n in g["nodes"]:
        uno = (n.get("properties") or {}).get("uno") or {}
        for k in ("resourceKey", "styleKey", "xName", "class"):
            v = uno.get(k)
            if v and re.match(r"^Orbital", str(v)):
                fabricated.append((n["id"], k, v))
        for k in ("resourceKey", "xName"):
            if uno.get(k):
                fabricated.append((n["id"], k, uno[k], "declared-identifier asserted from design-only input"))
    out = subprocess.run([sys.executable, str(KIT / "scripts/score_graph.py"), str(GOLD), f, "--json"],
                         capture_output=True, text=True, check=True)
    m = json.loads(out.stdout)
    print(f"{name}: nodes={len(g['nodes'])} edges={len(g['edges'])} unresolved={len(g.get('unresolved', []))}")
    print(f"  behavioral edges: {len(beh)} {'FAIL' if beh else 'ok'}")
    print(f"  fabricated declared identifiers: {len(fabricated)} {'FAIL ' + str(fabricated[:4]) if fabricated else 'ok'}")
    print(f"  vs source-backed gold: node-id R={m['node_id']['recall']:.3f} concept R={m['node_concept']['recall']:.3f} "
          f"edge R={m['edge']['recall']:.3f} uno F1={m['uno_mapping']['f1']:.3f} halluc={m['severe_hallucination_proxy']}")
