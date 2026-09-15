"""Robot Preflight: public API.

This module provides the central entry point for preflight verification.
Verifiers are managed via an extensible registry, dispatching by constraint
type.

Verifier #1 (aisle_clearance) executes deterministic glTF geometry computation
via ``Tools/WarehouseGeometry/validate_independent_warehouse_glb.py`` -- the same
script that produced the offline cross-validation recorded in the reference run
(agreeing with the Unity Play Mode measurement to within 1 micrometer).
``Preflight.check`` calls the verifier directly, re-parsing the facility ``.glb``
binary and recomputing node-world-space bounds on every call. It does not read a
stored result and hand it back.

Robot requirement metadata (the *value* to check against) is an explicitly
sourced lookup table. Each entry must cite a hashed manufacturer PDF under
``evidence/requirements/``.
"""
from __future__ import annotations

import importlib.util
import pathlib
from typing import Any

import yaml

from .models import ClearanceStatus, PreflightResult
from .verifiers.aisle_clearance import AisleClearanceVerifier
from .verifiers.registry import VerifierRegistry

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

# Verifier registry setup
_REGISTRY = VerifierRegistry()
_REGISTRY.register(AisleClearanceVerifier(_geometry, REPO_ROOT))


def _resolve_facility_dir(facility: str | pathlib.Path) -> tuple[pathlib.Path, pathlib.Path]:
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

    verifier = _REGISTRY.get(constraint)
    if verifier is None:
        raise NotImplementedError(
            f"no verifier registered for constraint {constraint!r}. "
            f"Available verifiers: {_REGISTRY.registered_types()}"
        )

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

    return verifier.verify(
        robot=robot,
        requirement=requirement,
        facility_dir=facility_dir,
        config=config,
    )


class Preflight:
    """Namespace for the public preflight entry point."""

    check = staticmethod(check)
    registry = _REGISTRY
