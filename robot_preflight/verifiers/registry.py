"""Registry of pluggable constraint verifiers."""
from __future__ import annotations

from typing import Dict, Optional
from .base import BaseVerifier


class VerifierRegistry:
    """Registry mapping constraint types to active verifiers."""

    def __init__(self) -> None:
        self._verifiers: dict[str, BaseVerifier] = {}

    def register(self, verifier: BaseVerifier) -> None:
        self._verifiers[verifier.constraint_type] = verifier

    def get(self, constraint_type: str) -> Optional[BaseVerifier]:
        return self._verifiers.get(constraint_type)

    def is_supported(self, constraint_type: str) -> bool:
        return constraint_type in self._verifiers

    def registered_types(self) -> list[str]:
        return sorted(self._verifiers.keys())
