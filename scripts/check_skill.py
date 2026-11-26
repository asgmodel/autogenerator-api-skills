"""Structural checks for skill links, source examples and minimal ontology."""
from pathlib import Path
import json
import re

root = Path(__file__).resolve().parents[1]
skill = root / "skills" / "autogenerator-api-skills"
errors = []
def require(test, message):
    if not test:
        errors.append(message)

entry = (skill / "SKILL.md").read_text(encoding="utf-8")
require(entry.startswith("---\nname: autogenerator-api-skills\n"), "skill frontmatter/name mismatch")
require(re.search(r"^description: .+", entry, re.M) is not None, "missing description")
for path in [root / "README.md", *skill.rglob("*.md")]:
    for link in re.findall(r"\]\(([^)]+)\)", path.read_text(encoding="utf-8")):
        if link.startswith(("https://", "http://", "#", "mailto:")):
            continue
        target = link.split("#", 1)[0]
        require(not target or (path.parent / target).exists(), f"{path.name}: missing link {target}")
ontology = json.loads((skill / "references" / "architecture.jsonld").read_text())
graph = {n["@id"]: n for n in ontology["@graph"]}
require(len(graph) == len(ontology["@graph"]), "duplicate node IDs")
for id_, node in graph.items():
    for rel in ("dependsOn", "uses", "generates", "requiresCheck", "lifetime"):
        targets = node.get(rel, [])
        if isinstance(targets, str):
            targets = [targets]
        require(len(set(targets)) == len(targets), f"{id_}: duplicate {rel}")
        for target in targets:
            require(target in graph, f"{id_}: dangling {rel} {target}")
    if "reference" in node:
        require((skill / node["reference"].split("#", 1)[0]).is_file(), f"{id_}: missing reference")
seen, active = set(), set()
def visit(id_):
    if id_ in active:
        errors.append("dependency cycle: " + id_)
        return
    if id_ in seen or id_ not in graph:
        return
    active.add(id_)
    deps = graph[id_].get("dependsOn", [])
    for dep in [deps] if isinstance(deps, str) else deps:
        visit(dep)
    active.remove(id_)
    seen.add(id_)
for id_ in graph:
    visit(id_)
count = 0
for path in sorted((skill / "assets" / "examples" / "validators").glob("*Validator.cs.txt")):
    code = path.read_text()
    enum = re.search(r"enum (\w+)\s*\{([^}]+)\}", code)
    require(enum is not None, f"{path.name}: no enum")
    if not enum:
        continue
    states = [s.strip().split("=")[0].strip() for s in enum[2].split(",") if s.strip()]
    bindings = re.findall(r"RegisterConditionValidator\(typeof\(" + enum[1] + r"\),\s*" + enum[1] + r"\.(\w+)", code)
    require(sorted(states) == sorted(bindings), f"{path.name}: enum/attribute mismatch")
    require(len(set(bindings)) == len(bindings), f"{path.name}: duplicate binding")
    require(not re.search(r'ToFailure(?:Async)?\(\s*"[^"]*"\s*\)', code), f"{path.name}: message in Result")
    require(not re.search(r"__[A-Z_]+__", code), f"{path.name}: unresolved tokens")
    count += len(bindings)
template = (skill / "assets" / "templates" / "ValidatorContext.cs.txt").read_text()
require(not re.search(r'ToFailureAsync\(\s*"[^"]*"\s*\)', template), "template failure factory mismatch")
if errors:
    raise SystemExit("\n".join(errors))
print(f"PASS: skill structure, links, {len(graph)} ontology nodes and {count} example state bindings")
