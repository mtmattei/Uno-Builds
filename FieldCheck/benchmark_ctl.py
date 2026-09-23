#!/usr/bin/env python3
import json, sys
from pathlib import Path
from datetime import datetime, timezone

out = Path('results')
out.mkdir(exist_ok=True)
log = out / 'timing.jsonl'

def now():
    return datetime.now(timezone.utc).isoformat()

def emit(event):
    rec = {"timestamp_utc": now(), "event": event}
    with log.open('a', encoding='utf-8') as f:
        f.write(json.dumps(rec) + "\n")
    print(json.dumps(rec))

if len(sys.argv) < 2:
    raise SystemExit('usage: python benchmark_ctl.py start|mark <name>|finish')
cmd = sys.argv[1]
if cmd == 'start': emit('run_start')
elif cmd == 'finish': emit('run_finish')
elif cmd == 'mark' and len(sys.argv) == 3: emit(sys.argv[2])
else: raise SystemExit('invalid command')
