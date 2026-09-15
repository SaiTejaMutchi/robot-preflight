# Unity development environment

Unity is a runtime and visualization environment for the geometry check in
this repository, not its conceptual identity. The check itself — see
[`robot_preflight/core.py`](../robot_preflight/core.py) and
[`Tools/WarehouseGeometry/validate_independent_warehouse_glb.py`](../Tools/WarehouseGeometry/validate_independent_warehouse_glb.py)
— is plain Python and requires no Unity install to run or reproduce.

Unity remains necessary to reproduce the original Play Mode measurement and
for visual inspection of the warehouse scene.

## Project

This repository's Unity project (`SG-VR` / `SpatialGrid VR`) started as a
prototype for viewing remotely authored experiences on Meta Quest headsets
(OpenXR/Meta XR, a legacy content API). The geometry checker is a later,
isolated addition with no dependency on that legacy system — see
[`ARCHITECTURE.md`](ARCHITECTURE.md) for a full audit of the legacy pipeline
it bypasses, and [`architecture.md`](architecture.md) for the current
checker's own implementation detail.

## Setup

- Unity `6000.3.9f1` (exact version in `ProjectSettings/ProjectVersion.txt`)
- Android Build Support, SDK/NDK, and OpenJDK via Unity Hub (Quest builds only)
- A supported Meta Quest device (Quest 2, Quest Pro, Quest 3, or Quest 3S) for Quest builds only

To reproduce the original Unity-side result:

1. Clone this repository.
2. Open the repository root in Unity Hub with Unity `6000.3.9f1`.
3. Let Package Manager restore packages and Unity regenerate `Library/`.
4. Open `PlatformVR_GeometryTest`, enter Play mode, and inspect the Console
   — see [`geometry-validation.md`](geometry-validation.md) §10 for the
   exact expected output.
5. For Quest: select Android/ARM64 and OpenXR in Build Profiles, then deploy.

## Batchmode invocation notes

Non-obvious gotchas hit while producing the evidence in `evidence/`, kept
here so a future run doesn't rediscover them the hard way:

- Never combine `-quit` with `-runTests` — Unity's test runner manages its
  own exit; combining them silently runs zero tests (looks like success,
  isn't).
- `-nographics` breaks Play Mode screenshot capture
  (`RenderTexture.Create failed`). Only use `-nographics` for a headless
  scene-*build* step; omit it for any `-runTests -testPlatform PlayMode`
  invocation that needs a real render.
- Example invocation pattern:
  ```bash
  UNITY="<path to Unity 6000.3.9f1 executable>"
  # Run Play Mode tests (needs real rendering, no -nographics, no -quit):
  "$UNITY" -batchmode -silent-crashes -logFile <path>.log \
    -projectPath "$(pwd)" \
    -runTests -testPlatform PlayMode -testResults <path>.xml
  # Run EditMode tests (nographics is fine here):
  "$UNITY" -batchmode -nographics -silent-crashes -logFile <path>.log \
    -projectPath "$(pwd)" \
    -runTests -testPlatform EditMode -testResults <path>.xml
  ```
- `WaitForEndOfFrame` is unsupported in batchmode — screenshot capture must
  use `RenderTexture` + `Camera.Render()` + `ReadPixels` + `EncodeToPNG`.
- These are long-running background processes (30 s – 2 min); wait for
  actual process completion, not just a launcher script's own return.

## Core geometry code (stable; do not modify without a reason tied to evidence)

- `Assets/Scripts/WarehouseGeometry/AisleClearanceChecker.cs` — the single
  deterministic measurement function.
- `Assets/Scripts/WarehouseGeometry/WarehouseEntityRegistry.cs` — indexes
  scene Transforms by name, computes world-space renderer bounds.
- `Assets/Scripts/WarehouseGeometry/CompatibilityResult.cs` — the typed
  result contract.
- `Assets/StreamingAssets/WarehouseGeometry/aws_independent_warehouse.glb`
  — the frozen, converted independent-environment scene. Its exact
  boundary transforms are what the frozen result in `evidence/results/`
  depends on; do not regenerate or reconvert it casually.

## Deeper reports

- [`FUNCTIONALITY_AUDIT.md`](FUNCTIONALITY_AUDIT.md) — what was kept vs.
  excluded when this project was cleaned from its original archive.
- [`verification.md`](verification.md) — includes the Unity test/bug
  history (EditMode results, two real bugs found and fixed in Play Mode).
