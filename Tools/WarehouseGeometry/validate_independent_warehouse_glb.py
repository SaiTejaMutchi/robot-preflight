#!/usr/bin/env python3
"""Offline structural and bounds validation for the GEO-003 independent-environment GLB.

Companion to validate_warehouse_glb.py, which is hardcoded to the two controlled
fixtures' node names (Rack_Left/Rack_Right/Aisle_01/Blocker_Region_01) and the Z
axis. This script targets the exact boundary node names and measurement axis
frozen in evidence/environments/warehouse_measurement_preregistration.md for the
independent AWS RoboMaker Small Warehouse World environment:
  Boundary A: aws_robomaker_warehouse_ShelfF_01_001
  Boundary B: aws_robomaker_warehouse_ShelfD_01_001
  Axis: X (glTF/Unity X, index 0)
It does not modify or replace validate_warehouse_glb.py.
"""
import argparse, json, pathlib, struct

def load(path):
    raw = pathlib.Path(path).read_bytes()
    magic, ver, total = struct.unpack_from('<4sII', raw)
    assert magic == b'glTF' and ver == 2 and total == len(raw)
    size, kind = struct.unpack_from('<II', raw, 12)
    assert kind == 0x4e4f534a
    return raw, json.loads(raw[20:20 + size].rstrip(b' \0'))

def mat_identity():
    return [1,0,0,0, 0,1,0,0, 0,0,1,0, 0,0,0,1]

def mat_mul(a, b):
    # column-major 4x4, glTF convention: result = a * b
    r = [0.0]*16
    for col in range(4):
        for row in range(4):
            s = 0.0
            for k in range(4):
                s += a[k*4+row]*b[col*4+k]
            r[col*4+row] = s
    return r

def mat_from_trs(t, r, s):
    tx,ty,tz = t or [0,0,0]
    sx,sy,sz = s or [1,1,1]
    qx,qy,qz,qw = r or [0,0,0,1]
    # quaternion -> rotation matrix
    xx,yy,zz = qx*qx, qy*qy, qz*qz
    xy,xz,yz = qx*qy, qx*qz, qy*qz
    wx,wy,wz = qw*qx, qw*qy, qw*qz
    r00 = 1-2*(yy+zz); r01 = 2*(xy+wz);   r02 = 2*(xz-wy)
    r10 = 2*(xy-wz);   r11 = 1-2*(xx+zz); r12 = 2*(yz+wx)
    r20 = 2*(xz+wy);   r21 = 2*(yz-wx);   r22 = 1-2*(xx+yy)
    # apply scale then rotation, column-major
    return [
        r00*sx, r01*sx, r02*sx, 0,
        r10*sy, r11*sy, r12*sy, 0,
        r20*sz, r21*sz, r22*sz, 0,
        tx, ty, tz, 1,
    ]

def node_local_matrix(n):
    if 'matrix' in n:
        return list(n['matrix'])
    return mat_from_trs(n.get('translation'), n.get('rotation'), n.get('scale'))

def mat_transform_point(m, p):
    x,y,z = p
    ox = m[0]*x + m[4]*y + m[8]*z  + m[12]
    oy = m[1]*x + m[5]*y + m[9]*z  + m[13]
    oz = m[2]*x + m[6]*y + m[10]*z + m[14]
    return (ox, oy, oz)

def bounds(doc, index, parent_m=None):
    if parent_m is None:
        parent_m = mat_identity()
    n = doc['nodes'][index]
    wm = mat_mul(parent_m, node_local_matrix(n))
    found = []
    if 'mesh' in n:
        for p in doc['meshes'][n['mesh']]['primitives']:
            a = doc['accessors'][p['attributes']['POSITION']]
            lo_local, hi_local = a['min'], a['max']
            corners = [(lo_local[0] if bx==0 else hi_local[0],
                        lo_local[1] if by==0 else hi_local[1],
                        lo_local[2] if bz==0 else hi_local[2])
                       for bx in (0,1) for by in (0,1) for bz in (0,1)]
            world_corners = [mat_transform_point(wm, c) for c in corners]
            lo = tuple(min(c[i] for c in world_corners) for i in range(3))
            hi = tuple(max(c[i] for c in world_corners) for i in range(3))
            found.append((lo, hi))
    for child in n.get('children', []):
        found += bounds(doc, child, wm)
    return found

def combined(items):
    assert items
    return (tuple(min(x[0][i] for x in items) for i in range(3)),
            tuple(max(x[1][i] for x in items) for i in range(3)))

def inspect(path, boundary_a, boundary_b, axis_index, required_m, tolerance_m):
    raw, doc = load(path)
    by_name = {n.get('name'): i for i, n in enumerate(doc['nodes'])}
    required_names = {boundary_a, boundary_b}
    assert required_names <= set(by_name), f"Missing node(s): {required_names - set(by_name)}"
    # fail on duplicate/ambiguous names, same rule as WarehouseEntityRegistry.TryResolveUnique
    name_counts = {}
    for n in doc['nodes']:
        nm = n.get('name')
        if nm is not None:
            name_counts[nm] = name_counts.get(nm, 0) + 1
    for nm in required_names:
        assert name_counts[nm] == 1, f"Node name '{nm}' is not unique ({name_counts[nm]} occurrences)"

    a = combined(bounds(doc, by_name[boundary_a]))
    b = combined(bounds(doc, by_name[boundary_b]))
    if a[0][axis_index] > b[0][axis_index]:
        a, b = b, a
    available = max(0.0, b[0][axis_index] - a[1][axis_index])
    difference = available - required_m
    status = 'REVIEW' if abs(difference) <= tolerance_m else ('BLOCKED' if difference < 0 else 'PASS')
    extensions = doc.get('extensionsUsed', [])
    assert not any(x in extensions for x in ('EXT_meshopt_compression', 'KHR_draco_mesh_compression'))
    triangles = sum(doc['accessors'][p['indices']]['count'] // 3
                     for m in doc['meshes'] for p in m['primitives'] if 'indices' in p)
    return {
        'file': str(path), 'bytes': len(raw), 'nodes': len(doc['nodes']),
        'unique_mesh_triangles': triangles, 'materials': len(doc.get('materials', [])),
        'textures': len(doc.get('textures', [])),
        'boundary_a': boundary_a, 'boundary_b': boundary_b, 'axis_index': axis_index,
        'boundary_a_bounds': a, 'boundary_b_bounds': b,
        'available_m': round(available, 6), 'required_m': required_m,
        'difference_m': round(difference, 6), 'tolerance_m': tolerance_m, 'status': status,
    }

if __name__ == '__main__':
    p = argparse.ArgumentParser()
    p.add_argument('file')
    p.add_argument('--boundary-a', default='aws_robomaker_warehouse_ShelfF_01_001')
    p.add_argument('--boundary-b', default='aws_robomaker_warehouse_ShelfD_01_001')
    p.add_argument('--axis', default='X', choices=['X', 'Y', 'Z'])
    p.add_argument('--required', type=float, default=1.915)
    p.add_argument('--tolerance', type=float, default=0.005)
    a = p.parse_args()
    axis_index = {'X': 0, 'Y': 1, 'Z': 2}[a.axis]
    print(json.dumps(inspect(a.file, a.boundary_a, a.boundary_b, axis_index, a.required, a.tolerance), indent=2))
