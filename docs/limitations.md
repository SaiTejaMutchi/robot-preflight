# Limitations

Robot Preflight's current proof is one verified selected-span compatibility
check. This document states plainly what that does and does not cover, and
tracks known open discrepancies rather than reconciling them silently.

## What the current result proves

The selected warehouse span (measured between two named shelf entities in
an independently sourced AWS RoboMaker warehouse scene) satisfies the
published OTTO 1500 minimum one-way aisle-width requirement.

## What it does not prove

- whole-facility readiness
- turning clearance
- docking compatibility
- doorway clearance
- route feasibility
- throughput
- safety certification
- deployment approval
- universal robot support
- universal geometry/CAD format support
- customer demand
- planner integration
- live fleet-manager writeback

A PASS from this check is a statement about one constraint, for one robot,
in one facility, along one axis. It is not a deployment sign-off.

## Evidence tiers

Not every number in this repository carries the same weight. Three tiers,
used consistently:

- **Verified** — evidence-backed, exercised, and reproducible: the OTTO
  1500 aisle-clearance check against the frozen AWS warehouse example.
- **Implemented** — code exists and runs, but hasn't been independently
  re-verified the same way (e.g., the general robot/constraint/facility
  config format works beyond the one shipped example, but no second
  example has been built and evidenced).
- **Planned** — roadmap only, not built: additional constraints, multiple
  robot configurations, planner integration, requalification triggers.

IMPLEMENTED does not mean VERIFIED. Anything not explicitly marked verified
should be read as implemented-but-unverified or planned.

## Known open discrepancy: OTTO document revision

A later OTTO 1500 manufacturer document (`OTTO-DS001F-EN-AUG2026`) states a
different one-way aisle-width requirement — **2.209 m**, with an explicit
"no payload" qualifier the earlier `OTTO-DS001D-EN-APR2024` document's text
did not carry — for the same nominal constraint used in the verified PASS
result above (which used 1.915 m).

This is **not resolved**. The frozen PASS result has not been re-run
against the newer value, and the SDK's requirement lookup still cites the
APR2024 revision deliberately, not by oversight. Full evidence and analysis:
[`../evidence/requirements/otto/OTTO_EVIDENCE_RECORD_AUG2026.md`](../evidence/requirements/otto/OTTO_EVIDENCE_RECORD_AUG2026.md).

This is exactly the kind of reconciliation problem described in
[`workflow-context.md`](workflow-context.md): different documents claim to
be the same fact, and a serious system has to surface that conflict rather
than pick one silently.

## Known internal correction (evidence hygiene)

`evidence/results/otto1500_warehouse.json`'s recorded facility-geometry
hash was found, during SDK construction, to be stale: the underlying `.glb`
was legitimately expanded by a later commit (adding walls/roof/floor/extra
shelves for full-scene visualization) whose own commit message documents a
regression re-run confirming the measurement was unaffected to six decimal
places — but the JSON's hash field was never updated to match. This was
fixed additively (a `superseding_file_state` field was added; the original
hash was preserved, not overwritten) rather than silently changed. See that
file's `environment_source.superseding_file_state` for the full trail.
