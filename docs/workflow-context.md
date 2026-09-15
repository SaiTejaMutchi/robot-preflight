# AMR Deployment Workflow Context

This document defines how `robot-preflight` sits within the real autonomous
mobile robot (AMR) deployment lifecycle, where its technical boundaries lie today,
and what remains downstream.

## The 12-Stage AMR Deployment Lifecycle

Deploying an autonomous mobile robot fleet into an active warehouse or
manufacturing facility is an iterative, multi-stage engineering process:

1. **Mission Definition** — operational objectives, material throughput, pick/drop endpoints.
2. **Facility Representation** — architectural drawings, CAD models, 2D floor plans, lidar scans.
3. **Robot & Envelope Definition** — base vehicle, attachments, payload dimensions, operating limits.
4. **Feasibility Study & Simulation** — physics/traffic simulation (Isaac Sim, Gazebo, Webots), cycle times.
5. **Detailed Deployment Design** — network infrastructure, safety zones, charger placements, route networks.
6. **Site Preparation** — physical marking, barcode/reflector installation, Wi-Fi validation.
7. **OEM Fleet Configuration** — mapping, zone declarations, dispatch rules (e.g. OTTO Fleet Manager, MiR Fleet).
8. **Systems Integration** — PLC, WCS, WMS, MES, and elevator/door integration.
9. **Commissioning** — physical floor trials, sensor validation, localization tuning.
10. **Workflow Validation & Stabilization** — throughput soak testing, bottleneck analysis, exception handling.
11. **Production Operations** — live fleet dispatch, continuous material movement.
12. **Continuous Improvement** — layout changes, route optimization, fleet expansion.

---

## Where Robot Preflight Fits

Robot Preflight structures one part of the reconciliation work that currently happens before simulation and commissioning.

```
Stage 1: Mission Definition (what the operation needs)
Stage 2: Facility Representation (what the facility contains)
Stage 3: Robot & Envelope Definition (what the robot can do)
                 │
                 ▼
      [ ROBOT PREFLIGHT ]
      • Sourced robot requirements (curated)
      • Facility evidence (GLB today)
      • Manual entity selection (today)
      • Verifier Registry (Verifier #1: aisle clearance)
      • PASS / BLOCKED / REVIEW decision model
                 │
                 ▼
Stage 4: Feasibility & Simulation
                 │
                 ▼
Stage 5+: Detailed Design, OEM Configuration, Commissioning
```

Robot Preflight conceptually sits **between Stages 1–3 and Stage 4**.

Robot Preflight turns sourced robot requirements and facility evidence into
inspectable deployment constraint checks before simulation and commissioning.
Task-aware grounding and multi-constraint preflight are next.

### What Robot Preflight Does Not Replace

Robot Preflight does **not** replace:
* **Physics & Traffic Simulation**: It does not simulate dynamics, wheel slip, battery drain, or traffic contention.
* **OEM Fleet Managers**: It does not dispatch vehicles, program fleet routes, or edit OEM operational maps.
* **Navigation Stacks (Nav2 / Open-RMF)**: It does not generate global costmaps or trajectory plans.
* **Safety Certification**: It does not certify compliance with ISO 3691-4 or ANSI/RIA R15.08.

Its job is to make physical assumptions explicit and catch spatial conflicts early.

---

## Facility Evidence and the SignalWeave Ecosystem

Robot Preflight currently consumes structured GLB geometry. Real AMR deployments may start from CAD, maps, scans, robot-generated maps, or other facility representations.

SignalWeave is building technical pieces inside the existing AMR deployment workflow:

* [`spatial-ai`](https://github.com/SaiTejaMutchi/spatial-ai) structures physical facility state from mobile RGB-D and LiDAR observations into persistent physical entities (`wall-001`, `floor-001`), metric geometry, and provenance-linked measurements.
* [`robot-preflight`](https://github.com/SaiTejaMutchi/robot-preflight) verifies robot deployment constraints against facility state before simulation and commissioning.

```
       [ SPATIAL-AI ]                    [ ROBOT-PREFLIGHT ]
RGB-D / LiDAR observations        Sourced robot requirements + Facility evidence
           │                                      │
           ▼                                      ▼
Structured facility state  ──(future)──>  Deterministic constraint verification
(what physically exists)                  (does robot requirement agree with evidence?)
```

> [!IMPORTANT]
> **No Runtime Integration Today**: Facility evidence may come from existing GLB/CAD/map assets or from systems that construct structured spatial state. SignalWeave's `spatial-ai` explores one such path from RGB-D/LiDAR observations to measurable physical entities. Robot Preflight consumes structured facility evidence and verifies robot deployment constraints against it. Today these repositories are separate open-source components with **no automatic runtime integration**.

---

## The Six Truths of AMR Deployment

In an industrial facility, the same deployment is represented across six distinct
sources of truth:

| Truth | Common Representation | Typical Owner |
|---|---|---|
| **1. What the operation needs** | Workflows, throughput, endpoints | Industrial Engineering |
| **2. What the facility contains** | CAD, glTF models, scans, layouts | Facilities / Real Estate |
| **3. What the robot can do** | Spec sheets, payload envelopes, limits | Robot OEM |
| **4. What simulation predicts** | Kinematics, cycle times, traffic | Automation Engineering |
| **5. What the fleet is configured to do** | Zones, restrictions, endpoints | Field Applications |
| **6. What actually happens on the floor** | Telemetry, faults, interventions | Operations |

### Mapping the Six Truths to SignalWeave Components

* **`spatial-ai` primarily contributes to Truth 2**: it reconstructs persistent physical entities and metric geometry from sensor observations.
* **`robot-preflight` starts reconciling Truth 2 and Truth 3**: it deterministically checks whether sourced robot requirements are satisfied by facility evidence.
* **Mission context from Truth 1 is still mostly external and manual today**: the user designates target span entities and coordinate axes.
* **Truths 4–6 remain downstream**: simulation predictions (Truth 4), fleet manager zone configurations (Truth 5), and live operational telemetry (Truth 6) occur after preflight verification.

---

## Current Public Boundary vs. Future Direction

| Capability | Current State in Repository | Roadmap / Future Direction |
|---|---|---|
| **Constraint Verifiers** | Exactly one: Selected-span aisle clearance (**Verifier #1**) | Vertical clearance, door width, turning radius, floor slope |
| **Grounding** | Explicit manual node specification in `config.yaml` | Automated route/zone grounding from semantic task definitions |
| **Facility Input** | Standard uncompressed binary glTF 2.0 (`.glb`) | CAD (STEP/DWG), 2D maps, lidar point clouds |
| **Robot Input** | Curated lookup citing hashed manufacturer PDFs | Automated cited specification extraction from PDF |
| **Downstream Output** | Structured JSON with complete provenance trail | Direct export to Nav2 costmap restrictions, Isaac Sim configs |
| **Decision Vocabulary** | PASS, BLOCKED, REVIEW arithmetic implemented & verified | Multi-constraint aggregate preflight scoring |

See [`current-capabilities.md`](current-capabilities.md) for the complete capability matrix and code audit.
