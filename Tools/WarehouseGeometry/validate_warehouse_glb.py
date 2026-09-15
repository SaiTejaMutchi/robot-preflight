#!/usr/bin/env python3
"""Offline structural and bounds validation for the two checked-in GLBs."""
import argparse, json, pathlib, struct

def load(path):
    raw=pathlib.Path(path).read_bytes(); magic,ver,total=struct.unpack_from('<4sII',raw)
    assert magic==b'glTF' and ver==2 and total==len(raw)
    size,kind=struct.unpack_from('<II',raw,12); assert kind==0x4e4f534a
    return raw,json.loads(raw[20:20+size].rstrip(b' \0'))

def bounds(doc,index,parent_t=(0,0,0),parent_s=(1,1,1)):
    n=doc['nodes'][index]; t=n.get('translation',[0,0,0]); s=n.get('scale',[1,1,1])
    wt=tuple(parent_t[i]+t[i]*parent_s[i] for i in range(3)); ws=tuple(parent_s[i]*s[i] for i in range(3))
    found=[]
    if 'mesh' in n:
        for p in doc['meshes'][n['mesh']]['primitives']:
            a=doc['accessors'][p['attributes']['POSITION']]
            lo=tuple(wt[i]+a['min'][i]*ws[i] for i in range(3)); hi=tuple(wt[i]+a['max'][i]*ws[i] for i in range(3))
            found.append((tuple(min(lo[i],hi[i]) for i in range(3)),tuple(max(lo[i],hi[i]) for i in range(3))))
    for child in n.get('children',[]): found += bounds(doc,child,wt,ws)
    return found

def combined(items):
    assert items
    return tuple(min(x[0][i] for x in items) for i in range(3)),tuple(max(x[1][i] for x in items) for i in range(3))

def inspect(path):
    raw,doc=load(path); by_name={n.get('name'):i for i,n in enumerate(doc['nodes'])}
    required={'Rack_Left','Rack_Right','Aisle_01','Blocker_Region_01'}; assert required <= set(by_name)
    left=combined(bounds(doc,by_name['Rack_Left'])); right=combined(bounds(doc,by_name['Rack_Right']))
    if left[0][2] > right[0][2]: left,right=right,left
    available=max(0,right[0][2]-left[1][2]); needed=1.915; difference=available-needed
    extensions=doc.get('extensionsUsed',[])
    assert not any(x in extensions for x in ('EXT_meshopt_compression','KHR_draco_mesh_compression'))
    triangles=sum(doc['accessors'][p['indices']]['count']//3 for m in doc['meshes'] for p in m['primitives'])
    return {'file':str(path),'bytes':len(raw),'nodes':len(doc['nodes']),'unique_mesh_triangles':triangles,
      'materials':len(doc.get('materials',[])),'textures':len(doc.get('textures',[])),'available_m':round(available,3),
      'required_m':needed,'difference_m':round(difference,3),'status':'BLOCKED' if difference<-.005 else 'PASS' if difference>.005 else 'REVIEW'}

if __name__=='__main__':
    p=argparse.ArgumentParser(); p.add_argument('files',nargs='+'); a=p.parse_args()
    print(json.dumps([inspect(x) for x in a.files],indent=2))
