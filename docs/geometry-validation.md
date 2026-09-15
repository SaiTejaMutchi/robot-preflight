# SpatialGrid warehouse geometry test report

**Assessment:** implementation-ready GO for desktop; conditional GO for Quest.
The isolated source path is implemented and offline-validated. An actual Unity
Editor/Player run remains required because Unity 6000.3.9f1 is not installed on
the assessment host. No runtime capability is claimed from static inspection.

## 1. GO / NO-GO

**PUBLIC DATASET NO-GO within this host/timebox; implementation GO for the
controlled desktop proof.** glTFast 6.14.1 is already pinned and its
runtime API supports URL/file loading and hierarchy-preserving instantiation.
The test does not depend on authentication, dashboard, API routing, XR, or
legacy animation. **Quest is conditional GO** after device profiling.

## 2. Best warehouse input format

An uncompressed, self-contained GLB 2.0 in meters, with semantic node names.
This is less failure-prone than separate `.gltf` buffers/textures and is already
supported by the pinned package without adding packages.

## 3. Lowest-risk conversion pipeline

Direct GLB is preferred. Next is FBX/OBJ/USD imported into Blender or another
DCC, normalized and exported to uncompressed GLB. Point cloud to mesh is last:
it adds meshing, decimation, topology, scale, and semantic-label risks.

The public warehouse scene actually obtained was mixxit's **3D Industrial
Warehouse** from OpenGameArt (CC BY 3.0): an 87.2 KB ZIP containing a 1.10 MB
SketchUp-generated Collada DAE, declared in inches with Z-up. The host has no
Blender, Assimp, or Collada-to-glTF converter, and the source exposes no useful
semantic rack names, so conversion was stopped rather than turning into an
unbounded conversion project.

For the independent fallback, a CC BY 4.0 public real-scale industrial-rack GLB
was used directly and assembled twice into a deterministic layout. It is a
public synthetic warehouse component inside a controlled testcase, not a
captured warehouse dataset and not a valid substitute for the unknown public
scene measurement.

## 4. Exact SpatialGrid components reused

- Unity 6000.3.9f1 project and URP 17.3.0 configuration.
- `com.unity.cloud.gltfast` 6.14.1 and its `GltfImport` runtime path.
- Existing desktop camera/light scene foundation.
- Existing OpenXR/Meta configuration only as a later Quest starting point.

## 5. Exact legacy components bypassed

`ContentManager`, `LoadModelContent`, `LoginManager`, dashboard/user/API code,
`MaterialChange`, `TweenTrail`, iTween/timeline code, media loaders, virtual
keyboard, multiplayer, and the XRI sample scene. The proof creates no mesh
colliders and calls no server.

## 6. Required code changes

The new isolated assembly loads local StreamingAssets or an explicit remote
URL, requests original unique glTF names, instantiates under one root, indexes
named entities, calculates renderer bounds, checks Z-axis clearance, highlights
a blocker with `MaterialPropertyBlock`, and focuses the active camera. The test
scene is now the sole enabled build scene. See the repository diff for exact
files.

## 7. Risks for desktop

- Unity import/play has not yet been run on this host.
- Arbitrary datasets may omit stable names or encode unexpected units/pivots.
- glTFast can return success on partial loads; production code should capture
  strict logger diagnostics.
- Transparent source materials and shader extensions need asset-specific QA.

## 8. Risks for Quest

- Android StreamingAssets uses a packaged URI and needs an actual APK test.
- Large meshes/textures can exceed memory or GPU budgets even when desktop is fine.
- Runtime material/shader variants must be included and verified on device.
- Dense per-triangle colliders are explicitly avoided; later interaction should
  use selected primitive/simplified colliders.

## 9. Recommended warehouse scene limits

For the first desktop proof: under 200k visible triangles, 20 materials, 30 MB
GLB, 2k maximum texture dimension, and a shallow named hierarchy. For Quest:
start under 100k visible triangles, 10 materials, 20 MB, 1k textures, minimal
transparency, and no blanket MeshCollider generation. Raise limits only from
device profiling.

## 10. Fastest under-60-minute proof

Open `PlatformVR_GeometryTest`, enter Play mode, and inspect Console. Success is:
scene loads; rack bounds preserve metric scale; unique `Rack_Left` and
`Rack_Right` resolve; output reads `available=1.750m`,
`difference=-0.165m`, `status=Blocked`; `Blocker_Region_01` turns red; and the
camera focuses it. This 1.750 m result is derived from imported bounds but its
layout was deliberately generated, so it is a fallback result—not a public
dataset aisle finding. Then launch a Player with `--warehouse-controlled` (or set
the controller's public `Source` before its first frame) to load
`WarehouseGeometry/warehouse_controlled_0_940m.glb`: expected output is exactly
`available=0.940m`, `difference=-0.975m`, `status=Blocked`.

## 11. Format, transform, hierarchy, and material findings

The repository supports `.glb`/`.gltf` through glTFast today. It has no current
runtime FBX/USD/OBJ loader. glTFast's standard GameObject instantiator preserves
node hierarchy and local transforms; `OriginalUnique` preserves usable names.
The public rack uses one standard metallic-roughness PBR material and no
textures, a low-risk URP case. More complex texture and extension combinations
must be checked individually.

## 12. Geometry split and collider policy

A warehouse can be one GLB, but blocker-addressable regions must remain named
nodes with distinct renderer subtrees. Splitting into multiple network assets
is unnecessary for this proof; split only for streaming, culling, update, or
memory reasons. The old loader's collider-per-renderer behavior is unsafe for a
warehouse model and is not reused.

## 13. Final recommendation and UI readiness

Proceed with **A. direct warehouse GLB loading** when the public source is
already GLB and semantically named. Use **B. convert to GLB first** for a chosen
FBX/USD/OBJ dataset. Do not choose **C** unless licensing, conversion, naming,
or performance makes external data unusable. This implementation used a public
GLB component plus controlled assembly, so it proves the runtime contract but
not fidelity to a real captured facility.

The exact public-data result fields are therefore: source = OpenGameArt 3D
Industrial Warehouse; original = DAE; converted GLB = no; scene scale = source
metadata 0.0254 m/unit, not runtime-verified; selected aisle/boundaries/clearance,
difference, and status = not available because conversion stopped. The fallback
GLB is 32,892 bytes, 960 unique-mesh triangles, four materials, and zero
textures; it measures 1.750 m between `Rack_Left` and `Rack_Right`, for -0.165 m
and BLOCKED. The controlled GLB is 1,960 bytes, 36 unique-mesh triangles, three
materials, and measures 0.940 m, for -0.975 m and BLOCKED.

Offline validation proves both generated assets and arithmetic, but it does not
prove glTFast instantiation or rendering. Therefore the statement “SpatialGrid
path verified; public dataset conversion is the remaining problem.” must not be
claimed until the pending Unity Play Mode run passes.

The repo is **ready for the separate inspection-UI prompt after the Unity Play
Mode acceptance run passes**. Until that run, UI work should not mask loader or
binding failures.
