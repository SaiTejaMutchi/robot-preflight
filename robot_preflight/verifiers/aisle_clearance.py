"""Verifier #1: Selected-span aisle clearance verifier."""
from __future__ import annotations

import pathlib
from typing import Any

from ..models import PreflightResult
from .base import BaseVerifier

_AXIS_INDEX = {"X": 0, "Y": 1, "Z": 2}


class AisleClearanceVerifier(BaseVerifier):
    """Verifies nearest-face clearance between two facility boundary entities."""

    def __init__(self, geometry_module: Any, repo_root: pathlib.Path) -> None:
        self._geometry = geometry_module
        self._repo_root = repo_root

    @property
    def constraint_type(self) -> str:
        return "aisle_clearance"

    def verify(
        self,
        *,
        robot: str,
        requirement: dict[str, Any],
        facility_dir: pathlib.Path,
        config: dict[str, Any],
    ) -> PreflightResult:
        geometry_rel = pathlib.Path(config["facility"]["geometry"])
        geometry_path = (
            geometry_rel if geometry_rel.is_absolute() else self._repo_root / geometry_rel
        )
        if not geometry_path.exists():
            raise FileNotFoundError(f"facility geometry not found: {geometry_path}")

        axis = config["measurement"].get("axis", "X")
        if axis not in _AXIS_INDEX:
            raise ValueError(f"unknown measurement axis {axis!r}; expected one of {list(_AXIS_INDEX.keys())}")
        axis_index = _AXIS_INDEX[axis]
        tolerance_m = float(config["constraint"].get("tolerance_m", requirement["tolerance_m"]))

        raw = self._geometry.inspect(
            str(geometry_path),
            config["measurement"]["entity_a"],
            config["measurement"]["entity_b"],
            axis_index,
            requirement["required_m"],
            tolerance_m,
        )

        margin_m = round(raw["available_m"] - raw["required_m"], 6)

        config_path = facility_dir / "config.yaml"
        return PreflightResult(
            decision=raw["status"],
            robot=robot,
            constraint=self.constraint_type,
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
                "geometry_file": str(geometry_path.relative_to(self._repo_root)),
                "geometry_file_bytes": raw["bytes"],
                "geometry_file_nodes": raw["nodes"],
                "geometry_file_triangles": raw["unique_mesh_triangles"],
                "axis": axis,
                "tolerance_m": tolerance_m,
                "facility_config": str(config_path.relative_to(self._repo_root)),
            },
        )
