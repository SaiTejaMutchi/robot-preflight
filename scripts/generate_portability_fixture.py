#!/usr/bin/env python3
"""Generate a minimal synthetic binary glTF 2.0 (.glb) fixture for portability testing.

This fixture contains two distinct box entities ("PortabilityRack_A" and "PortabilityRack_B")
separated by a known, deterministic clearance along the X axis:
  - PortabilityRack_A X bounds: [0.000000, 1.000000] m
  - PortabilityRack_B X bounds: [2.600000, 3.600000] m
  - Nearest-face gap along X: 2.600000 - 1.000000 = 1.600000 m

When evaluated against OTTO 1500 aisle clearance (1.915000 m, tolerance 0.005000 m),
the margin is 1.600000 - 1.915000 = -0.315000 m, which evaluates deterministically
to BLOCKED.

This fixture is synthetic and intended purely as a software portability smoke test,
proving that the verifier functions dynamically on a separate GLB artifact with
different geometry and entity names. It is NOT an independently evidenced facility.
"""
from __future__ import annotations

import json
import pathlib
import struct

REPO_ROOT = pathlib.Path(__file__).resolve().parent.parent
OUTPUT_PATH = REPO_ROOT / "examples" / "portability_facility" / "portable_fixture.glb"


def create_box_mesh(min_pt: tuple[float, float, float], max_pt: tuple[float, float, float]):
    x0, y0, z0 = min_pt
    x1, y1, z1 = max_pt
    vertices = [
        (x0, y0, z0), (x1, y0, z0), (x1, y1, z0), (x0, y1, z0),
        (x0, y0, z1), (x1, y0, z1), (x1, y1, z1), (x0, y1, z1),
    ]
    indices = [
        0, 2, 1, 0, 3, 2,  # -Z
        4, 5, 6, 4, 6, 7,  # +Z
        0, 1, 5, 0, 5, 4,  # -Y
        2, 3, 7, 2, 7, 6,  # +Y
        0, 4, 7, 0, 7, 3,  # -X
        1, 2, 6, 1, 6, 5,  # +X
    ]
    return vertices, indices


def build_glb(output_path: pathlib.Path) -> bytes:
    # Rack A: [0.0, 0.0, 0.0] to [1.0, 2.0, 1.0]
    # Rack B: [2.6, 0.0, 0.0] to [3.6, 2.0, 1.0]
    va, ia = create_box_mesh((0.0, 0.0, 0.0), (1.0, 2.0, 1.0))
    vb, ib = create_box_mesh((2.6, 0.0, 0.0), (3.6, 2.0, 1.0))

    bin_data = bytearray()

    def pack_geometry(verts, inds, min_pt, max_pt):
        v_offset = len(bin_data)
        for v in verts:
            bin_data.extend(struct.pack("<fff", *v))
        v_len = len(bin_data) - v_offset
        while len(bin_data) % 4 != 0:
            bin_data.append(0)

        i_offset = len(bin_data)
        for idx in inds:
            bin_data.extend(struct.pack("<H", idx))
        i_len = len(bin_data) - i_offset
        while len(bin_data) % 4 != 0:
            bin_data.append(0)

        return (v_offset, v_len, len(verts), min_pt, max_pt), (i_offset, i_len, len(inds))

    va_info, ia_info = pack_geometry(va, ia, [0.0, 0.0, 0.0], [1.0, 2.0, 1.0])
    vb_info, ib_info = pack_geometry(vb, ib, [2.6, 0.0, 0.0], [3.6, 2.0, 1.0])

    buffer_views = [
        {"buffer": 0, "byteOffset": va_info[0], "byteLength": va_info[1], "target": 34962},
        {"buffer": 0, "byteOffset": ia_info[0], "byteLength": ia_info[1], "target": 34963},
        {"buffer": 0, "byteOffset": vb_info[0], "byteLength": vb_info[1], "target": 34962},
        {"buffer": 0, "byteOffset": ib_info[0], "byteLength": ib_info[1], "target": 34963},
    ]

    accessors = [
        {"bufferView": 0, "byteOffset": 0, "componentType": 5126, "count": va_info[2], "type": "VEC3", "min": va_info[3], "max": va_info[4]},
        {"bufferView": 1, "byteOffset": 0, "componentType": 5123, "count": ia_info[2], "type": "SCALAR", "min": [0], "max": [7]},
        {"bufferView": 2, "byteOffset": 0, "componentType": 5126, "count": vb_info[2], "type": "VEC3", "min": vb_info[3], "max": vb_info[4]},
        {"bufferView": 3, "byteOffset": 0, "componentType": 5123, "count": ib_info[2], "type": "SCALAR", "min": [0], "max": [7]},
    ]

    meshes = [
        {"name": "PortabilityRack_A_Mesh", "primitives": [{"attributes": {"POSITION": 0}, "indices": 1}]},
        {"name": "PortabilityRack_B_Mesh", "primitives": [{"attributes": {"POSITION": 2}, "indices": 3}]},
    ]

    nodes = [
        {"name": "PortabilityRack_A", "mesh": 0},
        {"name": "PortabilityRack_B", "mesh": 1},
    ]

    scene = {"nodes": [0, 1]}

    gltf_dict = {
        "asset": {"version": "2.0", "generator": "scripts/generate_portability_fixture.py"},
        "scene": 0,
        "scenes": [scene],
        "nodes": nodes,
        "meshes": meshes,
        "accessors": accessors,
        "bufferViews": buffer_views,
        "buffers": [{"byteLength": len(bin_data)}],
    }

    json_bytes = json.dumps(gltf_dict, separators=(",", ":")).encode("utf-8")
    while len(json_bytes) % 4 != 0:
        json_bytes += b" "

    glb = bytearray()
    glb.extend(b"glTF")
    glb.extend(struct.pack("<I", 2))
    total_length = 12 + 8 + len(json_bytes) + 8 + len(bin_data)
    glb.extend(struct.pack("<I", total_length))

    # JSON chunk
    glb.extend(struct.pack("<I", len(json_bytes)))
    glb.extend(b"JSON")
    glb.extend(json_bytes)

    # BIN chunk
    glb.extend(struct.pack("<I", len(bin_data)))
    glb.extend(b"BIN\x00")
    glb.extend(bin_data)

    output_path.parent.mkdir(parents=True, exist_ok=True)
    output_path.write_bytes(glb)
    return bytes(glb)


def main() -> None:
    glb = build_glb(OUTPUT_PATH)
    print(f"Generated portability fixture: {OUTPUT_PATH} ({len(glb)} bytes)")


if __name__ == "__main__":
    main()
