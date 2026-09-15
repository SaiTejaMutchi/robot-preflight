# Architecture

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

Robot Preflight turns sourced robot requirements and facility evidence into
inspectable deployment constraint checks before simulation and commissioning.
Task-aware grounding and multi-constraint preflight are next.
Physical measurements come from explicit geometry. Compatibility decisions remain
deterministic and inspectable.

## Decision vocabulary

- **PASS** — evidence clearly satisfies the declared constraint.
- **BLOCKED** — evidence clearly violates the declared constraint.
- **REVIEW** — evidence is insufficient, ambiguous, or too close to the
  tolerance for a deterministic decision either way.

## Two independent implementations of the same computation

The same geometry computation exists in two places, deliberately, so one
can check the other:

- `Assets/Scripts/WarehouseGeometry/AisleClearanceChecker.cs` — the Unity
  runtime path, run in Play Mode against a real glTFast import.
- `Tools/WarehouseGeometry/validate_independent_warehouse_glb.py` — a
  from-scratch Python glTF binary parser with no Unity dependency, used
  both as an independent cross-check and as the computation the
  `robot_preflight` SDK calls directly (see `robot_preflight/core.py`).

Both take the same inputs — a facility `.glb` and two named entity
boundaries — and compute the same nearest-face world-space gap along a
configured axis. They agree to within 1 micrometer on the frozen example;
see [`verification.md`](verification.md) for the exact numbers.

## Unity-side implementation detail

For the Unity execution path specifically (`Assets/Scripts/WarehouseGeometry/`):

- **Loader** (`WarehouseGeometryTestController.Run()`): unloads any
  previously loaded scene first (clears highlight/result, restores camera,
  destroys the previous root, disposes the glTFast importer — so re-running
  cannot layer a second imported scene on top of the first), then loads the
  configured `.glb` via glTFast with `NodeNameMethod =
  NameImportMethod.OriginalUnique` (node names preserved exactly,
  de-duplicated only on collision), instantiated under a fresh root object.
- **Entity registry** (`WarehouseEntityRegistry`): indexes instantiated
  nodes by name, exposes `TryResolveUnique(name, out Transform)` which
  fails on a missing *or* duplicated name, and `TryGetRendererBounds` to
  compute world-space bounds from all renderers under a transform. A run
  aborts with an explicit exception rather than proceeding with an
  ambiguous name binding.
- **Checker** (`AisleClearanceChecker.Check`): rejects `requiredMeters <=
  0`; available clearance is the gap between the two bounds' near faces
  along the chosen axis, clamped to a minimum of 0; status is `Review` if
  `|difference| <= tolerance`, else `Blocked` if `difference < 0`, else
  `Pass`. Pure deterministic arithmetic over two `Bounds` — no AI in this
  path.
- **Highlight** (`WarehouseHighlightController`): preserves each renderer's
  original `MaterialPropertyBlock` and restores it on `Clear()`, so
  highlighting a BLOCKED/REVIEW region is reversible and never mutates
  source materials.
- **Camera focus**: captures the camera's pre-focus position/rotation/
  near-clip once per load and restores it on unload, so repeated load/
  unload cycles don't accumulate drift.
- **Diagnostic logging**: every run emits structured log lines
  (`WAREHOUSE_IMPORT`, `WAREHOUSE_ENTITIES`, `WAREHOUSE_TEST`) intended as
  runtime evidence, independent of any UI.

## What this repository is not

Not a whole-facility digital twin, not a route planner, not a fleet
manager, not a safety certification tool, not a simulator. It is a
narrow, deterministic compatibility check between one robot requirement and
one piece of facility geometry. See [`limitations.md`](limitations.md) and
[`workflow-context.md`](workflow-context.md) for where that fits into the
larger deployment problem.

## Deeper documentation

- [`geometry-validation.md`](geometry-validation.md) — the geometry
  pipeline's GO/NO-GO analysis and risks.
- [`ARCHITECTURE.md`](ARCHITECTURE.md) — a full audit of the Unity project
  this checker lives inside, including the legacy system it bypasses.
- [`unity-development.md`](unity-development.md) — Unity setup and
  batchmode notes for reproducing the original Play Mode run.
