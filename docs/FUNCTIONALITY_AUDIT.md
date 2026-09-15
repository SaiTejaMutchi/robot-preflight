# ZIP Functionality and Reuse Audit

## Conclusion

The cleaned repository includes all canonical Unity project inputs from the ZIP: `Assets`, `Packages`, `ProjectSettings`, and the root documentation. No unique C# source, Unity scene, prefab, shader, project configuration, or package declaration exists only in the excluded top-level folders.

The excluded folders are generated outputs and local state: `.utmp`, `Library`, `Logs`, `UserSettings`, `Builds`, `New folder`, and recovery scenes. `New folder` contains only an APK plus Burst debug output. `Builds` contains compiled Windows and Android players. These are useful as reference/demo artifacts, but they are reproducible outputs rather than source.

## Coverage by ZIP area

| ZIP area | In clean repo | Decision | Functional value |
|---|:---:|---|---|
| `Assets/` | Yes | Keep | All product code, scenes, prefabs, models, resources, XR settings, samples, and vendored libraries |
| `Packages/` | Yes | Keep | Direct and transitive Unity dependency declarations |
| `ProjectSettings/` | Yes | Keep | Unity version, build scenes, Android, URP, input, quality, and XR settings |
| Root `README.md` | Replaced | Rewritten accurately | Old README incorrectly described an AR Foundation app; current README describes verified Quest architecture |
| `Library/` | No | Exclude | Unity import/package/build cache; regenerated from canonical inputs |
| `.utmp/` | No | Exclude | Android GameActivity, CMake, Ninja, object-file, and prefab intermediates |
| `Logs/` | No | Exclude | Local editor/import logs |
| `UserSettings/` | No | Exclude | Per-developer editor state |
| `Assets/_Recovery/` | No | Exclude | Multiple editor recovery snapshots of a scene already represented by canonical assets |
| `Builds/` | No | Exclude from Git | Windows player, Android APK, managed assemblies, runtime libraries, and data files |
| `New folder/` | No | Exclude from Git | `VR.apk` and non-shipping Burst debug information only |

## Existing capabilities available for reuse

### High-value capabilities for SignalWeave Robot Deployment Preflight

1. **Quest-ready platform shell**
   - Unity 6, Android ARM64, OpenXR, Meta XR SDK, and Quest device declarations are already configured.
   - Oculus Touch, Touch Plus, and Touch Pro profiles are enabled.
   - Meta foveation and subsampled layout are configured for headset performance.
   - Reuse: establish a clean single-purpose product scene instead of rebuilding XR bootstrapping.

2. **Runtime glTF/GLB loading**
   - `LoadModelContent` uses glTFast to fetch and instantiate remote models.
   - Server-provided position, rotation, and scale are already applied.
   - Reuse: load a SignalWeave `scene.glb` or reconstructed facility model. Replace the hard-coded experience API with an explicit scene/preflight manifest.

3. **JSON-driven spatial composition**
   - `ContentManager` already treats remote JSON as a manifest for objects, IDs, transforms, media settings, and effects.
   - Reuse: keep the manifest-driven concept and replace parallel untyped lists with typed `SceneState`, `Entity`, and `PreflightResult` DTOs.

4. **Stable object IDs and transform binding**
   - Downloaded assets are intended to use server object codes, and effects reference `objectId`.
   - Reuse: bind blocker IDs in `preflight_result.json` to reconstructed scene entities for deterministic highlighting.

5. **Mesh collider generation**
   - Loaded model renderers receive mesh colliders.
   - Reuse selectively for ray selection, inspection, and spatial highlighting. Do not enable on every high-density reconstruction mesh without profiling.

6. **XR Interaction Toolkit starter rig and input assets**
   - The repository includes XRI input actions, starter assets, locomotion, controller animation, teleportation, gaze, poke, grab, and object-spawn examples.
   - Reuse only the minimal locomotion and selection components needed for inspection. Imported starter assets are version 3.0.9 while the package is 3.3.1, so validate or reimport before production.

7. **Meta interaction samples**
   - Included samples cover hand/controller concurrency, ray, poke, grab, distance grab, locomotion, gestures, poses, snapping, manipulators, and virtual keyboard behavior.
   - Reuse as reference implementations, not production scenes. Controller-based ray selection is the lowest-risk inspection path; hand interaction can remain optional.

8. **Runtime material manipulation**
   - `MaterialChange`, `TweenTrail`, and the fade path in `ContentManager` change URP transparency and alpha.
   - Reuse conceptually for blocker highlighting, but implement a dedicated highlight material/property-block service. The shared fade target in the current implementation is race-prone.

