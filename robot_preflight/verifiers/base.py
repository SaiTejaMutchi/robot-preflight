"""Abstract base class for preflight constraint verifiers."""
from __future__ import annotations

import abc
import pathlib
from typing import Any

from ..models import PreflightResult


class BaseVerifier(abc.ABC):
    """Abstract verifier interface."""

    @property
    @abc.abstractmethod
    def constraint_type(self) -> str:
        """Constraint identifier handled by this verifier."""
        ...

    @abc.abstractmethod
    def verify(
        self,
        *,
        robot: str,
        requirement: dict[str, Any],
        facility_dir: pathlib.Path,
        config: dict[str, Any],
    ) -> PreflightResult:
        """Execute verification and produce a structured PreflightResult."""
        ...
