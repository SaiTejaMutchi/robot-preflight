#!/usr/bin/env python3
"""Build the checked-in warehouse proof fixtures from the attributed public rack GLB."""
import argparse
import json
import pathlib
import struct


def read_glb(path):
    data = pathlib.Path(path).read_bytes()
    magic, version, _ = struct.unpack_from("<4sII", data)
    if magic != b"glTF" or version != 2:
        raise ValueError("Expected an uncompressed glTF 2.0 GLB")
    json_len, json_type = struct.unpack_from("<II", data, 12)
    if json_type != 0x4E4F534A:
        raise ValueError("Missing GLB JSON chunk")
    document = json.loads(data[20:20 + json_len].rstrip(b" \0"))
    offset = 20 + json_len
    bin_len, bin_type = struct.unpack_from("<II", data, offset)
    if bin_type != 0x004E4942:
        raise ValueError("Missing GLB binary chunk")
    return document, bytearray(data[offset + 8:offset + 8 + bin_len])


def align4(blob):
    blob.extend(b"\0" * ((-len(blob)) % 4))


def add_cube(document, blob):
    # A shared unit cube. Node transforms define floor/aisle/blocker dimensions.
    positions = [
        (-.5,-.5,-.5),(.5,-.5,-.5),(.5,.5,-.5),(-.5,.5,-.5),
        (-.5,-.5,.5),(.5,-.5,.5),(.5,.5,.5),(-.5,.5,.5),
    ]
    indices = [0,2,1,0,3,2,4,5,6,4,6,7,0,1,5,0,5,4,
               2,3,7,2,7,6,1,2,6,1,6,5,3,0,4,3,4,7]
    align4(blob); pos_offset = len(blob)
    for value in positions: blob.extend(struct.pack("<3f", *value))
    align4(blob); idx_offset = len(blob)
    for value in indices: blob.extend(struct.pack("<H", value))
    views = document.setdefault("bufferViews", [])
    accessors = document.setdefault("accessors", [])
    pos_view = len(views); views.append({"buffer":0,"byteOffset":pos_offset,"byteLength":len(positions)*12,"target":34962})
    idx_view = len(views); views.append({"buffer":0,"byteOffset":idx_offset,"byteLength":len(indices)*2,"target":34963})
    pos_acc = len(accessors); accessors.append({"bufferView":pos_view,"componentType":5126,"count":8,"type":"VEC3","min":[-.5,-.5,-.5],"max":[.5,.5,.5]})
    idx_acc = len(accessors); accessors.append({"bufferView":idx_view,"componentType":5123,"count":len(indices),"type":"SCALAR"})
    materials = document.setdefault("materials", [])
    floor_mat = len(materials); materials.append({"name":"Warehouse_Floor","pbrMetallicRoughness":{"baseColorFactor":[.24,.27,.30,1],"metallicFactor":0,"roughnessFactor":.9}})
    aisle_mat = len(materials); materials.append({"name":"Aisle_Marking","pbrMetallicRoughness":{"baseColorFactor":[.88,.66,.08,1],"metallicFactor":0,"roughnessFactor":.7}})
    blocker_mat = len(materials); materials.append({"name":"Blocker_Region","alphaMode":"BLEND","doubleSided":True,"pbrMetallicRoughness":{"baseColorFactor":[.9,.08,.04,.28],"metallicFactor":0,"roughnessFactor":.6}})
    meshes = document.setdefault("meshes", [])
    result = []
    for name, material in (("Floor_Box",floor_mat),("Aisle_Box",aisle_mat),("Blocker_Box",blocker_mat)):
        result.append(len(meshes)); meshes.append({"name":name,"primitives":[{"attributes":{"POSITION":pos_acc},"indices":idx_acc,"material":material}]})
    return result


