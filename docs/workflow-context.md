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

```
Stage 1: Mission Definition
Stage 2: Facility Representation
Stage 3: Robot & Envelope Definition
                 │
                 ▼
      [ ROBOT PREFLIGHT ]
      • Sourced requirement extraction
      • Spatial entity grounding
      • Deterministic geometry checks
      • PASS / BLOCKED / REVIEW decision
                 │
                 ▼
Stage 4: Feasibility & Simulation
                 │
                 ▼
Stage 5+: Detailed Design, OEM Configuration, Commissioning
```

Robot Preflight conceptually sits **between Stages 1–3 and Stage 4**.

Its role is to turn documented robot requirements and facility geometry into
grounded, deterministic constraints *before* committing engineering time to
physics simulation, detailed network design, or on-site commissioning.

### What Robot Preflight Does Not Replace

Robot Preflight does **not** replace:
* **Physics & Traffic Simulation**: It does not simulate dynamics, wheel slip, battery drain, or traffic contention.
* **OEM Fleet Managers**: It does not dispatch vehicles, program fleet routes, or edit OEM operational maps.
* **Navigation Stacks (Nav2 / Open-RMF)**: It does not generate global costmaps or trajectory plans.
* **Safety Certification**: It does not certify compliance with ISO 3691-4 or ANSI/RIA R15.08.

Its job is to make physical assumptions explicit and catch spatial conflicts early.

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

Today, **Robot Preflight focuses strictly on reconciling Truth 2 (the facility) and Truth 3 (the robot)**,
using manually declared context from **Truth 1 (the mission)**.

Truths 4, 5, and 6 are downstream:
* A verified constraint produced by Robot Preflight can inform simulation parameters (Truth 4).
* A verified constraint can serve as an acceptance criterion for OEM zone configuration (Truth 5).
* Robot Preflight does not monitor live telemetry (Truth 6).

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