9. **Simple timeline animation**
   - iTween-driven move, rotate, scale, and fade-in effects are present.
   - Reuse only if the preflight demo needs a short camera/object cue. It is not necessary for deterministic blocker visualization and should not be part of the core geometry calculation.

10. **Runtime image, video, and audio presentation**
    - Remote textures, video URLs, audio clips, thumbnails, playback settings, and resource prefabs are implemented.
    - Reuse later for evidence panels, robot manual excerpts, spoken notes, or walkthrough media. Current scope correctly excludes these from the narrow geometry-check vertical slice.

11. **Non-native VR keyboard**
    - A standalone MRTK keyboard can capture a typed identifier and transition to another scene.
    - Reuse later for scene/job lookup or annotations. For the narrow demo, a preconfigured input avoids UI risk.

12. **Legacy authenticated catalog UI**
    - Login, bearer-token requests, experience listing, dynamic buttons, profile UI, and logout exist.
    - Reuse the catalog/button-generation pattern only after security refactoring. Do not reuse plaintext password persistence, response logging, or the split legacy API contract.

13. **Desktop fallback build path**
    - The ZIP contains a Windows standalone build and the project has Standalone OpenXR configuration.
    - Reuse as evidence that a desktop player was produced and as a fallback target concept. The compiled Windows build is not a substitute for a clean build from the reconstructed source.

14. **Bundled visual assets**
    - Models, prefabs, HDR environment, fonts, UI assets, shaders, and sample scenes can accelerate prototyping.
    - Reuse only assets with confirmed licenses and product relevance. Large samples/fonts/HDR files should be pruned after reference checks.

## Functionality not present in the ZIP

The attached deployment plan is explicit that the following must still be built; none is hidden in excluded ZIP folders:

- SignalWeave reconstruction ingestion contract (`scene.glb` plus `scene_state.json`)
- Robot PDF ingestion and AI extraction
- Source-page/text provenance and confidence
- Engineer review/edit/approval UI
- Versioned approved robot profile
- Deterministic aisle/opening/clearance geometry checks
- `preflight_result.json` contract
- Required-versus-measured-versus-deficit result presentation
- Blocker-to-scene entity/region binding
- Reliable highlight/inspection visualization for preflight results
- Audit trail, report export, or evidence persistence
- Automated tests and ten-run demo reliability harness

The existing project supplies a spatial viewer foundation, not the robot-preflight product logic.

## Important reusable evidence from excluded binaries

The ZIP contains:

- `New folder/VR.apk`
- `Builds/VR platform.apk`
- `Builds/SG-VR.exe` plus its Windows runtime data
- compiled `Assembly-CSharp.dll` and dependency assemblies

Recommended handling:

1. Copy binaries to a release/artifact store outside Git if historical execution matters.
2. Record SHA-256 hashes, build dates, target platform, and known behavior.
3. Run the APK on Quest and the Windows build in an isolated test environment to capture screenshots/video and expected behavior.
4. Use them as black-box baselines only; do not decompile generated assemblies into the source tree unless a verified source gap is discovered.

## Current source gaps that limit reuse

- Build Settings enable `DemoScene 1` and an imported XRI demo, while application code loads scene indices 1–4 and the product `AR` scene is disabled.
- `ContentManager` hard-codes one SpatialGrid experience ID.
- Dashboard selection data is not transferred into the content scene.
- Audio/video setting fields contain mapping/index defects.
- Model object naming can break object-ID animation binding.
- No unified completion state guarantees that all downloaded assets are ready before effects begin.
- Passwords and tokens are handled insecurely.
- No typed schema, payload limits, cancellation, cache, or resource-unload lifecycle exists.

These are implementation defects in the retained source, not evidence of omitted ZIP functionality.

## Recommended narrow reuse boundary

For the narrow geometry-check scope, reuse:

- Unity 6 + URP + Android/Quest configuration
- OpenXR/Meta XR loader and controller rig
- glTFast runtime loading
- JSON-manifest idea
- entity IDs/transforms
- a minimal selection/highlight interaction
- desktop build target as fallback

Bypass for the deadline:

- legacy login/dashboard/profile flow
- hard-coded SpatialGrid experience request
- image/video/audio content authoring
- iTween timeline system
- virtual keyboard
- multiplayer package/scaffolding
- recovery/sample scenes as product entry points

Build newly:

- typed SignalWeave scene and preflight contracts
- AI-assisted, cited constraint extraction with engineer approval
- deterministic geometry check
- stable result-to-entity binding
- blocker visualization with auditable measurements

This boundary matches the attached plan: it extracts maximum leverage from the ZIP without spending the deadline on unrelated prototype cleanup.
