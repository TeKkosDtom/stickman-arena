#!/usr/bin/env python3
"""Lee los XML de NUnit que deja el Test Runner de Unity y escribe solo el resumen y los fallos.
Uso: python3 tools/ci/test-summary.py <carpeta con *.xml>
Sale con código 1 si hubo fallos."""
import glob, os, sys, xml.etree.ElementTree as ET

folder = sys.argv[1] if len(sys.argv) > 1 else "TestResults"
files = glob.glob(f"{folder}/**/*.xml", recursive=True)
# En GitHub Actions, además del log, se emiten anotaciones (::error::) que se pueden leer
# por la API de checks sin descargar logs ni artefactos.
IN_ACTIONS = os.environ.get("GITHUB_ACTIONS") == "true"
def annotate(level, title, msg):
    if IN_ACTIONS:
        print(f"::{level} title={title}::{msg.replace(chr(10), ' ')[:900]}")

if not files:
    print(f"Sin resultados en {folder}")
    annotate("error", "Sin resultados de pruebas", "Unity no generó XML de resultados: suele ser un error de compilación (buscar 'error CS' en el log del paso anterior).")
    sys.exit(0)

total = passed = failed = skipped = 0
failures = []
for f in files:
    root = ET.parse(f).getroot()
    for case in root.iter("test-case"):
        total += 1
        r = case.get("result", "")
        if r == "Passed": passed += 1
        elif r == "Skipped": skipped += 1
        elif r == "Failed":
            failed += 1
            msg = case.findtext("failure/message", "").strip().splitlines()
            failures.append((case.get("fullname"), msg[0] if msg else ""))
print(f"Pruebas: {total} total, {passed} ok, {failed} fallidas, {skipped} omitidas")
annotate("notice" if not failed else "error", "Resumen de pruebas", f"{total} total, {passed} ok, {failed} fallidas, {skipped} omitidas")
for name, msg in failures[:20]:
    print(f"  FALLÓ {name}: {msg[:300]}")
    annotate("error", "Prueba fallida", f"{name}: {msg[:300]}")
if len(failures) > 20: print(f"  ... y {len(failures) - 20} más (ver el artefacto resultados-pruebas)")
sys.exit(1 if failed else 0)
