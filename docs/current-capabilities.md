# Robot Preflight: Current Capabilities Audit

This document records the exact capability boundary of the `robot-preflight`
repository based on code inspection across Python, Unity, schemas, and evidence files.

## Product Maturity Level

**Architecture maturity: LEVEL 2**  
**Evidence breadth: LEVEL 1**

* **Architecture Maturity — LEVEL 2**: An extensible deployment-constraint verification framework. The verifier registry (`robot_preflight.verifiers`), abstract verifier interface (`BaseVerifier`), structured result contract (`PreflightResult`), and constraint dispatch mechanism exist in code today.
* **Evidence Breadth — LEVEL 1**: Only one verifier has full end-to-end external reference evidence: selected-span aisle clearance (Verifier #1: OTTO 1500 in AWS RoboMaker warehouse, cross-validated with Unity within 1 µm).
* **Roadmap — LEVEL 3**: Task-aware grounding and multi-constraint preflight (ingesting mission semantics and performing automated entity grounding to facility geometry).
* **Future Vision — LEVEL 4**: Downstream deployment compilation (emitting validated configurations for planners, simulators, and OEM fleet managers).

> [!IMPORTANT]
> **Why Level 2 Architecture**: Verifier registry, base interface, structured result contract, and constraint type dispatch exist in active code.  
> **Why Level 1 Evidence Breadth**: Only aisle clearance has an externally validated reference example with frozen physical evidence and dual-engine cross-validation.  
> Do not describe the system as Level 3 (no automated task/mission grounding exists). Do not claim automatic deployment qualification.

---

## Capability Matrix

| Capability Area | Specific Capability | Status | Code / Artifact Evidence | Public Claim Allowed |
|---|---|---|---|---|
| **A. Robot Requirements** | Sourced requirement lookup | **VERIFIED** | `robot_preflight/core.py` (`_ROBOT_REQUIREMENTS` table citations) | Yes — curated lookup with cited, hashed manufacturer PDFs |
| | Automated PDF / text ingestion | **NOT IMPLEMENTED** | No PDF parser or OCR pipeline in Python code; PDFs in `evidence/` are manually curated | No — do not claim automated extraction from documents |
| | Preservation of quote, page, tolerance | **VERIFIED** | `evidence/results/otto1500_warehouse.json`, `PreflightResult.requirement_source` | Yes — preserved in evidence record and result string |
| **B. Robot Configuration** | Scalar robot model requirement | **VERIFIED** | `robot: id: otto_1500`, `1.915 m` in `config.yaml` and `core.py` | Yes — scalar dimension linked to robot model |
| | Combined envelope (attachment + payload) | **NOT IMPLEMENTED** | No payload or attachment data fields in Python or Unity models | No — roadmap only |
| **C. Facility Representation** | Binary glTF 2.0 (`.glb`) parsing | **VERIFIED** | `Tools/WarehouseGeometry/validate_independent_warehouse_glb.py:inspect`, glTFast in Unity | Yes — parses standard uncompressed binary glTF 2.0 |
| | Arbitrary GLB consumption | **PARTIAL** | General parser exists, but requires unique node names and uncompressed meshes | Scoped — supports standard GLBs with unique node names |
| | Second-artifact portability | **VERIFIED** | `examples/portability_facility`, `test_second_facility_artifact_portability` | Yes — same verifier demonstrated across multiple compatible facility artifacts |
| | CAD / 2D map / scan ingestion | **NOT IMPLEMENTED** | Only `.glb` files supported; no STEP, DWG, DXF, or point-cloud loaders | No — GLB-only today |
| **D. Deployment Task** | Task & route semantics (start, goal, zones) | **NOT IMPLEMENTED** | Zero task, waypoint, or route fields in schemas or runtime | No — task context is absent |
| **E. Grounding** | Automatic entity identification | **NOT IMPLEMENTED** | User must manually specify `entity_a` and `entity_b` node names in `config.yaml` | No — grounding is explicit and manual |
| | Semantic zone / aisle resolution | **NOT IMPLEMENTED** | Relies on exact glTF node names, not semantic labels | No — must disclose manual node specification |
| **F. Constraint Model** | Single constraint data contract | **IMPLEMENTED** | `config.yaml`, `PreflightResult` dataclass, `CompatibilityResult` struct | Yes — structured single-check contract with metadata |
| | Polymorphic constraint engine | **PARTIAL** | Single constraint type (`aisle_clearance`) currently active | Disclose as initial constraint type |
| **G. Verifier Registry** | Pluggable verifier interface | **IMPLEMENTED** | `robot_preflight/verifiers/` modular verifier pattern | Yes — extensible verifier architecture |
| | Active verified implementations | **VERIFIED (1)** | Verifier #1: Selected-span aisle clearance | Exactly one verifier verified end-to-end |
| **H. Decision Model** | Decision model implemented | **IMPLEMENTED** | `robot_preflight/core.py`, `AisleClearanceVerifier`: PASS / BLOCKED / REVIEW arithmetic | Yes — arithmetic implemented and verified for all 3 states |
| | Independently evidenced reference result | **VERIFIED (1)** | `evidence/results/otto1500_warehouse.json`: PASS reference result | Yes — reference PASS result independently validated |
| **I. Evidence Model** | Complete provenance trail | **VERIFIED** | `PreflightResult.to_dict()`, `evidence/results/otto1500_warehouse.json` | Yes — stores hashes, nodes, bounds, methods, sources |
| **J. Downstream Output** | Machine-readable JSON artifact | **VERIFIED** | `robot-preflight check --json`, `PreflightResult.to_dict()` | Yes — structured JSON output |
| | Planner / Nav2 / Open-RMF / OEM writeback | **NOT IMPLEMENTED** | No exporter for Nav2, ROS, Open-RMF, or OEM tools | No — downstream integration is roadmap |
| **K. Requalification** | Change detection / check invalidation | **NOT IMPLEMENTED** | No dependency graph or cache invalidation logic | No — runs on explicit invocation |

---

## Detailed Audit Findings

### 1. What is Verified Today
* A deterministic aisle-clearance measurement between two named glTF nodes along a specified axis.
* Sourced manufacturer requirement citation: OTTO 1500 minimum one-way aisle width (`1.915 m`), sourced from document `OTTO-DS001D-EN-APR2024`, page 1.
* Independent cross-validation: Unity 6 Play Mode (`7.509663 m`) and Python glTF binary parser (`7.509662 m`) agree to within 1 µm on the frozen AWS RoboMaker warehouse environment.
* Provenance and evidence preservation: Input file hashes, glTF byte counts, node counts, triangle counts, and exact calculation steps are captured in structured JSON.
* CLI and Python SDK: `robot-preflight check <dir>` and `Preflight.check(...)` execute dynamically without reading stored results.

### 2. What is Implemented but Narrow
* **Decision Vocabulary**: PASS, BLOCKED, and REVIEW states are implemented in the arithmetic of both Python and C# engines.
* **glTF Binary Parser**: General glTF 2.0 parsing logic exists in pure Python, but depends on node names being unique and uncompressed accessor buffers.
* **CLI Exit Codes**: CLI returns `0` on PASS, `1` on BLOCKED or REVIEW, and `2` on execution error, suitable for basic CI gating.
* **Portability Smoke Testing**: A synthetic facility fixture (`examples/portability_facility/portable_fixture.glb`) with distinct entities (`PortabilityRack_A`, `PortabilityRack_B`) verifies that the aisle clearance verifier executes dynamically against a separate GLB artifact and evaluates to BLOCKED, without altering verifier logic or reading precomputed data.

### 3. What is Manual Today
* **Requirement Entry**: Requirements are not extracted by an automated agent or PDF ingestion pipeline; they are manually curated into `_ROBOT_REQUIREMENTS` and verified against hashed evidence records.
* **Grounding**: The system does not automatically inspect a facility and determine which racks form a critical aisle. The user must manually supply the exact node IDs (`entity_a` and `entity_b`).

### 4. What is Not Implemented (Roadmap)
* Automated CAD, IFC, or 2D floor-plan ingestion.
* Mission, workflow, or route-level task models.
* Dynamic attachment and payload bounding-envelope calculation.
* Native exporters or integration plugins for Nav2, Open-RMF, Gazebo, Isaac Sim, or OEM fleet managers.
* Automatic change detection and selective requalification.
