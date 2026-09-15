"""Robot Preflight: public API.

This module is a thin wrapper around the deterministic glTF geometry
computation in ``Tools/WarehouseGeometry/validate_independent_warehouse_glb.py``
-- the same script that produced the offline cross-validation recorded in
``evidence/results/otto1500_warehouse.json`` (agreeing with the Unity
Play Mode measurement to within 1 micrometer). ``Preflight.check`` imports
and calls that script's ``inspect()`` function directly; it re-parses the
facility ``.glb`` binary and recomputes node-world-space bounds on every
call. It does not read a stored result and hand it back.

Robot requirement metadata (the *value* to check against) is a small,
explicitly sourced lookup table below -- not a geometry computation. Each
entry must cite a hashed manufacturer PDF under ``evidence/requirements/``.
"""
from __future__ import annotations

import dataclasses
import importlib.util
import pathlib
from typing import Any

import yaml

REPO_ROOT = pathlib.Path(__file__).resolve().parent.parent
_GEOMETRY_SCRIPT = (
    REPO_ROOT / "Tools" / "WarehouseGeometry" / "validate_independent_warehouse_glb.py"
)


def _load_geometry_module():
    spec = importlib.util.spec_from_file_location(
        "_robot_preflight_geometry_impl", _GEOMETRY_SCRIPT
    )
    if spec is None or spec.loader is None:
        raise ImportError(f"cannot load geometry script at {_GEOMETRY_SCRIPT}")
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


_geometry = _load_geometry_module()

_AXIS_INDEX = {"X": 0, "Y": 1, "Z": 2}

# Sourced robot requirements. Each entry must trace to a hashed manufacturer
# document under evidence/requirements/. This table holds what the requirement
# *is*; it never holds a measured or computed value.
_ROBOT_REQUIREMENTS: dict[tuple[str, str], dict[str, Any]] = {
    ("otto_1500", "aisle_clearance"): {
        "required_m": 1.915,
        "tolerance_m": 0.005,
        "source": (
            "OTTO 1500 Spec Sheet, doc OTTO-DS001D-EN-APR2024, p.1: "
            "'Min. Aisle Width 1915 mm (78 in) (One Way)'. See "
            "evidence/requirements/otto/OTTO_EVIDENCE_RECORD.md. "
            "(A later document, OTTO-DS001F-EN-AUG2026, states 2.209 m for the "
            "same nominal constraint with an explicit 'no payload' qualifier -- "
            "see evidence/requirements/otto/OTTO_EVIDENCE_RECORD_AUG2026.md. This "
            "lookup intentionally keeps citing the APR2024 revision because "
            "that is the revision the frozen reference result was verified "
            "against; it is not yet re-verified against AUG2026.)"
        ),
    },
}


@dataclasses.dataclass(frozen=True)
class PreflightResult:
    decision: str
    robot: str
    constraint: str
    required_m: float
    available_m: float
    margin_m: float
    requirement_source: str
    facility_source: str
    facility_entities: list
    verification_method: str
    evidence: dict

    def __str__(self) -> str:
        lines = [
            "Robot Preflight",
            "",
            f"Robot        {self.robot}",
            f"Constraint   {self.constraint}",
            "",
            f"Required     {self.required_m:.6f} m",
            f"Available    {self.available_m:.6f} m",
            f"Margin       {self.margin_m:+.6f} m",
            "",
            f"Decision     {self.decision}",
            "",
            "Requirement source",
            self.requirement_source,
            "",
            "Facility source",
            self.facility_source,
            "",
            "Verification",
            self.verification_method,
        ]
        return "\n".join(lines)

    def to_dict(self) -> dict:
        return dataclasses.asdict(self)


def _resolve_facility_dir(facility: str | pathlib.Path) -> pathlib.Path:
    p = pathlib.Path(facility)
    if not p.is_absolute():
        p = REPO_ROOT / p
    if p.is_file() and p.suffix in (".yaml", ".yml"):
        return p.parent, p
    if p.is_dir():
        config_path = p / "config.yaml"
        if not config_path.exists():
            raise FileNotFoundError(f"no config.yaml in facility directory {p}")
        return p, config_path
    raise FileNotFoundError(f"facility path not found: {p}")


def check(*, robot: str, facility: str, constraint: str) -> PreflightResult:
    """Run one preflight check and return a structured, inspectable result.

    Re-parses the facility's glTF geometry and recomputes the measurement;
    does not read a stored/frozen result.
    """
    key = (robot, constraint)
    if key not in _ROBOT_REQUIREMENTS:
        raise KeyError(
            f"no sourced requirement for robot={robot!r} constraint={constraint!r}. "
            "Add one to robot_preflight.core._ROBOT_REQUIREMENTS with a hashed evidence citation."
        )
    requirement = _ROBOT_REQUIREMENTS[key]

    facility_dir, config_path = _resolve_facility_dir(facility)
    config = yaml.safe_load(config_path.read_text())

    if config["robot"]["id"] != robot:
        raise ValueError(
            f"facility config robot.id={config['robot']['id']!r} does not match "
            f"requested robot={robot!r}"
        )
    if config["constraint"]["type"] != constraint:
        raise ValueError(
            f"facility config constraint.type={config['constraint']['type']!r} does not "
            f"match requested constraint={constraint!r}"
        )
    config_required_m = float(config["constraint"]["required_m"])
    if abs(config_required_m - requirement["required_m"]) > 1e-9:
        raise ValueError(
            f"facility config required_m={config_required_m} does not match sourced "
            f"requirement required_m={requirement['required_m']} for {key}; "
            "refusing to run a check against a mismatched constraint."
        )

    geometry_rel = pathlib.Path(config["facility"]["geometry"])
    geometry_path = (
        geometry_rel if geometry_rel.is_absolute() else REPO_ROOT / geometry_rel
    )
    if not geometry_path.exists():
        raise FileNotFoundError(f"facility geometry not found: {geometry_path}")

    axis = config["measurement"].get("axis", "X")
    axis_index = _AXIS_INDEX[axis]
    tolerance_m = float(config["constraint"].get("tolerance_m", requirement["tolerance_m"]))

    raw = _geometry.inspect(
        str(geometry_path),
        config["measurement"]["entity_a"],
        config["measurement"]["entity_b"],
        axis_index,
        requirement["required_m"],
        tolerance_m,
    )

    margin_m = round(raw["available_m"] - raw["required_m"], 6)

    return PreflightResult(
        decision=raw["status"],
        robot=robot,
        constraint=constraint,
        required_m=raw["required_m"],
        available_m=raw["available_m"],
        margin_m=margin_m,
        requirement_source=requirement["source"],
        facility_source=config["facility"].get("source", "unspecified"),
        facility_entities=[raw["boundary_a"], raw["boundary_b"]],
        verification_method=(
            "Deterministic geometry: nearest-face world-space gap between the two "
            "named entities' renderer bounds along the configured axis, computed by "
            "re-parsing the facility .glb (Tools/WarehouseGeometry/"
            "validate_independent_warehouse_glb.py:inspect, invoked directly by "
            "this module -- not a stored value)."
        ),
        evidence={
            "geometry_file": str(geometry_path.relative_to(REPO_ROOT)),
            "geometry_file_bytes": raw["bytes"],
            "geometry_file_nodes": raw["nodes"],
            "geometry_file_triangles": raw["unique_mesh_triangles"],
            "axis": axis,
            "tolerance_m": tolerance_m,
            "facility_config": str(config_path.relative_to(REPO_ROOT)),
        },
    )


class Preflight:
    """Namespace for the public preflight entry point."""

    check = staticmethod(check)
