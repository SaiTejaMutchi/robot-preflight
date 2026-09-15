# Robot Preflight

Verify robot requirements against facility geometry before commissioning.

AMR deployment moves through facility layouts, robot specifications,
simulation models, OEM configuration, controls, commissioning, and finally
the physical floor. Each stage represents the same deployment differently.

Robot Preflight starts with one narrow question:

**Does this robot requirement agree with the physical geometry of this site?**

## Verified example

| Evidence | Value |
|---|---|
| Robot | OTTO 1500 |
| Constraint | Minimum one-way aisle width |
| Required | 1.915 m |
| Measured | 7.509663 m |
| Margin | +5.594663 m |
| Decision | **PASS** |
| Requirement source | OTTO 1500 manufacturer spec sheet, doc `OTTO-DS001D-EN-APR2024`, p.1 |
| Facility source | AWS RoboMaker Small Warehouse World (independently sourced, frozen revision) |
| Verification | Unity Play Mode + independent offline Python recomputation |
| Agreement | within 1 µm |

The requirement comes from the OTTO manufacturer specification. The
facility geometry comes from an independently sourced AWS RoboMaker
warehouse environment — not a warehouse built to pass. The measurement is
deterministic and was independently reproduced outside the main Unity
execution path. The two computations agree to within 1 micrometer.

