# Example: OTTO 1500 vs. AWS RoboMaker warehouse

This is the canonical, only example currently shipped. It is the same
robot/facility/constraint combination behind the frozen result in
[`../../evidence/results/otto1500_warehouse.json`](../../evidence/results/otto1500_warehouse.json).

Run it:

```bash
python -m robot_preflight check examples/otto1500_warehouse
```

or from Python:

```python
from robot_preflight import Preflight

result = Preflight.check(
    robot="otto_1500",
    facility="examples/otto1500_warehouse",
    constraint="aisle_clearance",
)
print(result)
```

Both paths re-parse [`config.yaml`](config.yaml) and the real facility file
it points at, then recompute the measurement from the raw glTF geometry.
Neither path reads `expected_result.json` or the frozen result record --
those are reference files for a human (or `scripts/verify_reference_result.py`)
to compare the fresh computation against, not inputs to the computation.

`expected_result.json` in this directory is a small excerpt of the frozen
values for quick comparison; the authoritative frozen record with full
provenance, hashes, and cross-validation is
[`evidence/results/otto1500_warehouse.json`](../../evidence/results/otto1500_warehouse.json).

What this example does **not** cover: any other robot, any other facility,
any other constraint type (turning clearance, docking, doorway clearance,
route feasibility), or the newer AUG2026 OTTO 1500 document revision (see
[`evidence/requirements/otto/OTTO_EVIDENCE_RECORD_AUG2026.md`](../../evidence/requirements/otto/OTTO_EVIDENCE_RECORD_AUG2026.md)).
Those are out of scope for this repository today.
