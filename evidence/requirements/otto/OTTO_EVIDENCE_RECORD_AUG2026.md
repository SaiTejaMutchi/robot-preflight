# OTTO 1500 machine-truth evidence record — AUG2026 revision

- Manufacturer: OTTO by Rockwell Automation
- Robot: OTTO 1500
- Document title: OTTO 1500 Spec Sheet
- Document ID: OTTO-DS001F-EN-AUG2026
- PDF metadata creation date: 2026-08-28 (Adobe InDesign 20.1, Macintosh)
- Pages: 2
- Local file: `OTTO_1500_Spec_Sheet_OTTO-DS001F-EN-AUG2026.pdf`
- PDF SHA-256: `e4d7ca82ff0fe5eafbdaafdba11ceedf9641a950bf481b1128ab0f9d1bebe339`
- PDF size: 1,674,774 bytes
- Visual evidence: `OTTO_1500_Spec_Sheet_OTTO-DS001F-EN-AUG2026_page_1.png`, `..._page_2.png` (144 DPI, rendered via poppler `pdftoppm`)
- Page-1 PNG SHA-256: `03b999f45606e1a5d0bbb430a78a3bfdf242f3f2fd40b86e5fae7c7003651df8`
- Page-2 PNG SHA-256: `8fcf7cf5034968dc06276896b5530641bb9f7a2980ac517e72c2e565e8f83798`

Verification used both `pdftotext -layout` extraction and a 144-DPI visual
rendering of both pages. Values below are transcribed from page 1 unless
noted; the sample workflow diagram and safety/automation/control tables are
on page 2.

## Constraints relevant to the existing SpatialGrid aisle-width checker

| Field | Value | Source text |
| --- | --- | --- |
| Min. aisle width, one way, no payload | 2.209 m (2,209 mm / 87 in) | `Min. Aisle Width 2,209 mm (87 in) (one way, no payload)` |
| Min. aisle width, two way, no payload | 3.570 m (3,570 mm / 146 in) | `Min. Aisle Width 3,570 mm (146 in) (two way, no payload)` |

## Revision conflict with the frozen APR2024 proof — DO NOT SILENTLY RESOLVE

[`OTTO_EVIDENCE_RECORD.md`](OTTO_EVIDENCE_RECORD.md) (document ID
`OTTO-DS001D-EN-APR2024`) froze **1.915 m** as the minimum one-way aisle
width and that value is what
[`evidence/results/otto1500_warehouse.json`](../../results/otto1500_warehouse.json)
and the Unity `AisleClearanceChecker` demo (PASS, +5.594663 m margin,
measured 7.509663 m) were run against.

The AUG2026 revision F document states a different value for the same
nominal constraint: **2.209 m**, explicitly qualified `one way, no payload`
— a qualifier the APR2024 source text did not carry. Whether this is a
genuine spec change between revisions D and F, or the APR2024 text simply
omitted the "no payload" qualifier that was always implicit, is not
determinable from either PDF alone.

**Neither existing artifact has been modified or re-run against this
value.** The APR2024-based PASS result remains historically valid *for that
document revision* and must not be presented as verified against
OTTO-DS001F-EN-AUG2026. Any future checker run against the 7.509663 m
measured span using the 2.209 m AUG2026 requirement would still PASS
(margin +5.300663 m), but that has not been executed or independently
cross-validated the way the 1.915 m run was, and must not be claimed until
it is.

Product implication: the aisle-width constraint must be keyed by
`(document_id, revision, directionality, payload_condition)`, not by robot
model alone. A source-aware system surfaces this kind of revision delta
instead of silently picking whichever number is currently in code.

## Other fields transcribed for future use (not yet wired into any checker)

- Max. capacity: 1,900 kg (4,190 lb) — includes payload and attachment, if any
- Max. speed: 2.0 m/s (4.5 mph); max docking speed: 0.3 m/s (0.7 mph); max turning speed: 1.5 rad/s (90°/s)
- Docking accuracy, standard: ±10 mm (x,y), ±10° (yaw), "repeatable to 3σ" — the σ glyph did not survive text extraction cleanly; confirmed against the page-1 PNG render, not fully OCR-verified
- Docking accuracy, precision upgrade: ±5 mm (x,y), ±10° (yaw), "repeatable to 3σ", requires OTTO High Accuracy Docking System (HADS), sold separately
- Chassis: 1,837 × 1,283 × 351 mm; mass 627 kg; ground clearance 16 mm; traversable gap 16 mm; passive rocker suspension; turns in place
- Energy: 80 Ah / 52.8 V battery; 10 hr runtime (90%→10%); 60 min charge (10%→90%); 80 A max charge rate; 3,000 full-charge-cycle life; autonomous opportunity charging default, manual charging optional
- Automation interface: 24 VDC/10 A regulated unswitched + 52.8 VDC/50 A unregulated power; 1× Ethernet, 1× USB 3.1, 1× HDMI, 1× CAN bus, 5 GPIO (2 safety-rated), 1 interface-control line; dual-channel E-Stop breakout
- Control system: 4× Intel RealSense cameras, 2× SICK Microscan3 (360° FOV), embedded 6-axis IMU, solid-state mil-spec computer with dedicated GPU, WiFi 2.4/5 GHz (802.11 a/b/g/n/ac), 2× long-range omnidirectional antennas
- Environmental: IP20; 0–40°C (32–104°F); 10–95% humidity, non-condensing
- Standards: CE marked, ANSI/ITSDF B56.5, RIA R15.08-1, ISO 12100, ISO 13849-1, ISO 3691-4, FCC Part 15 Subpart B, ICES-003, ICES-002, EN 1175-1, EN 60204-1

These fields are transcribed and hashed but not yet consumed by any
SpatialGrid checker or represented in `independent_result.json`. Treat them
as sourced reference data, not verified product claims, until a checker is
built and run against them the same way the aisle-width check was.
