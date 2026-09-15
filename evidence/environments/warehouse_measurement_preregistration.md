# Demo Measurement Preregistration — Independent AWS Environment (ENV-002)

**This document is immutable historical evidence once committed.** Its selected environment, boundaries, threshold, method, and expected-outcome field must never be edited after the measurement is observed. If the measurement definition must change, create `DEMO_MEASUREMENT_PREREGISTRATION_v2.md` and explain why this v1 was abandoned — never rewrite this file after seeing a result.

**Expected outcome: UNKNOWN.** No clearance value has been calculated, inspected, or estimated for this environment/aisle/boundary combination prior to committing this file. Selection below was made solely on geometric completeness, structural stability, and clear metric interpretation — never on which result (PASS or BLOCKED) it might produce.

## Dataset / environment

- **Name:** AWS RoboMaker Small Warehouse World
- **Producer:** Amazon Web Services / `aws-robotics`
- **Frozen repository revision:** `3c23a698bf0b4e366ddf8b084af507c519bd3483`
- **License:** MIT No Attribution (`MIT-0`), license SHA-256 `ef47b4ae2a1a8d38ef87b38dc5927967e7b472ffa3f7d125e89dbc8353a1e7af`
- **Freeze record of authority:** `evidence/environments/independent_environment_record.md` (ENV-001, `VERIFIED SOURCE FREEZE`)

## Exact scene/file identity

- **Exact scene entry point:** `worlds/small_warehouse.world`
- **Archive URL (immutable):** `https://github.com/aws-robotics/aws-robomaker-small-warehouse-world/archive/3c23a698bf0b4e366ddf8b084af507c519bd3483.tar.gz`
- **Archive SHA-256 (frozen, re-verified this session by re-downloading):** `213417f3b243cc6259a17b1eeafba28edd44f7d129ec5904f88793f1bc75d0e6` — **re-download match: CONFIRMED**, byte-for-byte identical to the ENV-001 freeze
- **Scene-file SHA-256 (frozen, re-verified this session):** `4a7d96866367df85668b09a71c60ce3a2807e7d3fe05df77a0edc62a1fa8e263` — **re-download match: CONFIRMED**
- **Model-manifest SHA-256 (frozen, not re-hashed this session):** `8c312694bc61fd7cd59f28e49a69e4fc86b2dbe35fc6546c6024f24a07a9fafe`

## Selected aisle segment

The longitudinal aisle directly between the single west-wall shelf unit and the nearest east-row shelf unit at the same approximate north/south (SDF Y) position, read verbatim from `worlds/small_warehouse.world` (re-downloaded and re-hashed this session, matching the frozen scene hash above):

```xml
<model name="aws_robomaker_warehouse_ShelfF_01_001">
  <include><uri>model://aws_robomaker_warehouse_ShelfF_01</uri></include>
  <pose frame="">-5.795143 -0.956635 0 0 0 0</pose>
</model>

<model name="aws_robomaker_warehouse_ShelfD_01_001">
  <include><uri>model://aws_robomaker_warehouse_ShelfD_01</uri></include>
  <pose frame="">4.73156 -1.242668 0 0 0 0</pose>
</model>
```

Selection rationale (geometric, not outcome-based):
- `ShelfF_01_001` is the **only** shelf instance on the west side of the warehouse — a structurally unambiguous, singular boundary with no group-membership ambiguity.
- Among the six east-side shelf instances (`ShelfE_01_001..003`, `ShelfD_01_001..003`), `ShelfD_01_001` (SDF Y = `-1.242668`) is the closest in the Y/north-south axis to `ShelfF_01_001` (SDF Y = `-0.956635`), a Y-difference of only `0.286033` m — meaning these two units face each other directly across the same aisle segment, rather than being diagonally offset. The next-closest candidate, `ShelfE_01_001` (Y = `0.57943`), is over 5× farther in Y and was rejected for that reason alone.
- Both instances are `<pose>` values directly on static, permanently-placed `<model>` declarations in the world file — not derived, not procedurally generated, not movable props.

