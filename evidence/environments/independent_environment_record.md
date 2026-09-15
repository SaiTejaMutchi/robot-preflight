# Independent Warehouse Environment Freeze Record

## ENV-001 status

`VERIFIED — SOURCE FROZEN; NOT CONVERTED; NOT MEASURED`

Freeze date: 2026-09-10 ET

No aisle-clearance value was calculated during this task. Measurement-region preregistration belongs to ENV-002.

## Selected independent asset

- **Source:** AWS RoboMaker Small Warehouse World
- **Producer:** Amazon Web Services / `aws-robotics`
- **Canonical repository:** https://github.com/aws-robotics/aws-robomaker-small-warehouse-world
- **Frozen revision:** `3c23a698bf0b4e366ddf8b084af507c519bd3483` (`ros1` branch tip inspected on 2026-09-10)
- **Immutable source archive:** https://github.com/aws-robotics/aws-robomaker-small-warehouse-world/archive/3c23a698bf0b4e366ddf8b084af507c519bd3483.tar.gz
- **Archive filename:** `aws_small_warehouse_3c23a698bf0b4e366ddf8b084af507c519bd3483.tar.gz`
- **Archive size observed:** 10,156,209 bytes (displayed as 9.7 MiB by `ls`)
- **Archive SHA-256:** `213417f3b243cc6259a17b1eeafba28edd44f7d129ec5904f88793f1bc75d0e6`
- **Exact scene entry point:** `worlds/small_warehouse.world`
- **Scene-file SHA-256:** `4a7d96866367df85668b09a71c60ce3a2807e7d3fe05df77a0edc62a1fa8e263`
- **Scene model references:** 14 unique `model://` assets; 28 active model declarations
- **Model payload:** 99 files under `models/` (about 12 MiB extracted)
- **Ordered relative-path model-manifest SHA-256:** `8c312694bc61fd7cd59f28e49a69e4fc86b2dbe35fc6546c6024f24a07a9fafe`

The model-manifest hash was produced from the extracted revision root by hashing every regular file under `models/`, sorted by relative path, then hashing that manifest. Paths in the manifest begin with `models/` and contain no host-specific prefix.

The archive plus its internal `worlds/small_warehouse.world` and `models/` directory is the frozen source asset. It is a Gazebo/SDFormat scene bundle, not yet a Unity-ready GLB. Any later conversion must retain the frozen source archive/hash and record the derived GLB hash separately.

## Provenance

The source repository describes this as a Gazebo warehouse world for warehouse and logistics robot applications. It was produced and published by AWS in the public `aws-robotics` organization. The frozen revision predates the repository's archival notice and contains the complete original scene definition, model definitions, visual/collision COLLADA meshes, textures, maps, documentation images, and license.

Repository README at the frozen revision:

https://github.com/aws-robotics/aws-robomaker-small-warehouse-world/blob/3c23a698bf0b4e366ddf8b084af507c519bd3483/README.md

The asset is independent of SpatialGrid and SignalWeave: neither this repository nor this demo generated, assembled, or tuned its shelf placement.

## License

- **License:** MIT No Attribution (`MIT-0`)
- **Frozen license file:** https://github.com/aws-robotics/aws-robomaker-small-warehouse-world/blob/3c23a698bf0b4e366ddf8b084af507c519bd3483/LICENSE
- **License-file Git blob:** `1bb4f21e5d8b5b21b6952c0b9bf2e6220d9a56be`
- **License-file SHA-256:** `ef47b4ae2a1a8d38ef87b38dc5927967e7b472ffa3f7d125e89dbc8353a1e7af`

The frozen license text matches the SPDX `MIT-0` grant and permits use, copying, modification, distribution, sublicensing, and sale without an attribution condition. Preserve this record with any derived asset even though attribution is not required.

## Metric-scale evidence

The world is SDFormat 1.6. Its model placements are expressed through six-value `<pose>` elements. SDFormat defines pose translation as meters and rotation as radians:

https://sdformat.org/tutorials/specification/specify_pose/

The shelf model definitions use `<scale>1 1 1</scale>`. Their source COLLADA visual meshes declare:

```xml
<unit meter="0.010000" name="centimeter" />
<up_axis>Z_UP</up_axis>
```

This provides an explicit conversion from modeled centimeters to meters rather than an inferred or visually guessed scale. A later GLB conversion must honor the COLLADA unit declaration, preserve the SDF world poses, convert Z-up to the selected glTF/Unity convention once, and validate the derived asset before measurement.

## Meaningful aisle evidence

The frozen scene contains a navigable longitudinal warehouse aisle visible in the source render `docs/images/small_warehouse_gazebo.png`, between independently placed warehouse shelf groups:

- western shelf boundary candidate: `aws_robomaker_warehouse_ShelfF_01_001`;
- eastern shelf boundary group candidate: the named `aws_robomaker_warehouse_ShelfD_01_*` and `aws_robomaker_warehouse_ShelfE_01_*` instances.

The world file gives these objects stable model names and explicit poses, and the asset includes separate visual and collision geometry for the shelf types. This is sufficient to freeze the environment as an independent source with meaningful candidate aisle geometry. ENV-002 must still preregister one exact aisle segment and the exact two boundary entities before any clearance calculation.

Source render SHA-256: `86810edfab22cdcc0186637b3954ac7adacf9bbab6ae0154dde92cddb6c7b9f3`

## Voxel51 candidate rejection

**Candidate:** Voxel51 `lidar-warehouse-dataset`

**Decision:** `REJECTED FOR ENV-001 PRIMARY ASSET`

The dataset itself is legitimate, warehouse-specific, metric, and CC-BY-SA-4.0. It contains 3,287 consecutive Velodyne VLP-16 scans and object annotations. However, the distributable dataset exposes each scan as a sensor-centric PCD with an average of about 4,879 points. The published structure does not include the LiDAR poses needed to register the scans into the dense global SLAM reconstruction; the dense reconstruction used by the authors is represented only by a PNG rendering, not a measurable point cloud or mesh.

Four separated published frames (`000000`, `000100`, `000250`, `000498`) were visually inspected. They show sparse radial single-scan returns and do not provide two durable, independently identifiable aisle surfaces suitable for reproducible blocker binding. Aggregating them without source poses would invent geometry and violate the independent-environment evidence rule.

Sources:

- https://huggingface.co/datasets/Voxel51/lidar-warehouse-dataset
- https://github.com/anavsgmbh/lidar-warehouse-dataset

## Qualification decision

The AWS RoboMaker Small Warehouse World qualifies for ENV-001 because it is:

1. independently authored and publicly retrievable at an immutable commit;
2. covered by a verified permissive license;
3. a complete structured warehouse environment rather than a demo-assembled rack pair;
4. explicitly metric through SDFormat and COLLADA unit metadata;
5. visibly equipped with a meaningful aisle and separately named shelf geometry;
6. compact enough for a low-risk offline SDF/DAE-to-GLB conversion attempt;
7. frozen by source revision, archive hash, exact scene path, scene hash, and model-manifest hash.

## Next task boundary

ENV-002 may select and preregister one exact aisle segment and two exact boundary entities. Its record must say `Expected outcome: UNKNOWN`. No measurement may run before that file is committed.
