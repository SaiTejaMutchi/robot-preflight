# Warehouse geometry proof assets

`warehouse_source.glb` is a deterministic test scene assembled from the public
SB1 industrial rack model in Daniel Rosehill's **Industrial Storage Unit 3D
Models** repository. The source asset is CC BY 4.0, modeled at real scale, and
is used twice under the stable logical nodes `Rack_Left` and `Rack_Right`.

- Source: https://github.com/danielrosehill/storage-box-3d-models
- Source file: `models/SB1/SB1.glb`
- Author: Daniel Rosehill
- License: CC BY 4.0 (see `THIRD_PARTY_NOTICE.md`)
- Classification: public synthetic warehouse component, not a captured facility dataset
- Public fixture measured rack-to-rack clearance: 1.750 m
- Controlled fixture measured rack-to-rack clearance: 0.940 m
- OTTO 1500 comparison requirement: 1.915 m

The GLBs are uncompressed, self-contained, texture-free, in meters, and place
their floors at Y=0. They are regenerated with:

```sh
python3 Tools/WarehouseGeometry/generate_warehouse_glb.py \
  --source /path/to/SB1.glb \
  --output-dir Assets/StreamingAssets/WarehouseGeometry
```
