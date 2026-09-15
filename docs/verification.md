# Verification

Robot Preflight's public claim is narrow and specific, so it should be easy
to check rather than easy to believe. This document describes exactly what
`make verify` does and where each piece of evidence lives.

## What `make verify` does

`scripts/verify_reference_result.py`:

1. Hashes the OTTO 1500 manufacturer PDF on disk and compares it against
   the hash recorded in the frozen result record.
2. Hashes the facility `.glb` on disk and compares it against the current
   hash recorded there (see [`limitations.md`](limitations.md) for why
   there are two hashes on file for that GLB, and which one is current).
3. Calls `robot_preflight.Preflight.check(...)` — the same public entry
   point a user would call — which re-parses the facility geometry and
   recomputes the measurement from scratch.
4. Compares the freshly computed required/measured/decision values against
   the frozen record, within the documented ~1 micrometer Unity-vs-Python
   agreement tolerance.
5. Prints each check's pass/fail status and the final values, and exits
   non-zero if anything failed.

It does not print stored numbers without running the steps above — if the
geometry parser or the hash check fails, the script fails loudly rather
than falling back to the frozen JSON.

## Two independent computations, one result

The same measurement was computed twice, by two different programs, and
the two independent runs are recorded side by side:

| Computation | Language/runtime | Result |
|---|---|---|
| Main execution path | C# / Unity 6000.3.9f1 Play Mode, glTFast import | 7.509663 m |
| Independent cross-check | Python, direct binary glTF parsing, no Unity | 7.509662 m |

Agreement: within 1 micrometer (float vs. double rounding only). Both
values, the test class/method for the Unity run, and the exact evidence log
paths are recorded in
[`../evidence/results/otto1500_warehouse.json`](../evidence/results/otto1500_warehouse.json).

The Python SDK in this repository (`robot_preflight/core.py`) calls the
same script used for that independent cross-check
(`Tools/WarehouseGeometry/validate_independent_warehouse_glb.py`) directly,
rather than reimplementing the geometry math a second time or reading the
frozen JSON back.

## Tests

`tests/test_robot_preflight.py` (`make test`) exercises the public API, not
internals:

- the reference result reproduces from a fresh computation;
- it matches the frozen record within the documented tolerance;
- a config pointing at a nonexistent entity name raises, rather than
  returning a result;
- a config whose stated `required_m` disagrees with the sourced
  requirement is rejected, rather than silently overridden;
- an unknown robot/constraint pair raises;
- structurally, that `robot_preflight/core.py` never reads the frozen
  result JSON in its actual code path (checked via `ast`, not just a
  string search of comments).

## Unity test history

Before the current SDK existed, the Unity-side implementation went through
its own verification chronology, including two real bugs found and fixed:

- **Bootstrap timing bug:** `WarehouseGeometryTestBootstrap`'s
  `[RuntimeInitializeOnLoadMethod(AfterSceneLoad)]` fires once, at the
  *first* scene loaded in a Play session — the Unity Test Framework's own
  bootstrap scene, not the actual geometry test scene loaded afterward.
  Fixed by also subscribing to `SceneManager.sceneLoaded`.
- **Batchmode screenshot bug:** the first Play Mode test's own screenshot
  code used `ScreenCapture.CaptureScreenshot`, which depends on
  `WaitForEndOfFrame` — unsupported in `-batchmode`. The measurement itself
  was already correct at this point (`required=1.915m available=1.750m
  difference=-0.165m status=Blocked`, matching the offline prediction
  exactly); only the test harness's own screenshot capture failed. Fixed by
  switching to `RenderTexture` + `Camera.Render()` + `ReadPixels`.

14 named EditMode tests (`AisleClearanceCheckerTests`,
`WarehouseEntityRegistryTests`, `WarehouseHighlightControllerTests` and
others covering equality/tolerance/rejection/duplicate-name/missing-node/
zero-geometry/hierarchy-transform/highlight-mutation cases) ran with result
`14/14 passed, 0 failed, 0 inconclusive, 0 skipped`.

## Offline validation of the BLOCKED and REVIEW path

The verified PASS result above is not the only case exercised. Before the
independent AWS warehouse measurement, two controlled fixtures were
validated offline (`Tools/WarehouseGeometry/validate_warehouse_glb.py`,
no Unity, no rendering) and — for the 1.750 m case — cross-checked with a
real Unity Play Mode run:

| File | SHA-256 | Available | Required | Difference | Status |
|---|---|---:|---:|---:|---|
| `warehouse_controlled_0_940m.glb` | `97c755a0...4fc1` | 0.940 m | 1.915 m | -0.975 m | BLOCKED |
| `warehouse_source.glb` | `5fcb83a7...2353` | 1.750 m | 1.915 m | -0.165 m | BLOCKED |

The 1.750 m case's offline prediction and its later Unity Play Mode
measurement were an exact match, giving early confidence in the
offline/runtime agreement pattern later confirmed to 1 µm on the frozen
AWS example. Full record:
[`../evidence/results/offline_geometry_validation.json`](../evidence/results/offline_geometry_validation.json).

## Deeper evidence

- [`geometry-validation.md`](geometry-validation.md) — the geometry
  pipeline's own GO/NO-GO analysis, risks, and validation decisions.
- [`../evidence/environments/warehouse_measurement_preregistration.md`](../evidence/environments/warehouse_measurement_preregistration.md)
  — the measurement method, written down *before* the accepted run.
- [`../evidence/environments/independent_environment_record.md`](../evidence/environments/independent_environment_record.md)
  — source, license, and hash freeze for the AWS warehouse environment.
