"""Data models and result structures for Robot Preflight."""
from __future__ import annotations

import dataclasses
from enum import Enum
from typing import Any, List, Dict


class ClearanceStatus(str, Enum):
    PASS = "PASS"
    BLOCKED = "BLOCKED"
    REVIEW = "REVIEW"


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
