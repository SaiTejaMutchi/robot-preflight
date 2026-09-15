# Workflow context

Why this repository exists, in more detail than the README's short version.

## AMR deployment is a staged process, not one system

Deploying an autonomous mobile robot (AMR) into a real facility moves
through a sequence of stages, roughly:

1. mission and throughput requirements
2. facility representation (CAD, scans, layouts)
3. robot, attachment, and payload definition
4. feasibility study and simulation
5. detailed deployment design
6. site preparation
7. OEM fleet-manager and map configuration
8. controls / PLC / WMS / MES integration
9. commissioning
10. workflow validation and stabilization
11. production operations
12. continuous improvement

No single publicly established system of record spans all of this. Each
stage tends to use its own representation of "the same" deployment, owned
by different tools and often different vendors or teams. Simulation can
force a redesign. Commissioning tests the digital assumptions against the
physical site and frequently finds they disagree. Stabilization then
changes maps, zones, or workflows based on what actually happened on the
floor. Deployment is iterative by nature, not a single hand-off.

## Six truths about the same deployment

At any given time, a deployment is described by several distinct
representations, which are not automatically kept consistent with each
other:

| Truth | Typical representation |
|---|---|
| What the operation needs | workflows, endpoints, throughput |
| What the facility contains | CAD, maps, scans, layouts |
| What the robot can do | robot, attachment, payload, constraints |
| What simulation predicts | routes, fleet size, traffic, throughput |
| What the fleet system is configured to do | zones, endpoints, workflows, rules |
| What actually happens | travel, docking, faults, interventions |

The recurring technical problem is reconciliation: keeping these
representations consistent as the project moves forward, and catching it
explicitly when they disagree rather than assuming they agree.

Some of the highest-value handoffs in this chain are exactly where
disagreement tends to hide:

- customer requirement → engineering mission
- facility drawing or scan → deployment layout
- robot documentation → engineering constraints
- simulation → detailed configuration
- detailed design → fleet manager
- fleet configuration → robot / controls
- commissioning observation → revised configuration
- production telemetry → engineering improvement

## Where Robot Preflight fits

Robot Preflight currently operates between exactly two of these truths:

**what the facility contains** and **what the robot can do**.

It takes a robot's documented physical requirement (from a manufacturer
specification) and a facility's geometry (from CAD/scan-derived data), and
produces one deterministic, inspectable answer to a narrow question: does
this specific requirement agree with this specific geometry?

It does not today ingest mission/throughput requirements, run simulation,
configure a fleet manager, integrate with PLC/WMS/MES, or observe
commissioning. It does not replace the tools that already exist for those
stages. It sits earlier, and a verified constraint produced here could
later become an input to simulation, configuration, or commissioning
preparation — but that hand-off does not exist yet. See
[`limitations.md`](limitations.md) for the exact current/roadmap boundary.

## What the current public evidence supports, and what remains unknown

Supported by evidence in this repository: one facility-geometry-vs-robot-
requirement reconciliation, computed twice independently, for one robot and
one constraint. See [`verification.md`](verification.md).

Not supported by anything in this repository: that this generalizes cleanly
to other constraints, other robots, other facility data formats, or other
stages of the deployment lifecycle above. That generalization is the
larger technical direction, not a current claim.