## Exact boundaries

- **Boundary A:** `aws_robomaker_warehouse_ShelfF_01_001`, SDF pose `(-5.795143, -0.956635, 0, 0, 0, 0)` (west)
- **Boundary B:** `aws_robomaker_warehouse_ShelfD_01_001`, SDF pose `(4.73156, -1.242668, 0, 0, 0, 0)` (east)
- Raw pose-origin separation (not the clear aisle width — origins only, informational): ΔX = `10.526703` m, ΔY = `0.286033` m

## Measurement method (frozen; execute exactly this way in GEO-003)

1. Convert the frozen SDF/COLLADA source to GLB following the preferred conversion order already recorded in `TASK_LEDGER.md` ("Independent-environment protocol"): direct GLB/glTF → FBX to GLB → USD to GLB → **DAE to GLB** (applicable here) → point cloud to mesh to GLB. Honor the COLLADA `<unit meter="0.010000">` (centimeter) declaration and the SDFormat meters/radians pose convention exactly as documented in `independent_environment_record.md` §"Metric-scale evidence"; convert Z-up to the target engine convention exactly once.
2. Import the derived GLB into Unity 6000.3.9f1 via glTFast (`NameImportMethod.OriginalUnique`), the same runtime path already verified for the controlled fixtures in RT-003.
3. Resolve `Boundary A` and `Boundary B` to their instantiated node transforms by the exact model names above (fail if not unique — do not proceed on an ambiguous or duplicate name match).
4. Compute axis-aligned world-space renderer bounds for each boundary node.
5. Apply the same deterministic function already implemented in `Assets/Scripts/WarehouseGeometry/AisleClearanceChecker.cs` (`AisleClearanceChecker.Check`): available clearance = the gap between the two bounds' near faces, clamped at 0; `difference = available - required`; status `Review` if `|difference| ≤ tolerance`, else `Blocked` if negative, else `Pass`.

## Measurement axis / plane / height

- **Axis:** `X` (`ClearanceAxis.X` in the existing checker) — the SDF X-axis is the direction of pose separation between Boundary A and Boundary B (ΔX = 10.53 m vs ΔY = 0.29 m), i.e., the aisle runs along X.
- **Plane / height:** horizontal ground-plane clearance only (SDF X/Y plane → glTF/Unity X/Z after the one-time up-axis conversion), using each shelf's full axis-aligned renderer bounds — the same height-agnostic footprint-gap method already used for the two controlled fixtures. No separate height/elevation cross-section is defined; if the converted geometry requires one (e.g., non-uniform shelf silhouette by height), that must be recorded as an amendment in a `_v2` file, not retrofitted into this one.

## Requirement, tolerance, and decision rule (frozen, unchanged from OTTO evidence)

- **OTTO 1500 requirement:** `1.915 m`
- **Source document ID:** `OTTO-DS001D-EN-APR2024`
- **Page:** `1`
- **Exact source text:** `Min. Aisle Width 1915 mm (78 in) (One Way)`
- **Tolerance:** `±0.005 m` (matches `AisleClearanceChecker`'s default `reviewToleranceMeters`)
- **Decision rule:** `status = Review` if `|available − 1.915| ≤ 0.005`; else `status = Blocked` if `available < 1.915`; else `status = Pass`. Accept whatever this rule produces — do not cherry-pick a different aisle pair after seeing the result.

## Expected outcome

**UNKNOWN.** This is a genuine unknown at the time of commit: no conversion, import, or bounds computation has been performed against `ShelfF_01_001`/`ShelfD_01_001` by this or any prior session. GEO-003 must not run before this file is committed, and once GEO-003 produces a result, this file must not be edited to match it.

## Timestamp and commit

- **Preregistered:** 2026-09-10, this session
- **Git commit introducing this file:** recorded at commit time immediately after this file is written and staged (see `TASK_LEDGER.md` Progress Log for the exact hash)