def write_glb(document, blob, path):
    align4(blob)
    document["buffers"] = [{"byteLength":len(blob)}]
    raw_json = json.dumps(document, separators=(",", ":")).encode()
    raw_json += b" " * ((-len(raw_json)) % 4)
    total = 12 + 8 + len(raw_json) + 8 + len(blob)
    output = struct.pack("<4sII", b"glTF", 2, total)
    output += struct.pack("<II", len(raw_json), 0x4E4F534A) + raw_json
    output += struct.pack("<II", len(blob), 0x004E4942) + bytes(blob)
    pathlib.Path(path).parent.mkdir(parents=True, exist_ok=True)
    pathlib.Path(path).write_bytes(output)


def build_public(source, output):
    doc, blob = read_glb(source)
    source_node = doc["nodes"][0]
    source_mesh = source_node["mesh"]
    # Original mesh local bounds plus its source node translation produce a rack
    # centered at x=0, z=0 and resting at y=0.
    rack_child = {"name":"Public_Rack_Mesh","mesh":source_mesh,"translation":source_node.get("translation",[0,0,0])}
    floor_mesh, aisle_mesh, blocker_mesh = add_cube(doc, blob)
    clear = 1.75
    rack_depth = .60
    rack_offset = clear / 2 + rack_depth / 2
    doc["nodes"] = [
        {"name":"Warehouse_Public_Source","children":[1,3,5,6,7,8]},
        {"name":"Rack_Left","translation":[0,0,-rack_offset],"children":[2]}, rack_child,
        {"name":"Rack_Right","translation":[0,0,rack_offset],"children":[4]}, dict(rack_child),
        {"name":"Floor","mesh":floor_mesh,"translation":[0,-.05,0],"scale":[8,.1,6]},
        {"name":"Aisle_01","mesh":aisle_mesh,"translation":[0,.006,0],"scale":[4,.012,clear]},
        {"name":"Blocker_Region_01","mesh":blocker_mesh,"translation":[0,.16,0],"scale":[.65,.32,clear]},
        {"name":"Door_Opening_01","mesh":aisle_mesh,"translation":[2.7,1.1,0],"scale":[.08,2.2,clear]},
    ]
    doc["scenes"] = [{"name":"Warehouse_Public_Test","nodes":[0]}]
    doc["scene"] = 0
    doc.setdefault("asset", {})["generator"] = "SpatialGrid WarehouseGeometry fixture generator"
    doc["asset"]["extras"] = {"source":"Industrial Storage Unit 3D Models / SB1 by Daniel Rosehill, CC BY 4.0","units":"meters","expectedClearanceMeters":clear}
    write_glb(doc, blob, output)


def build_controlled(output):
    doc = {"asset":{"version":"2.0","generator":"SpatialGrid WarehouseGeometry controlled fixture","extras":{"units":"meters","expectedClearanceMeters":.940}}}
    blob = bytearray()
    floor_mesh, aisle_mesh, blocker_mesh = add_cube(doc, blob)
    rack_mesh = floor_mesh
    width = .940; depth = .60; offset = width/2 + depth/2
    doc["nodes"] = [
        {"name":"Warehouse_Controlled_0_940m","children":[1,2,3,4,5]},
        {"name":"Rack_Left","mesh":rack_mesh,"translation":[0,1.15,-offset],"scale":[1.6,2.3,depth]},
        {"name":"Rack_Right","mesh":rack_mesh,"translation":[0,1.15,offset],"scale":[1.6,2.3,depth]},
        {"name":"Aisle_01","mesh":aisle_mesh,"translation":[0,.006,0],"scale":[3,.012,width]},
        {"name":"Blocker_Region_01","mesh":blocker_mesh,"translation":[0,.2,0],"scale":[.65,.4,width]},
        {"name":"Floor","mesh":floor_mesh,"translation":[0,-.05,0],"scale":[5,.1,4]},
    ]
    doc["scenes"] = [{"name":"Controlled_Clearance_Test","nodes":[0]}]; doc["scene"] = 0
    write_glb(doc, blob, output)


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--source", required=True)
    parser.add_argument("--output-dir", required=True)
    args = parser.parse_args()
    out = pathlib.Path(args.output_dir)
    build_public(args.source, out / "warehouse_source.glb")
    build_controlled(out / "warehouse_controlled_0_940m.glb")


if __name__ == "__main__": main()