[Run the example](#quickstart) · [Inspect the evidence](#dont-trust-the-screenshot-reproduce-it) · [See the architecture](#architecture)

## Demo

<video src="media/demo/robot_preflight.mp4" controls muted playsinline poster="media/screenshots/02_final_result.png" width="720"></video>

If the player above doesn't render: [`media/demo/robot_preflight.mp4`](media/demo/robot_preflight.mp4) (21 s, 2.2 MB, understandable muted).

This is supporting evidence, not the proof — the reproducible computation
below is the actual artifact.

## Why this exists

AMR deployment does not happen inside one system. The same deployment gets
represented as mission and throughput requirements, a facility layout or
map, a robot/attachment/payload definition, simulation assumptions, fleet
configuration, integration logic, commissioning evidence, and eventual
runtime behavior — usually in different tools, owned by different teams,
at different times. The technical problem is keeping those representations
consistent, and catching it explicitly when they disagree.

| Truth | Typical representation |
|---|---|
| What the operation needs | workflows, endpoints, throughput |
| What the facility contains | CAD, maps, scans, layouts |
| What the robot can do | robot, attachment, payload, constraints |
| What simulation predicts | routes, fleet size, traffic, throughput |
| What the fleet system is configured to do | zones, endpoints, workflows, rules |
| What actually happens | travel, docking, faults, interventions |

Robot Preflight currently operates between two of these:

**what the facility contains** and **what the robot can do**.

The first public primitive is a deterministic compatibility check between
them. The larger technical direction is to make more of these deployment
truths explicit, verifiable, and machine-readable — that direction is not
built yet. Full framing: [`docs/workflow-context.md`](docs/workflow-context.md).

## Install

```bash
git clone <this repository>
cd robot-preflight
pip install -e .
```

Pure-Python, stdlib + [PyYAML](https://pyyaml.org/) — no Unity, no GPU, no
license required to run the check itself.

## Quickstart

```bash
python -m robot_preflight check examples/otto1500_warehouse
```

This runs a fresh, independent Python recomputation, so it reports
**7.509662 m** — not the frozen Unity measurement of **7.509663 m** shown
in the table above. Both are genuine; they are two separate computations
of the same geometry, documented to agree within 1 µm (see
[Reproduce it](#dont-trust-the-screenshot-reproduce-it)), not a
discrepancy to resolve.

```
Robot Preflight

Robot        otto_1500
Constraint   aisle_clearance

Required     1.915000 m
Available    7.509662 m
Margin       +5.594662 m

Decision     PASS

Requirement source
OTTO 1500 Spec Sheet, doc OTTO-DS001D-EN-APR2024, p.1: ...

Facility source
AWS RoboMaker Small Warehouse World ...

Verification
Deterministic geometry: nearest-face world-space gap between the two named
entities' renderer bounds along the configured axis, computed by
re-parsing the facility .glb ... not a stored value.
```

Or from Python:

```python
from robot_preflight import Preflight

result = Preflight.check(
    robot="otto_1500",
    facility="examples/otto1500_warehouse",
    constraint="aisle_clearance",
)
print(result)
```

Both call the same real logic — they re-parse
[`examples/otto1500_warehouse/config.yaml`](examples/otto1500_warehouse/config.yaml)
and the facility `.glb` it points at, then recompute the measurement from
raw geometry. Neither reads a stored expected-output file back to you.

## Output contract

`available_m` below is the independent Python recomputation (7.509662 m),
not the frozen Unity measurement (7.509663 m) — see the note above.

```json
{
  "decision": "PASS",
  "robot": "otto_1500",
  "constraint": "aisle_clearance",
  "required_m": 1.915,
  "available_m": 7.509662,
  "margin_m": 5.594662,
  "requirement_source": "OTTO 1500 Spec Sheet, doc OTTO-DS001D-EN-APR2024, p.1 ...",
  "facility_source": "AWS RoboMaker Small Warehouse World ...",
  "facility_entities": [
    "aws_robomaker_warehouse_ShelfF_01_001",
    "aws_robomaker_warehouse_ShelfD_01_001"
  ],
  "verification_method": "Deterministic geometry ...",
  "evidence": {
    "geometry_file": "Assets/StreamingAssets/WarehouseGeometry/aws_independent_warehouse.glb",
    "geometry_file_bytes": 619192,
    "geometry_file_nodes": 30,
    "geometry_file_triangles": 8345,
    "axis": "X",
    "tolerance_m": 0.005,
    "facility_config": "examples/otto1500_warehouse/config.yaml"
  }
}
```

Get it with `python -m robot_preflight check examples/otto1500_warehouse --json`
or `result.to_dict()`. Every field is either a **source** citation, a
**measured** value from this run, or a **derived** value (margin = measured
− required); none of it is invented for display.

## What this proves

The selected warehouse span is wider than the published OTTO 1500 minimum
one-way aisle-width requirement.

## What this does not prove

This result does not establish:

- whole-facility readiness
- turning clearance
- docking compatibility
- doorway clearance
- route feasibility
- throughput
- safety certification
- deployment approval

It is one verified compatibility decision, for one robot, in one facility.

## Don't trust the screenshot. Reproduce it.

```bash
make verify
```

runs the real computation and checks it against the frozen record —
including source-file hashes, not just the final numbers:

```
Robot Preflight reference verification

OTTO source PDF hash         verified
Facility GLB hash            verified
Deterministic measurement (recomputed) verified
Required matches frozen record verified
Measured matches frozen record verified
Decision matches frozen record verified

Required                    1.915000 m
Measured (this run, Python)  7.509662 m
Measured (frozen, Unity)     7.509663 m
Margin (this run)            +5.594662 m

Decision                    PASS

Reference reproduction      VERIFIED (6 passed, 0 failed)
```

Or inspect each link in the chain directly:

### Frozen result

[`evidence/results/otto1500_warehouse.json`](evidence/results/otto1500_warehouse.json)

Machine-readable result record: measurement, decision, source hashes,
environment git revision, test class/method, and the Unity-vs-Python
cross-validation. Also documents one internal correction, transparently:
the facility `.glb`'s hash on file went stale after a later, legitimate
scene expansion — see that file's `superseding_file_state` field.

### Manufacturer requirement

[`evidence/requirements/otto/OTTO_1500_Spec_Sheet_OTTO-DS001D-EN-APR2024.pdf`](evidence/requirements/otto/OTTO_1500_Spec_Sheet_OTTO-DS001D-EN-APR2024.pdf)

Original manufacturer source for the aisle-width requirement, with a
rendered page-1 crop and SHA-256 recorded in
[`OTTO_EVIDENCE_RECORD.md`](evidence/requirements/otto/OTTO_EVIDENCE_RECORD.md).

### Measurement preregistration

[`evidence/environments/warehouse_measurement_preregistration.md`](evidence/environments/warehouse_measurement_preregistration.md)

Defines the selected geometry and measurement method *before* the accepted
run — so the entities weren't picked after seeing a favorable number.

### Geometry validation

[`docs/geometry-validation.md`](docs/geometry-validation.md)

Geometry checks, known risks, validation decisions, failure conditions, and
explicit GO/NO-GO criteria — including the honest NO-GO on the original
public dataset that was tried and abandoned before this one was used.

### Architecture

[`docs/architecture.md`](docs/architecture.md)

## Provenance chain

```
OTTO manufacturer specification
        ↓
1.915 m minimum one-way aisle width
        ↓
selected AWS warehouse geometry (independently sourced, frozen revision)
        ↓
aws_robomaker_warehouse_ShelfF_01_001  ↔  aws_robomaker_warehouse_ShelfD_01_001
        ↓
deterministic nearest-face span measurement
        ↓
7.509663 m (Unity)  /  7.509662 m (independent Python, agrees within 1 µm)
        ↓
measured − required
        ↓
+5.594663 m
        ↓
PASS
```

## Architecture

```
Robot requirement  +  Facility geometry
                ↓
        Requirement grounding
                ↓
      Deterministic geometry query
                ↓
            Measurement
                ↓
       Compatibility decision
                ↓
      PASS  /  BLOCKED  /  REVIEW
```

AI can help interpret a requirement and deployment context. Physical
measurements come from explicit geometry. Compatibility decisions remain
deterministic and inspectable — the same glTF bounds computation runs
whether it's invoked from Unity (`AisleClearanceChecker.cs`) or from this
Python SDK (`robot_preflight/core.py`, which calls the same
`validate_independent_warehouse_glb.py` used for cross-validation).

**PASS** — evidence clearly satisfies the declared constraint.
**BLOCKED** — evidence clearly violates the declared constraint.
**REVIEW** — evidence is insufficient, ambiguous, or too close to the
tolerance for a deterministic decision either way.

Deeper detail: [`docs/architecture.md`](docs/architecture.md).

## Where this fits in deployment

Tools already exist for facility design, simulation, OEM fleet
configuration, controls, and commissioning. Robot Preflight does not
replace them. It sits earlier, and asks a narrower question:

**Are the declared robot constraints consistent with the facility evidence
we have right now?**

A verified constraint produced here could later become an input to
simulation, configuration, or commissioning preparation — but that
hand-off does not exist today. See
[`docs/workflow-context.md`](docs/workflow-context.md) for the full
deployment-lifecycle picture this fits into.

## SDK

```python
from robot_preflight import Preflight, PreflightResult

result: PreflightResult = Preflight.check(
    robot="otto_1500",
    facility="examples/otto1500_warehouse",
    constraint="aisle_clearance",
)
result.decision      # "PASS"
result.available_m   # 7.509662
result.to_dict()      # the JSON contract above
```

Source: [`robot_preflight/core.py`](robot_preflight/core.py). It
dynamically imports and calls
[`Tools/WarehouseGeometry/validate_independent_warehouse_glb.py`](Tools/WarehouseGeometry/validate_independent_warehouse_glb.py)'s
`inspect()` function directly — the same script used for the frozen
cross-validation — rather than reimplementing the geometry math a second
time.

## CLI

```bash
python -m robot_preflight check examples/otto1500_warehouse
python -m robot_preflight check examples/otto1500_warehouse --json
```

Exits `0` on PASS, `1` on BLOCKED/REVIEW or a config error — usable as a CI
gate, not just a demo.

## Verification

```bash
make verify
```

See [Reproduce it](#dont-trust-the-screenshot-reproduce-it) above for what
it checks and its output, and [`docs/verification.md`](docs/verification.md)
for the full explanation.

## Tests

```bash
make test
```

Six tests against the public API (not internals): the reference result
reproduces, it matches the frozen record within the documented tolerance,
an unresolvable entity name raises instead of returning a result, a
facility config whose `required_m` disagrees with the sourced requirement
is rejected rather than silently overridden, an unknown robot/constraint
pair raises, and — structurally — that `robot_preflight/core.py` never
reads the frozen result file in its actual code path. Source:
[`tests/test_robot_preflight.py`](tests/test_robot_preflight.py).

## Current scope

**Verified:** the selected-span aisle-clearance check above, for OTTO 1500
against the frozen OTTO/AWS example, reproduced independently of Unity.

**Implemented, not independently re-verified:** the general
robot/constraint/facility config format (`examples/otto1500_warehouse/config.yaml`)
generalizes beyond this one example, but no second example has been built
and evidenced the same way.

**Known limitation, explicitly tracked, not silently resolved:** a later
OTTO 1500 manufacturer document (`OTTO-DS001F-EN-AUG2026`) states a
different one-way aisle-width value — 2.209 m, with an explicit "no
payload" qualifier the APR2024 document's text did not carry — for the same
nominal constraint. The frozen PASS result above has **not** been re-run or
re-verified against that revision; the SDK's requirement lookup still cites
APR2024 on purpose. Full analysis:
[`evidence/requirements/otto/OTTO_EVIDENCE_RECORD_AUG2026.md`](evidence/requirements/otto/OTTO_EVIDENCE_RECORD_AUG2026.md).
More in [`docs/limitations.md`](docs/limitations.md).

## Roadmap

Planned only — none of the items below exist yet:

1. Additional physical deployment constraints (turning clearance, docking, doorway clearance)
2. Multiple robot configurations, including re-verifying against the AUG2026 OTTO document
3. Planner-ready verified constraints
4. Requalification when robot, site, task, or payload changes

## Workflow context

[`docs/workflow-context.md`](docs/workflow-context.md) — the staged AMR
deployment lifecycle, the six truths, the highest-value handoffs, and where
this repository's current evidence stops.

## Unity development environment

Unity is a runtime and visualization environment for this geometry check,
not the project's conceptual identity — the check itself is Unity-agnostic
Python above. See [`docs/unity-development.md`](docs/unity-development.md)
for setup, batchmode notes, and how to reproduce the original Play Mode
run.

## Deeper docs

- [`docs/architecture.md`](docs/architecture.md)
- [`docs/verification.md`](docs/verification.md)
- [`docs/geometry-validation.md`](docs/geometry-validation.md)
- [`docs/workflow-context.md`](docs/workflow-context.md)
- [`docs/limitations.md`](docs/limitations.md)
- [`docs/unity-development.md`](docs/unity-development.md)
- [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md) — full audit of the legacy Unity system this checker lives inside
- [`docs/FUNCTIONALITY_AUDIT.md`](docs/FUNCTIONALITY_AUDIT.md)

## Built by

Robot Preflight is currently built by Sai Teja Mutchi.

Background: 3D geometry and spatial measurement at Dassault Systèmes;
multimodal grounding at Columbia; production AI systems at Everest.

## License

Licensed under the Apache License 2.0. See [`LICENSE`](LICENSE).
