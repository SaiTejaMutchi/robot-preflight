"""Smoke tests for the public robot_preflight API.

These call robot_preflight.Preflight.check() -- the same public entry point
a user would use -- not internal helpers, and they assert against the
canonical example, not a stored fixture the API reads back.
"""
from __future__ import annotations

import json
import pathlib

import pytest

from robot_preflight import Preflight
from robot_preflight.core import REPO_ROOT, _GEOMETRY_SCRIPT

TOL_M = 1e-6


def test_reference_result_reproduces():
    result = Preflight.check(
        robot="otto_1500",
        facility="examples/otto1500_warehouse",
        constraint="aisle_clearance",
    )
    assert result.decision == "PASS"
    assert result.required_m == pytest.approx(1.915, abs=TOL_M)
    assert result.available_m == pytest.approx(7.509662, abs=TOL_M)
    assert result.margin_m == pytest.approx(5.594662, abs=TOL_M)
    assert result.facility_entities == [
        "aws_robomaker_warehouse_ShelfF_01_001",
        "aws_robomaker_warehouse_ShelfD_01_001",
    ]


def test_matches_frozen_record_within_documented_tolerance():
    """Cross-check against evidence/results/otto1500_warehouse.json.

    Unity (float) and this independent Python recomputation (double) are
    documented in that file to agree to within 1 micrometer -- that is the
    tolerance used here, not slack added to make the test pass.
    """
    frozen = json.loads((REPO_ROOT / "evidence/results/otto1500_warehouse.json").read_text())
    result = Preflight.check(
        robot="otto_1500",
        facility="examples/otto1500_warehouse",
        constraint="aisle_clearance",
    )
    assert result.available_m == pytest.approx(
        frozen["measurement"]["measured_value_m"], abs=1.5e-6
    )
    assert result.decision == frozen["measurement"]["status"]


def test_public_api_does_not_read_the_frozen_result_file():
    """Guard against the API silently degrading into `return frozen_json`.

    The docstring is allowed to *mention* the frozen record filename (it
    does, to explain the relationship) -- what must never appear is code
    that opens/reads it. So this checks the non-comment, non-docstring
    lines.
    """
    import ast

    core_source = (REPO_ROOT / "robot_preflight" / "core.py").read_text()
    tree = ast.parse(core_source)
    module_docstring = ast.get_docstring(tree) or ""
    code_only = core_source.replace(module_docstring, "", 1)
    code_lines = [
        line for line in code_only.splitlines() if not line.strip().startswith("#")
    ]
    assert not any("otto1500_warehouse.json" in line for line in code_lines)
    # The geometry module it calls must be the same script that produced
    # the independent cross-validation, not a private reimplementation.
    assert _GEOMETRY_SCRIPT.name == "validate_independent_warehouse_glb.py"


def test_unresolvable_entity_raises_instead_of_returning_a_result(tmp_path: pathlib.Path):
    """A config pointing at a nonexistent entity must fail loudly, not fall
    back to a stored/expected value."""
    facility_dir = tmp_path / "otto1500_warehouse_bad_entity"
    facility_dir.mkdir()
    config = (REPO_ROOT / "examples/otto1500_warehouse/config.yaml").read_text()
    config = config.replace(
        "aws_robomaker_warehouse_ShelfF_01_001", "this_entity_does_not_exist"
    )
    (facility_dir / "config.yaml").write_text(config)

    with pytest.raises(AssertionError):
        Preflight.check(
            robot="otto_1500",
            facility=str(facility_dir),
            constraint="aisle_clearance",
        )


def test_mismatched_required_value_is_rejected(tmp_path: pathlib.Path):
    """A facility config whose required_m disagrees with the sourced
    requirement must be rejected, not silently overridden."""
    facility_dir = tmp_path / "otto1500_warehouse_bad_requirement"
    facility_dir.mkdir()
    config = (REPO_ROOT / "examples/otto1500_warehouse/config.yaml").read_text()
    config = config.replace("required_m: 1.915", "required_m: 1.5")
    (facility_dir / "config.yaml").write_text(config)

    with pytest.raises(ValueError):
        Preflight.check(
            robot="otto_1500",
            facility=str(facility_dir),
            constraint="aisle_clearance",
        )


def test_unknown_robot_constraint_pair_raises():
    with pytest.raises(KeyError):
        Preflight.check(
            robot="nonexistent_robot",
            facility="examples/otto1500_warehouse",
            constraint="aisle_clearance",
        )
