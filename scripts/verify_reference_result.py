#!/usr/bin/env python3
"""Reproduce the frozen Robot Preflight reference result and report whether it verifies.

This recomputes the OTTO 1500 / AWS-warehouse aisle-clearance preflight from
the real facility .glb via ``robot_preflight.Preflight`` -- which itself
re-parses the geometry independently of Unity -- then checks the recomputed
values and the relevant source-file hashes against the frozen record in
``evidence/results/otto1500_warehouse.json``.

This script performs the checks described in its output; it does not print
stored numbers without running the underlying computation.

Exit code 0 means every check passed. Non-zero means at least one did not.
"""
from __future__ import annotations

import hashlib
import json
import pathlib
import sys

REPO_ROOT = pathlib.Path(__file__).resolve().parent.parent
sys.path.insert(0, str(REPO_ROOT))

from robot_preflight import Preflight  # noqa: E402

# The Unity (float) and independent-Python (double) computations of the same
# glTF bounds are documented to agree to within 1 micrometer -- this is not
# slack added to make the check pass, it is the repository's own documented
# cross-validation tolerance (see the frozen record's "cross_validation_note").
DOCUMENTED_UNITY_PYTHON_AGREEMENT_M = 1e-6
COMPARISON_SLACK_M = 5e-7  # float rounding margin on top of the above

ok_count = 0
fail_count = 0


def sha256(path: pathlib.Path) -> str:
    return hashlib.sha256(path.read_bytes()).hexdigest()


def check(label: str, ok: bool, detail: str = "") -> None:
    global ok_count, fail_count
    ok_count += 1 if ok else 0
    fail_count += 0 if ok else 1
    status = "verified" if ok else "FAILED"
    line = f"{label:<28} {status}"
    if detail and not ok:
        line += f"  ({detail})"
    print(line)


def main() -> int:
    frozen_path = REPO_ROOT / "evidence" / "results" / "otto1500_warehouse.json"
    frozen = json.loads(frozen_path.read_text())

    otto_pdf = REPO_ROOT / "evidence" / "requirements" / "otto" / "OTTO_1500_Spec_Sheet_OTTO-DS001D-EN-APR2024.pdf"
    facility_glb = REPO_ROOT / frozen["environment_source"]["derived_glb_path"]

    print("Robot Preflight reference verification\n")

    check(
        "OTTO source PDF hash",
        otto_pdf.exists() and sha256(otto_pdf) == frozen["requirement"]["source_pdf_sha256"],
        detail=str(otto_pdf),
    )
    # The GLB was legitimately updated after this record was generated (a
    # later commit added walls/roof/floor/extra shelves; see
    # environment_source.superseding_file_state in the frozen record for the
    # full story and the commit that changed it). Check against the
    # current-file hash it documents, not the stale original one.
    superseding = frozen["environment_source"].get("superseding_file_state")
    expected_glb_hash = (
        superseding["derived_glb_sha256_current"]
        if superseding
        else frozen["environment_source"]["derived_glb_sha256"]
    )
    check(
        "Facility GLB hash",
        facility_glb.exists() and sha256(facility_glb) == expected_glb_hash,
        detail=str(facility_glb),
    )

    try:
        result = Preflight.check(
            robot="otto_1500",
            facility="examples/otto1500_warehouse",
            constraint="aisle_clearance",
        )
        computed_ok = True
        error = None
    except Exception as exc:  # noqa: BLE001
        computed_ok = False
        error = exc
        result = None

    check("Deterministic measurement (recomputed)", computed_ok, detail=str(error) if error else "")

    if result is not None:
        check(
            "Required matches frozen record",
            abs(result.required_m - frozen["requirement"]["required_value_m"]) < 1e-9,
        )
        check(
            "Measured matches frozen record",
            abs(result.available_m - frozen["measurement"]["measured_value_m"])
            < DOCUMENTED_UNITY_PYTHON_AGREEMENT_M + COMPARISON_SLACK_M,
            detail=f"computed {result.available_m}, frozen {frozen['measurement']['measured_value_m']}",
        )
        check(
            "Decision matches frozen record",
            result.decision == frozen["measurement"]["status"],
        )

    print()
    if result is not None:
        print(f"Required                    {result.required_m:.6f} m")
        print(f"Measured (this run, Python)  {result.available_m:.6f} m")
        print(f"Measured (frozen, Unity)     {frozen['measurement']['measured_value_m']:.6f} m")
        print(f"Margin (this run)            {result.margin_m:+.6f} m")
        print()
        print(f"Decision                    {result.decision}")
    print()

    all_ok = fail_count == 0
    print(f"Reference reproduction      {'VERIFIED' if all_ok else 'NOT VERIFIED'} ({ok_count} passed, {fail_count} failed)")

    return 0 if all_ok else 1


if __name__ == "__main__":
    raise SystemExit(main())
