#!/usr/bin/env python3
"""Static verification of an experiment arm (no compiler available):
1. XML well-formedness of every .xaml file.
2. Resource resolution: every {StaticResource X} / {ThemeResource X} referenced
   in the arm's files must be defined by an x:Key in the arm's files.
3. Event-handler resolution: every Click/Loaded handler named in XAML must
   exist in the code-behind.
"""
import re, sys, pathlib
import xml.etree.ElementTree as ET

arm = pathlib.Path(sys.argv[1])
errors, notes = [], []

xamls = sorted(arm.glob("*.xaml"))
cs = "\n".join(p.read_text() for p in arm.glob("*.cs"))

defined, referenced, handlers = set(), set(), set()
for x in xamls:
    text = x.read_text()
    try:
        ET.fromstring(text)
        notes.append(f"well-formed: {x.name}")
    except ET.ParseError as e:
        errors.append(f"XML parse error in {x.name}: {e}")
    defined |= set(re.findall(r'x:Key="([^"]+)"', text))
    referenced |= set(re.findall(r'\{(?:StaticResource|ThemeResource)\s+([^}]+)\}', text))
    handlers |= set(re.findall(r'\b(?:Click|Loaded|Tapped|PointerEntered|PointerExited)="([A-Za-z_]\w*)"', text))

missing = sorted(r for r in referenced if r not in defined)
if missing:
    errors.append(f"unresolved resource keys ({len(missing)}): {', '.join(missing)}")
else:
    notes.append(f"all {len(referenced)} referenced resource keys resolve to {len(defined)} defined keys")

missing_handlers = sorted(h for h in handlers if not re.search(rf'\b{h}\s*\(', cs))
if missing_handlers:
    errors.append(f"handlers named in XAML but missing in code-behind: {', '.join(missing_handlers)}")
elif handlers:
    notes.append(f"all {len(handlers)} XAML event handlers exist in code-behind")

print(f"== {arm.name} ==")
for n in notes:
    print("  ok:", n)
for e in errors:
    print("  FAIL:", e)
sys.exit(1 if errors else 0)
