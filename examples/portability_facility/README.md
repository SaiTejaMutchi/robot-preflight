# Portability Smoke Test: OTTO 1500 vs. Portable Fixture

This example is a **portability smoke test**, not an independently validated deployment.

Its sole purpose is to prove software generality: that the existing `aisle_clearance` verifier runs dynamically against a second facility artifact with different geometry and entity names, without modifying verifier logic or reading any precomputed result.

| Parameter | Value |
|---|---|
| Evidence Tier | **Portability Smoke Test** (Software Generality) |
| Robot | OTTO 1500 (`otto_1500`) |
| Constraint | Minimum one-way aisle width (`aisle_clearance`) |
| Required | 1.915000 m |
| Measured | 1.600000 m |
| Margin | -0.315000 m |
| Decision | **BLOCKED** |
| Facility Artifact | `portable_fixture.glb` (1.5 KB synthetic glTF 2.0 binary) |
| Entity Pair | `PortabilityRack_A` vs. `PortabilityRack_B` along the X axis |
| Generation Script | `scripts/generate_portability_fixture.py` |

---

## Why this result is BLOCKED

The canonical reference example ([`../otto1500_warehouse`](../otto1500_warehouse)) evaluates to `PASS` (7.509662 m available vs. 1.915 m required).

This portability fixture is intentionally configured with a narrower aisle gap (1.600000 m), which is below the 1.915000 m requirement and outside the +/-0.005 m review tolerance. This demonstrates that:
1. Different geometry dynamically produces a different numeric measurement.
2. The `BLOCKED` decision is computed from raw geometry bounds, not hard-coded.
3. The same verifier logic handles both sides of the constraint threshold.

---

## Run this check

Via the CLI:

```bash
python -m robot_preflight check examples/portability_facility
```

Expected output:

```text
Robot Preflight

Robot        otto_1500
Constraint   aisle_clearance

Required     1.915000 m
Available    1.600000 m
Margin       -0.315000 m

Decision     BLOCKED
```

Via the Python SDK:

```python
from robot_preflight import Preflight

result = Preflight.check(
    robot="otto_1500",
    facility="examples/portability_facility",
    constraint="aisle_clearance",
)

assert result.decision == "BLOCKED"
assert result.available_m == 1.6
assert result.facility_entities == ["PortabilityRack_A", "PortabilityRack_B"]
```

---

## Contrast with Reference Verification

| Attribute | Reference Example (`otto1500_warehouse`) | Portability Smoke Test (`portability_facility`) |
|---|---|---|
| **Purpose** | External deployment validity | Software generality and verifier portability |
| **Facility Origin** | Independently sourced AWS RoboMaker world | Minimal synthetic fixture |
| **Evidence Quality** | Dual-engine cross-validation (Unity + Python within 1 µm), hashed manufacturer PDF | Deterministic test fixture |
| **Decision** | **PASS** | **BLOCKED** |
